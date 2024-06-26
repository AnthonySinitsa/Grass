using UnityEngine;

public class ModelGrass : MonoBehaviour
{
    public ComputeShader grassComputeShader;
    public ComputeShader windComputeShader;
    public Material grassMaterial;
    public Mesh grassMesh;
    [Header("Grass")]
    public int numChunks = 5;
    public int chunkDensity = 100;
    [Range(0.0f, 1.0f)]
    public float tilt = 0.0f;
    [Range(0.0f, 1.0f)]
    public float bend = 0.0f;
    public float width = 0.0f;
    public float voronoiScale = 1.0f;
    public float chunkSize = 10.0f;
    public bool grassUpdate = false;
    [Header("Wind")]
    public float windSpeed = 1.0f;
    public float frequency = 1.0f;
    public float windStrength = 1.0f;

    private int seed = 12345;
    private ComputeBuffer grassBuffer, argsBuffer, windBuffer;
    private int grassKernelHandle, windKernelHandle;

    void Start()
    {
        grassKernelHandle = grassComputeShader.FindKernel("CSMain");
        windKernelHandle = windComputeShader.FindKernel("CSMain");
        InitializeBuffers();
        GenerateGrass();
    }

    void InitializeBuffers()
    {
        OnDestroy();

        int totalGrassBlades = numChunks * numChunks * chunkDensity * chunkDensity;

        grassBuffer = new ComputeBuffer(totalGrassBlades, sizeof(float) * 7);
        windBuffer = new ComputeBuffer(totalGrassBlades, sizeof(float) * 3);

        grassComputeShader.SetBuffer(grassKernelHandle, "grassBuffer", grassBuffer);
        windComputeShader.SetBuffer(windKernelHandle, "windBuffer", windBuffer);

        uint[] args = new uint[5] { grassMesh.GetIndexCount(0), (uint)(totalGrassBlades), 0, 0, 0 };
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
    }

    void GenerateGrass()
    {
        grassComputeShader.SetInt("numChunks", numChunks);
        grassComputeShader.SetInt("chunkDensity", chunkDensity);
        grassComputeShader.SetFloat("tilt", tilt);
        grassComputeShader.SetFloat("bend", bend);
        grassComputeShader.SetFloat("width", width);
        grassComputeShader.SetFloat("voronoiScale", voronoiScale);
        grassComputeShader.SetFloat("chunkSize", chunkSize);
        grassComputeShader.SetInt("seed", seed);
        int totalGrassBlades = numChunks * numChunks * chunkDensity * chunkDensity;
        grassComputeShader.Dispatch(grassKernelHandle, Mathf.CeilToInt(totalGrassBlades / 10.0f), 1, 1);
    }

    void Update()
    {
        UpdateWind();

        if (grassUpdate)
        {
            InitializeBuffers();
            GenerateGrass();
            grassUpdate = false;
        }

        grassMaterial.SetBuffer("grassBuffer", grassBuffer);
        grassMaterial.SetBuffer("windBuffer", windBuffer);

        float boundsSize = numChunks * chunkSize * 5;

        Graphics.DrawMeshInstancedIndirect(
            grassMesh, 0, grassMaterial, new Bounds(
                Vector3.zero, new Vector3(
                    boundsSize, 10.0f, boundsSize
                )
            ), argsBuffer
        );
    }

    void UpdateWind()
    {
        windComputeShader.SetFloat("windSpeed", windSpeed);
        windComputeShader.SetFloat("frequency", frequency);
        windComputeShader.SetFloat("windStrength", windStrength);
        windComputeShader.SetFloat("time", Time.time);

        int totalGrassBlades = numChunks * numChunks * chunkDensity * chunkDensity;
        windComputeShader.Dispatch(windKernelHandle, Mathf.CeilToInt(totalGrassBlades), 1, 1);
    }

    void OnDestroy()
    {
        if (grassBuffer != null)
        {
            grassBuffer.Release();
            grassBuffer = null;
        }
        if (argsBuffer != null)
        {
            argsBuffer.Release();
            argsBuffer = null;
        }
        if (windBuffer != null)
        {
            windBuffer.Release();
            windBuffer = null;
        }
    }
}