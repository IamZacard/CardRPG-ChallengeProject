using UnityEngine;

public class MapManager : MonoBehaviour
{
    public MapNode currentNode;

    public void MoveToNode(MapNode node)
    {
        currentNode = node;
        node.OnNodeSelected();
    }
}