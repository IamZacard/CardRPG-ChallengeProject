using UnityEngine;

public class MapNode : MonoBehaviour
{
    public enum NodeType { Combat, Shop, Event, Rest }
    public NodeType type;
    public bool isCompleted = false;

    public void OnNodeSelected()
    {
        Debug.Log($"Selected {type} node.");
        // Load scene based on type (e.g., SceneManager.LoadScene(type.ToString()))
    }
}