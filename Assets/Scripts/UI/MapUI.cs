using UnityEngine;

public class MapUI : MonoBehaviour
{
    public void DisplayMap()
    {
        Debug.Log("Displaying map.");
        // Show map nodes here
    }

    public void HighlightCurrentNode(MapNode node)
    {
        Debug.Log($"Highlighting {node.type} node.");
        // Highlight UI element for current node
    }
}