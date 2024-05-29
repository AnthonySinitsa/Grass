using UnityEngine;

public class BillboardGrass : MonoBehaviour
{
    public Mesh grassMesh; // Reference to the grass mesh (with 3 intersecting quads)
    public Material grassMaterial; // Reference to the grass material using the shader
    public int gridWidth = 100; // Width of the grid in meters
    public int gridHeight = 100; // Height of the grid in meters
    public float spacing = 1f; // Distance between each grass instance

    private ComputeBuffer positionBuffer; // Buffer to hold position data for each grass instance
    private ComputeBuffer argsBuffer; // Buffer to hold draw arguments for indirect rendering

    void Start()
    {
        InitializeGrass();
    }

    void InitializeGrass()
    {
        Debug.Log("Initializing grass...");

        int grassCount = gridWidth * gridHeight; // Total number of grass instances
        Vector4[] positions = new Vector4[grassCount]; // Array to hold the positions

        // Fill the positions array with the positions of each grass instance
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                int index = x * gridHeight + z;
                positions[index] = new Vector4(x * spacing, 0, z * spacing, 0);
            }
        }

        // Create and fill the position buffer
        positionBuffer = new ComputeBuffer(grassCount, sizeof(float) * 4);
        positionBuffer.SetData(positions);
        grassMaterial.SetBuffer("positionBuffer", positionBuffer);

        // Set up the draw arguments buffer
        uint[] args = new uint[5] { grassMesh.GetIndexCount(0), (uint)grassCount, 0, 0, 0 };
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        Debug.Log("Grass initialized.");
    }

    void Update()
    {
        // Draw the grass instances using GPU instancing
        Graphics.DrawMeshInstancedIndirect(grassMesh, 0, grassMaterial, new Bounds(Vector3.zero, new Vector3(gridWidth * spacing, 10, gridHeight * spacing)), argsBuffer);
    }

    void OnDestroy()
    {
        // Release the buffers to free up GPU memory
        if (positionBuffer != null)
        {
            positionBuffer.Release();
        }
        if (argsBuffer != null)
        {
            argsBuffer.Release();
        }
    }
}
