using UnityEngine;

public class ModelGrass : MonoBehaviour
{
    public ComputeShader grassComputeShader;
    public Material grassMaterial;
    public Mesh grassMesh;
    public Texture2D grassTypeTexture;
    
    public int chunkSize = 10;
    public int grassDensity = 1000;

    private ComputeBuffer grassBuffer;
    private int kernelHandle;

    struct GrassBlade
    {
        public Vector3 position;
        public Vector2 facing;
        public float windStrength;
        public int perBladeHash;
        public int grassType;
        public Vector2 clumpFacing;
        public Color clumpColor;
        public float height;
        public float width;
        public float tilt;
        public float bend;
        public float sideCurve;
    }

    void Start()
    {
        kernelHandle = grassComputeShader.FindKernel("CSMain");

        grassBuffer = new ComputeBuffer(grassDensity, sizeof(float) * 17 + sizeof(int) * 2);
        grassComputeShader.SetBuffer(kernelHandle, "grassBuffer", grassBuffer);

        GenerateGrass();
    }

    void GenerateGrass()
    {
        grassComputeShader.SetInt("chunkSize", chunkSize);
        grassComputeShader.SetInt("grassDensity", grassDensity);
        grassComputeShader.Dispatch(kernelHandle, grassDensity / 10, 1, 1);
    }

    void OnRenderObject()
    {
        grassMaterial.SetPass(0);
        grassMaterial.SetBuffer("grassBuffer", grassBuffer);
        Graphics.DrawProceduralNow(MeshTopology.Points, grassBuffer.count);
    }

    void OnDestroy()
    {
        grassBuffer.Release();
    }
}
