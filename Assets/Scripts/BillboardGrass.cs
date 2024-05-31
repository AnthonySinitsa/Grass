using UnityEngine;

public class BillboardGrass : MonoBehaviour
{
    public Mesh grassMesh;
    public Material grassMaterial;
    public ComputeShader grassComputeShader;
    public int gridWidth = 100;
    public int gridHeight = 100;
    public float displacementStrength = 200.0f;
    public float spacing = 1f;
    public float frequency = 0.1f;

    private float density = 0.5f;

    private ComputeBuffer grassBuffer, argsBuffer; // Buffer to hold grass data (position and rotation)

    void Start()
    {
        InitializeGrass();
    }

    void InitializeGrass()
    {
        Debug.Log("Initializing grass...");
        
        int grassCount  = gridWidth * gridHeight * 3;

        // Create and fill the grass buffer
        grassBuffer = new ComputeBuffer(grassCount, sizeof(float) * 8);
        grassMaterial.SetBuffer("grassBuffer", grassBuffer);

        int resolution = gridWidth * gridHeight;

        // Set up the compute shader
        grassComputeShader.SetInt("_Dimension", resolution);
        grassComputeShader.SetFloat("_Spacing", spacing);
        grassComputeShader.SetInt("_GridWidth", gridWidth);
        grassComputeShader.SetInt("_GridHeight", gridHeight);
        grassComputeShader.SetFloat("_DisplacementStrength", displacementStrength);
        grassComputeShader.SetFloat("_Density", density);
        grassComputeShader.SetFloat("_Frequency", frequency);
        grassComputeShader.SetBuffer(0, "_GrassBuffer", grassBuffer);

        // Dispatch the compute shader
        int threadGroups = Mathf.CeilToInt(grassCount / 10.0f);
        grassComputeShader.Dispatch(0, threadGroups, 1, 1);

        // Set up the draw arguments buffer
        uint[] args = new uint[5] { grassMesh.GetIndexCount(0), (uint)grassCount, 0, 0, 0 };
        argsBuffer = 
            new ComputeBuffer(
                1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments
            );
        argsBuffer.SetData(args);

        Debug.Log("Grass initialized.");
    }

    void Update()
    {
        // Draw the grass instances using GPU instancing
        Graphics.DrawMeshInstancedIndirect(
            grassMesh, 0, grassMaterial, new Bounds(
                Vector3.zero, new Vector3(
                    1000.0f, 200.0f, 500.0f
                )
            ), argsBuffer
        );
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
