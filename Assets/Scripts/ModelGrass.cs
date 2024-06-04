using UnityEngine;

public class ModelGrass : MonoBehaviour
{
    public ComputeShader grassComputeShader;
    public Material grassMaterial;
    public Mesh grassMesh;

    public int fieldSize = 100;
    public bool grassUpdate = false;

    private ComputeBuffer grassBuffer, argsBuffer;
    private int kernelHandle;

    void Start()
    {
        kernelHandle = grassComputeShader.FindKernel("CSMain");

        int totalGrassBlades = fieldSize * fieldSize;
        grassBuffer = new ComputeBuffer(totalGrassBlades, sizeof(float) * 3);
        grassComputeShader.SetBuffer(kernelHandle, "grassBuffer", grassBuffer);

        uint[] args = new uint[5] { grassMesh.GetIndexCount(0), (uint)(totalGrassBlades), 0, 0, 0 };
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        GenerateGrass();
    }

    void GenerateGrass()
    {
        grassComputeShader.SetInt("fieldSize", fieldSize);
        int totalGrassBlades = fieldSize * fieldSize;
        grassComputeShader.Dispatch(kernelHandle, Mathf.CeilToInt(totalGrassBlades / 10.0f), 1, 1);
    }

    void Update()
    {
        if (grassUpdate)
        {
            GenerateGrass();
            grassUpdate = false;
        }

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