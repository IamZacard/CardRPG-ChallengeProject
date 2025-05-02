using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public GameObject nodePrefab;
    public int numberOfNodes = 10;

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        for (int i = 0; i < numberOfNodes; i++)
        {
            GameObject node = Instantiate(nodePrefab, new Vector3(i * 2, 0, 0), Quaternion.identity);
            node.GetComponent<MapNode>().type = (MapNode.NodeType)Random.Range(0, 4); // Random type
        }
    }
}