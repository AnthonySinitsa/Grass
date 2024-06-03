using UnityEngine;

public class ModelGrass : MonoBehaviour
{
    public ComputeShader grassComputeShader;
    public Material grassMaterial;
    public Mesh grassMesh;

    public int chunkSize = 10;
    public int grassDensity = 1000;

    private ComputeBuffer grassBuffer;
    private ComputeBuffer argsBuffer;
    private int kernelHandle;

    struct GrassBlade
    {
        public Vector3 position;
    }

    void Start()
    {
        kernelHandle = grassComputeShader.FindKernel("CSMain");

        grassBuffer = new ComputeBuffer(grassDensity, sizeof(float) * 3);
        grassComputeShader.SetBuffer(kernelHandle, "grassBuffer", grassBuffer);

        uint[] args = new uint[5] { grassMesh.GetIndexCount(0), (uint)grassDensity, 0, 0, 0 };
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        GenerateGrass();
    }

    void GenerateGrass()
    {
        grassComputeShader.SetInt("chunkSize", chunkSize);
        grassComputeShader.SetInt("grassDensity", grassDensity);
        grassComputeShader.Dispatch(kernelHandle, Mathf.CeilToInt(grassDensity / 10.0f), 1, 1);
    }

    void Update()
    {
        grassMaterial.SetBuffer("grassBuffer", grassBuffer);
        Graphics.DrawMeshInstancedIndirect(grassMesh, 0, grassMaterial, new Bounds(Vector3.zero, new Vector3(chunkSize, 10, chunkSize)), argsBuffer);
    }

    void OnDestroy()
    {
        grassBuffer.Release();
        argsBuffer.Release();
    }
}
