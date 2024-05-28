using UnityEngine;

public class BillboardGrass : MonoBehaviour {
    public int dimension = 100; // Number of grass objects per side
    public float scale = 1.0f; // Scale of each grass object
    public Material grassMaterial;
    public Mesh grassMesh;

    private ComputeBuffer grassDataBuffer;
    private int kernelIndex;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 }; // Arguments for drawing mesh

    struct GrassData {
        public Vector4 position; // Position in world space (xyz) and scale (w)
        public Vector2 uv;       // UV coordinates for texture mapping
    }

    void Start() {
        InitializeGrass();
    }

    void InitializeGrass() {
        int numInstances = dimension * dimension;
        GrassData[] grassDataArray = new GrassData[numInstances];
        
        for (int x = 0; x < dimension; x++) {
            for (int y = 0; y < dimension; y++) {
                int index = x + y * dimension;
                grassDataArray[index].position = new Vector4(x * scale, 0.0f, y * scale, scale);
                grassDataArray[index].uv = new Vector2((float)x / (dimension - 1), (float)y / (dimension - 1));
            }
        }

        grassDataBuffer = new ComputeBuffer(numInstances, sizeof(float) * 6);
        grassDataBuffer.SetData(grassDataArray);

        kernelIndex = grassMaterial.FindKernel("InitializeGrass");
        grassMaterial.SetBuffer("_GrassDataBuffer", grassDataBuffer);
        grassMaterial.SetInt("_Dimension", dimension);

        args[0] = (uint)grassMesh.GetIndexCount(0);
        args[1] = (uint)numInstances;
        args[2] = (uint)grassMesh.GetIndexStart(0);
        args[3] = (uint)grassMesh.GetBaseVertex(0);

        UpdateArgsBuffer();
    }

    void UpdateArgsBuffer() {
        ComputeBuffer argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
        grassMaterial.SetBuffer("_GrassDataBuffer", grassDataBuffer);
        argsBuffer.Release();
    }

    void OnRenderObject() {
        grassMaterial.SetPass(0);
        Graphics.DrawMeshInstancedIndirect(grassMesh, 0, grassMaterial, new Bounds(Vector3.zero, Vector3.one * 10000.0f), 0, null, 0, null, UnityEngine.Rendering.ShadowCastingMode.Off, false);
    }

    void OnDestroy() {
        grassDataBuffer.Release();
    }
}
