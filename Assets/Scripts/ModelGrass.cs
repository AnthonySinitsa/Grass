using UnityEngine;

public class ModelGrass : MonoBehaviour
{
    public ComputeShader grassComputeShader;
    public Material grassMaterial;
    public Mesh grassMesh;

    public int numChunks = 5;
    public int chunkDensity = 100;
    public float chunkSize = 10.0f;
    public bool grassUpdate = false;

    private ComputeBuffer grassBuffer, argsBuffer;
    private int kernelHandle;

    void Start()
    {
        kernelHandle = grassComputeShader.FindKernel("CSMain");

        int totalGrassBlades = numChunks * numChunks * chunkDensity * chunkDensity;
        grassBuffer = new ComputeBuffer(totalGrassBlades, sizeof(float) * 3);
        grassComputeShader.SetBuffer(kernelHandle, "grassBuffer", grassBuffer);

        uint[] args = new uint[5] { grassMesh.GetIndexCount(0), (uint)(totalGrassBlades), 0, 0, 0 };
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        GenerateGrass();
    }

    void GenerateGrass()
    {
        grassComputeShader.SetInt("numChunks", numChunks);
        grassComputeShader.SetInt("chunkDensity", chunkDensity);
        grassComputeShader.SetFloat("chunkSize", chunkSize);
        int totalGrassBlades = numChunks * numChunks * chunkDensity * chunkDensity;
        grassComputeShader.Dispatch(kernelHandle, Mathf.CeilToInt(totalGrassBlades / 10.0f), 1, 1);
    }

    void Update()
    {
        if (grassUpdate)
        {
            Start();
            grassUpdate = false;
        }

        grassMaterial.SetBuffer("grassBuffer", grassBuffer);

        float boundsSize = numChunks * chunkSize;

        Graphics.DrawMeshInstancedIndirect(
            grassMesh, 0, grassMaterial, new Bounds(
                Vector3.zero, new Vector3(
                    boundsSize, 10.0f, boundsSize
                )
            ), argsBuffer
        );
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