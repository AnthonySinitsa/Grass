using UnityEngine;

public class BillboardGrass : MonoBehaviour
{
    public Mesh grassMesh; // Reference to the grass mesh (a single quad)
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

        int grassCount = gridWidth * gridHeight * 3; // Total number of grass instances (3 quads per instance)
        Vector4[] positions = new Vector4[grassCount];
        Vector4[] rotations = new Vector4[grassCount];

        int index = 0;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 basePosition = new Vector3(x * spacing, 0, z * spacing);

                // First quad (no rotation)
                positions[index] = new Vector4(basePosition.x, basePosition.y, basePosition.z, 0);
                rotations[index] = Quaternion.Euler(0, 0, 0).eulerAngles;
                index++;

                // Second quad (60 degrees rotation)
                positions[index] = new Vector4(basePosition.x, basePosition.y, basePosition.z, 0);
                rotations[index] = Quaternion.Euler(0, 60, 0).eulerAngles;
                index++;

                // Third quad (-60 degrees rotation)
                positions[index] = new Vector4(basePosition.x, basePosition.y, basePosition.z, 0);
                rotations[index] = Quaternion.Euler(0, -60, 0).eulerAngles;
                index++;
            }
        }

        // Create and fill the position buffer
        positionBuffer = new ComputeBuffer(grassCount, sizeof(float) * 4);
        positionBuffer.SetData(positions);
        grassMaterial.SetBuffer("positionBuffer", positionBuffer);

        // Create and fill the rotation buffer
        var rotationBuffer = new ComputeBuffer(grassCount, sizeof(float) * 4);
        rotationBuffer.SetData(rotations);
        grassMaterial.SetBuffer("rotationBuffer", rotationBuffer);

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
