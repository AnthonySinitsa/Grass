using UnityEngine;

public class BillboardGrass : MonoBehaviour
{
    public Texture2D grassTexture;
    public ComputeShader grassComputeShader;

    private int grassCount = 1000; // Adjust as needed
    private ComputeBuffer grassBuffer;
    private Material grassMaterial;
    
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
        GrassData[] grassDataArray = new GrassData[grassCount];
        for (int i = 0; i < grassCount; i++)
        {
            grassDataArray[i].position = new Vector3(
                Random.Range(-50, 50), 0, Random.Range(-50, 50)
            );
            grassDataArray[i].uv = new Vector2(0, 0); // UVs will be handled by shader
        }

        grassBuffer = new ComputeBuffer(grassCount, sizeof(float) * 5);
        grassBuffer.SetData(grassDataArray);

        int kernelHandle = grassComputeShader.FindKernel("CSMain");
        grassComputeShader.SetBuffer(kernelHandle, "grassBuffer", grassBuffer);
        grassComputeShader.Dispatch(kernelHandle, grassCount / 10, 1, 1);

        grassMaterial = new Material(Shader.Find("Unlit/BillboardGrass"));
        grassMaterial.SetTexture("_MainTex", grassTexture);
        grassMaterial.SetBuffer("grassBuffer", grassBuffer);
    }

    void OnRenderObject()
    {
        grassMaterial.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, grassCount);
    }

    void OnDestroy()
    {
        if (grassBuffer != null)
        {
            grassBuffer.Release();
        }
    }
}
