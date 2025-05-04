using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapNode : MonoBehaviour
{
    public NodeType Type; // Combat, Rest, etc.
    public bool IsCompleted { get; private set; }
    public List<MapNode> NextNodes = new List<MapNode>();

    public void Complete() => IsCompleted = true;

    internal void OnNodeSelected()
    {
        switch (Type)
        {
            case NodeType.Combat:
            case NodeType.Elite:
            case NodeType.Boss:
                SceneManager.LoadScene("Combat");
                break;
            case NodeType.Rest:
                SceneManager.LoadScene("Rest");
                break;
            case NodeType.Merchant:
                SceneManager.LoadScene("Shop");
                break;
            case NodeType.Event:
                SceneManager.LoadScene("Rest");
                break;
            
            case NodeType.StartingNode:
                break;
        }
    }
}

public enum NodeType { StartingNode, Combat, Elite, Rest, Merchant, Event, Boss }