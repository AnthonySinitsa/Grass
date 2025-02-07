using UnityEngine;

[System.Serializable]
public struct GrassLODSettings
{
    public Mesh grassMesh;
    public float minDistance;
    public float maxDistance;
    public int densityDivisor; // Reduces density for lower LODs
}

public class GrassLODManager : MonoBehaviour
{
    public ComputeShader grassComputeShader;
    public ComputeShader windComputeShader;
    public Material grassMaterial;
    public GrassLODSettings[] lodSettings;
    
    [Header("Grass Settings")]
    public int numChunks = 5;
    public int baseDensity = 100; // This is the highest LOD density
    public float chunkSize = 10.0f;
    
    private Camera mainCamera;
    private ComputeBuffer[] grassBuffers;
    private ComputeBuffer[] argsBuffers;
    private ComputeBuffer[] windBuffers;
    private int grassKernelHandle, windKernelHandle;
    private Transform cameraTransform;
    
    private void Start()
    {
        mainCamera = Camera.main;
        cameraTransform = mainCamera.transform;
        
        grassKernelHandle = grassComputeShader.FindKernel("CSMain");
        windKernelHandle = windComputeShader.FindKernel("CSMain");
        
        InitializeBuffers();
        GenerateGrassForAllLODs();
    }
    
    private void InitializeBuffers()
    {
        int numLODs = lodSettings.Length;
        grassBuffers = new ComputeBuffer[numLODs];
        argsBuffers = new ComputeBuffer[numLODs];
        windBuffers = new ComputeBuffer[numLODs];
        
        for (int i = 0; i < numLODs; i++)
        {
            int density = baseDensity / lodSettings[i].densityDivisor;
            int totalGrassBlades = numChunks * numChunks * density * density;
            
            grassBuffers[i] = new ComputeBuffer(totalGrassBlades, sizeof(float) * 7);
            windBuffers[i] = new ComputeBuffer(totalGrassBlades, sizeof(float) * 3);
            
            uint[] args = new uint[5] { lodSettings[i].grassMesh.GetIndexCount(0), (uint)totalGrassBlades, 0, 0, 0 };
            argsBuffers[i] = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            argsBuffers[i].SetData(args);
        }
    }
    
    private void GenerateGrassForLOD(int lodIndex)
    {
        int density = baseDensity / lodSettings[lodIndex].densityDivisor;
        
        grassComputeShader.SetInt("numChunks", numChunks);
        grassComputeShader.SetInt("chunkDensity", density);
        grassComputeShader.SetFloat("chunkSize", chunkSize);
        grassComputeShader.SetBuffer(grassKernelHandle, "grassBuffer", grassBuffers[lodIndex]);
        
        int totalGrassBlades = numChunks * numChunks * density * density;
        const int THREAD_GROUP_SIZE = 64;
        int numThreadGroups = Mathf.CeilToInt((float)totalGrassBlades / THREAD_GROUP_SIZE);
        
        grassComputeShader.Dispatch(grassKernelHandle, numThreadGroups, 1, 1);
    }
    
    private void GenerateGrassForAllLODs()
    {
        for (int i = 0; i < lodSettings.Length; i++)
        {
            GenerateGrassForLOD(i);
        }
    }
    
    private void UpdateWindForLOD(int lodIndex)
    {
        int density = baseDensity / lodSettings[lodIndex].densityDivisor;
        
        windComputeShader.SetBuffer(windKernelHandle, "windBuffer", windBuffers[lodIndex]);
        windComputeShader.SetInt("numChunks", numChunks);
        windComputeShader.SetInt("chunkDensity", density);
        windComputeShader.SetFloat("chunkSize", chunkSize);
        windComputeShader.SetFloat("time", Time.time);
        
        int totalGrassBlades = numChunks * numChunks * density * density;
        const int THREAD_GROUP_SIZE = 64;
        int numThreadGroups = Mathf.CeilToInt((float)totalGrassBlades / THREAD_GROUP_SIZE);
        
        windComputeShader.Dispatch(windKernelHandle, numThreadGroups, 1, 1);
    }
    
    private void Update()
    {
        Vector3 cameraPos = cameraTransform.position;
        float boundsSize = numChunks * chunkSize;
        Bounds bounds = new Bounds(Vector3.zero, new Vector3(boundsSize, 10.0f, boundsSize));
        
        // Update wind and render for each LOD level
        for (int i = 0; i < lodSettings.Length; i++)
        {
            UpdateWindForLOD(i);
            
            grassMaterial.SetBuffer("grassBuffer", grassBuffers[i]);
            grassMaterial.SetBuffer("windBuffer", windBuffers[i]);
            
            // Only render this LOD if the camera is within its distance range
            float distanceToCamera = Vector3.Distance(cameraPos, bounds.center);
            if (distanceToCamera >= lodSettings[i].minDistance && 
                distanceToCamera < lodSettings[i].maxDistance)
            {
                Graphics.DrawMeshInstancedIndirect(
                    lodSettings[i].grassMesh,
                    0,
                    grassMaterial,
                    bounds,
                    argsBuffers[i]
                );
            }
        }
    }
    
    private void OnDestroy()
    {
        if (grassBuffers != null)
        {
            foreach (var buffer in grassBuffers)
            {
                if (buffer != null) buffer.Release();
            }
        }
        
        if (argsBuffers != null)
        {
            foreach (var buffer in argsBuffers)
            {
                if (buffer != null) buffer.Release();
            }
        }
        
        if (windBuffers != null)
        {
            foreach (var buffer in windBuffers)
            {
                if (buffer != null) buffer.Release();
            }
        }
    }
}