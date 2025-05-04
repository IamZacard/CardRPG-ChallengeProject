using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private GameObject mapPanel; // Reference to MapPanel GameObject
    private MapNode currentNode;

    public MapNode GenerateFloor(int floorNumber)
    {
        MapNode startNode = new GameObject("Start").AddComponent<MapNode>();
        startNode.Type = NodeType.Combat;

        MapNode endNode = new GameObject("Boss").AddComponent<MapNode>();
        endNode.Type = NodeType.Boss;

        startNode.NextNodes.Add(endNode); // Simple linear path for prototype
        currentNode = startNode;
        return startNode;
    }

    public void MoveToNode(MapNode node)
    {
        if (node == null || !currentNode.NextNodes.Contains(node))
        {
            Debug.LogError("Invalid node transition");
            return;
        }

        currentNode.Complete();
        currentNode = node;

        // Trigger node-specific logic
        switch (node.Type)
        {
            case NodeType.Combat:
                StartCombat();
                break;
            case NodeType.Boss:
                StartBossCombat();
                break;
            case NodeType.Rest:
                TriggerRest();
                break;
                // Add other node types (Merchant, Event, etc.) as needed
        }

        // Update map UI
        FindObjectOfType<MapUI>().Initialize(currentNode);
    }

    private void StartCombat()
    {
        // Initialize a standard combat encounter
        CombatState combatState = FindObjectOfType<CombatState>();
        Player player = FindObjectOfType<Player>();
        List<Enemy> enemies = new List<Enemy> { CreateEnemy("BasicEnemy") };
        combatState.Initialize(player, enemies, FindObjectOfType<Deck>(), FindObjectOfType<Hand>(), FindObjectOfType<DiscardPile>());
        combatState.StartTurn();
        if (mapPanel != null)
            mapPanel.SetActive(false);
    }

    private void StartBossCombat()
    {
        // Initialize a boss combat
        CombatState combatState = FindObjectOfType<CombatState>();
        Player player = FindObjectOfType<Player>();
        List<Enemy> enemies = new List<Enemy> { CreateEnemy("BossEnemy") };
        combatState.Initialize(player, enemies, FindObjectOfType<Deck>(), FindObjectOfType<Hand>(), FindObjectOfType<DiscardPile>());
        combatState.StartTurn();
        if (mapPanel != null)
            mapPanel.SetActive(false);
    }

    private void TriggerRest()
    {
        // Heal player
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.Heal(20); // Use Heal method instead of direct Health modification
            FindObjectOfType<HealthBarUI>().UpdateHealth();
        }
    }

    private Enemy CreateEnemy(string enemyType)
    {
        GameObject enemyObj = new GameObject(enemyType);
        Enemy enemy = enemyObj.AddComponent<Enemy>();
        // Configure enemy stats (e.g., health, AI) based on enemyType
        return enemy;
    }
}