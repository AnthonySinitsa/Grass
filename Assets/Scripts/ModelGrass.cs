using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
    private ComputeBuffer argsBuffer;
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

        grassBuffer = new ComputeBuffer(grassDensity, Marshal.SizeOf(typeof(GrassBlade)));
        grassComputeShader.SetBuffer(kernelHandle, "grassBuffer", grassBuffer);

        GenerateGrass();
    }

    void GenerateGrass()
    {
        grassComputeShader.SetInt("chunkSize", chunkSize);
        grassComputeShader.SetInt("grassDensity", grassDensity);
        grassComputeShader.Dispatch(kernelHandle, grassDensity / 10, 1, 1);

        uint[] args = new uint[5] { (uint)grassMesh.GetIndexCount(0), (uint)grassDensity, 0, 0, 0 };
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
    }

    void OnRenderObject()
    {
        grassMaterial.SetPass(0);
        grassMaterial.SetBuffer("grassBuffer", grassBuffer);
        Graphics.DrawMeshInstancedIndirect(grassMesh, 0, grassMaterial, new Bounds(Vector3.zero, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue)), argsBuffer);
    }

    void OnDestroy()
    {
        grassBuffer.Release();
        argsBuffer.Release();
    }
}
