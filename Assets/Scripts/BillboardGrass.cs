using UnityEngine;

public class BillboardGrass : MonoBehaviour
{
    public Mesh grassMesh;
    public Material grassMaterial;
    public Texture2D grassTexture;
    public int instanceCount = 10000;
    public float planeSize = 300f;

    private Matrix4x4[] matrices;
    private Vector4[] colors;
    private MaterialPropertyBlock propertyBlock;

    void Start()
    {
        InitializeGrass();
    }

    void InitializeGrass()
    {
        matrices = new Matrix4x4[instanceCount];
        colors = new Vector4[instanceCount];
        propertyBlock = new MaterialPropertyBlock();

        for (int i = 0; i < instanceCount; i++)
        {
            float x = Random.Range(-planeSize / 2, planeSize / 2);
            float z = Random.Range(-planeSize / 2, planeSize / 2);
            float y = 0f; // Assuming flat terrain, adjust as necessary

            Vector3 position = new Vector3(x, y, z);
            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
            Vector3 scale = Vector3.one;

            matrices[i] = Matrix4x4.TRS(position, rotation, scale);
            colors[i] = new Vector4(1f, 1f, 1f, 1f);
        }

        propertyBlock.SetVectorArray("_Color", colors);
    }

    void Update()
    {
        Graphics.DrawMeshInstanced(grassMesh, 0, grassMaterial, matrices, instanceCount, propertyBlock);
    }
}
