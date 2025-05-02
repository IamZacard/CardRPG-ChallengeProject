using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public void TakeTurn()
    {
        Debug.Log("Enemy is taking its turn.");
        // Simple AI: Deal 5 damage to player
        FindObjectOfType<Player>().TakeDamage(5);
        FindObjectOfType<TurnSystem>().EndTurn();
    }
}