using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapUI : MonoBehaviour
{
    [SerializeField] private Button[] nodeButtons;
    private MapNode currentNode;

    public void Initialize(MapNode node)
    {
        currentNode = node;
        for (int i = 0; i < nodeButtons.Length; i++)
        {
            if (i < node.NextNodes.Count)
            {
                nodeButtons[i].gameObject.SetActive(true);
                nodeButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = node.NextNodes[i].Type.ToString();
                int index = i;
                nodeButtons[i].onClick.AddListener(() => SelectNode(index));
            }
            else
            {
                nodeButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void SelectNode(int index)
    {
        // Transition to the selected node (e.g., start combat)
        FindObjectOfType<MapGenerator>().MoveToNode(currentNode.NextNodes[index]);
    }
}