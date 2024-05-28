using UnityEngine;

public class BillboardGrass : MonoBehaviour
{
    public Mesh grassMesh;
    public Material grassMaterial;
    public int gridWidth = 100; // Width of the grid in meters
    public int gridHeight = 100; // Height of the grid in meters
    public float spacing = 1f; // Distance between each grass instance

    private ComputeBuffer positionBuffer;
    private ComputeBuffer argsBuffer;

    void Start()
    {
        InitializeGrass();
    }

    void InitializeGrass()
    {
        Debug.Log("Initializing grass...");

        int grassCount = gridWidth * gridHeight;
        Vector4[] positions = new Vector4[grassCount];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                int index = x * gridHeight + z;
                positions[index] = new Vector4(x * spacing, 0, z * spacing, 0);
            }
        }

        positionBuffer = new ComputeBuffer(grassCount, sizeof(float) * 4);
        positionBuffer.SetData(positions);
        grassMaterial.SetBuffer("positionBuffer", positionBuffer);

        uint[] args = new uint[5] { grassMesh.GetIndexCount(0), (uint)grassCount, 0, 0, 0 };
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        Debug.Log("Grass initialized.");
    }

    void Update()
    {
        Graphics.DrawMeshInstancedIndirect(grassMesh, 0, grassMaterial, new Bounds(Vector3.zero, new Vector3(gridWidth * spacing, 10, gridHeight * spacing)), argsBuffer);
    }

    void OnDestroy()
    {
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
