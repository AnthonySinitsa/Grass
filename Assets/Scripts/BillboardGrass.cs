using UnityEngine;

public class BillboardGrass : MonoBehaviour
{
    public Texture2D grassTexture;
    public ComputeShader grassComputeShader;
    public Mesh grassMesh;
    public Material grassMaterial;

    public int gridWidth = 100; // Width of the grid in meters
    public int gridHeight = 100; // Height of the grid in meters

    private int grassCount;
    private ComputeBuffer grassBuffer;
    private ComputeBuffer argsBuffer;

    struct GrassData
    {
        public Vector3 position;
        public Vector2 uv;
    }

    void Start()
    {
        InitializeGrass();
    }

    void InitializeGrass()
    {
        Debug.Log("Initializing grass...");

        grassCount = gridWidth * gridHeight;
        GrassData[] grassDataArray = new GrassData[grassCount];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                int index = x * gridHeight + z;
                grassDataArray[index].position = new Vector3(x, 0, z);
                grassDataArray[index].uv = new Vector2(0, 0); // UVs will be handled by shader
            }
        }

        grassBuffer = new ComputeBuffer(grassCount, sizeof(float) * 5);
        grassBuffer.SetData(grassDataArray);

        int kernelHandle = grassComputeShader.FindKernel("CSMain");
        grassComputeShader.SetBuffer(kernelHandle, "grassBuffer", grassBuffer);
        grassComputeShader.Dispatch(kernelHandle, grassCount / 10, 1, 1);

        grassMaterial.SetTexture("_MainTex", grassTexture);
        grassMaterial.SetBuffer("grassBuffer", grassBuffer);

        uint[] args = new uint[5] { grassMesh.GetIndexCount(0), (uint)grassCount, 0, 0, 0 };
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        Debug.Log("Grass initialized.");
    }

    void Update()
    {
        Graphics.DrawMeshInstancedIndirect(grassMesh, 0, grassMaterial, new Bounds(Vector3.zero, new Vector3(gridWidth, 10, gridHeight)), argsBuffer);
    }

    void OnDestroy()
    {
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
