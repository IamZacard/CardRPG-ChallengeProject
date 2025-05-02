using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public Player player;
    public Enemy enemy;
    public TurnSystem turnSystem;

    void Start()
    {
        turnSystem = GetComponent<TurnSystem>();
        turnSystem.StartCombat();
    }

    public void EndCombat()
    {
        // Placeholder for victory/defeat logic; would typically return to map
        Debug.Log("Combat ended.");
    }
}