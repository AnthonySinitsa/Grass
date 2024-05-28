using UnityEngine;

public class BillboardGrass : MonoBehaviour
{
    public GameObject grassPrefab; // The grass prefab with 3 intersecting quads
    public int gridWidth = 100; // Width of the grid in meters
    public int gridHeight = 100; // Height of the grid in meters
    public float spacing = 1f; // Distance between each grass instance

    void Start()
    {
        InitializeGrass();
    }

    void InitializeGrass()
    {
        Debug.Log("Initializing grass...");

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 position = new Vector3(x * spacing, 0, z * spacing);
                Instantiate(grassPrefab, position, Quaternion.identity, transform);
            }
        }

        Debug.Log("Grass initialized.");
    }
}
