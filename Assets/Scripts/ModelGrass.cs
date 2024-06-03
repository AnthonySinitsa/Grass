using UnityEngine;

public class ModelGrass : MonoBehaviour
{
    public ComputeShader grassComputeShader;
    public Material grassMaterial;
    public Mesh grassMesh;

    public int fieldSize = 100; // size of the field in units
    public int chunkDensity = 1000; // number of grass chunks in the field
    public int numChunks = 5; // number of chunks to spawn

    private ComputeBuffer grassBuffer, argsBuffer;
    private int kernelHandle;

    void Start()
    {
        kernelHandle = grassComputeShader.FindKernel("CSMain");

        grassBuffer = new ComputeBuffer(chunkDensity * numChunks, sizeof(float) * 3);
        grassComputeShader.SetBuffer(kernelHandle, "grassBuffer", grassBuffer);

        uint[] args = new uint[5] { grassMesh.GetIndexCount(0), (uint)(chunkDensity * numChunks), 0, 0, 0 };
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        GenerateGrass();
    }

    void GenerateGrass()
    {
        grassComputeShader.SetInt("fieldSize", fieldSize);
        grassComputeShader.SetInt("chunkDensity", chunkDensity);
        grassComputeShader.SetInt("numChunks", numChunks);
        grassComputeShader.Dispatch(kernelHandle, Mathf.CeilToInt(chunkDensity * numChunks / 10.0f), 1, 1);
    }

    void Update()
    {
        grassMaterial.SetBuffer("grassBuffer", grassBuffer);
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
        grassBuffer.Release();
        argsBuffer.Release();
    }
}