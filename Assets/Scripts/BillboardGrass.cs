using UnityEngine;

public class BillboardGrass : MonoBehaviour
{
    public Mesh grassMesh; // Reference to the grass mesh (a single quad)
    public Material grassMaterial; // Reference to the grass material using the shader
    public ComputeShader grassComputeShader; // Reference to the compute shader
    public int gridWidth = 100; // Width of the grid in meters (total width, not just positive direction)
    public int gridHeight = 100; // Height of the grid in meters (total height, not just positive direction)
    public float spacing = 1f; // Distance between each grass instance

    private ComputeBuffer grassBuffer; // Buffer to hold grass data (position and rotation)
    private ComputeBuffer argsBuffer; // Buffer to hold draw arguments for indirect rendering

    void Start()
    {
        InitializeGrass();
    }

    void InitializeGrass()
    {
        Debug.Log("Initializing grass...");

        int grassCount = gridWidth * gridHeight * 3; // Total number of grass instances (3 quads per instance)

        // Create and fill the grass buffer
        grassBuffer = new ComputeBuffer(grassCount, sizeof(float) * 8);
        grassMaterial.SetBuffer("grassBuffer", grassBuffer);

        // Set up the compute shader
        grassComputeShader.SetInt("gridWidth", gridWidth);
        grassComputeShader.SetInt("gridHeight", gridHeight);
        grassComputeShader.SetFloat("spacing", spacing);
        grassComputeShader.SetBuffer(0, "grassBuffer", grassBuffer);

        // Dispatch the compute shader
        int threadGroups = Mathf.CeilToInt(grassCount / 10.0f);
        grassComputeShader.Dispatch(0, threadGroups, 1, 1);

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
        if (grassBuffer != null)
        {
            grassBuffer.Release();
        }
        if (argsBuffer != null)
        {
            argsBuffer.Release();
        }
    }
}
