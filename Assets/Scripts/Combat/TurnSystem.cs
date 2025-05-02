using UnityEngine;

public class TurnSystem : MonoBehaviour
{
    public bool isPlayerTurn = true;

    public void StartCombat()
    {
        Debug.Log("Combat started.");
        PlayerTurn();
    }

    public void PlayerTurn()
    {
        isPlayerTurn = true;
        Debug.Log("Player's turn.");
        // Enable player controls here (e.g., via UI)
    }

    public void EnemyTurn()
    {
        isPlayerTurn = false;
        Debug.Log("Enemy's turn.");
        FindObjectOfType<EnemyAI>().TakeTurn(); // Trigger enemy AI
    }

    public void EndTurn()
    {
        if (isPlayerTurn)
        {
            EnemyTurn();
        }
        else
        {
            PlayerTurn();
        }
    }
}