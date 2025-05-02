// CombatState.cs - Contains all relevant information about the current combat state
using UnityEngine;
using System.Collections.Generic;

public class CombatState
{
    public Player Player { get; set; }
    public Enemy[] Enemies { get; set; }
    public Deck DrawPile { get; set; }
    public Deck DiscardPile { get; set; }
    public Deck ExhaustPile { get; set; }
    public int TurnNumber { get; set; }
    public bool IsPlayerTurn { get; set; }

    // Reference to the combat manager for callbacks
    public CombatManager CombatManager { get; set; }

    // Find a specific enemy by ID or reference
    public Enemy FindEnemy(int enemyId)
    {
        foreach (var enemy in Enemies)
        {
            if (enemy.GetInstanceID() == enemyId)
            {
                return enemy;
            }
        }
        return null;
    }

    // Check if all enemies are dead
    public bool AreAllEnemiesDead()
    {
        foreach (var enemy in Enemies)
        {
            if (enemy != null && enemy.CurrentHealth > 0)
            {
                return false;
            }
        }
        return true;
    }

    // Find all alive enemies
    public List<Enemy> GetAliveEnemies()
    {
        List<Enemy> aliveEnemies = new List<Enemy>();
        foreach (var enemy in Enemies)
        {
            if (enemy != null && enemy.CurrentHealth > 0)
            {
                aliveEnemies.Add(enemy);
            }
        }
        return aliveEnemies;
    }

    // Check if player has enough action points for a specific cost
    public bool HasEnoughActionPoints(int cost)
    {
        return Player.CurrentActionPoints >= cost;
    }

    // Reset state for a new turn
    public void StartNewTurn(bool isPlayerTurn)
    {
        TurnNumber++;
        IsPlayerTurn = isPlayerTurn;

        if (isPlayerTurn)
        {
            Player.OnStartTurn();
        }
        else
        {
            // Process enemy turn start
            foreach (var enemy in GetAliveEnemies())
            {
                enemy.OnStartTurn();
            }
        }
    }

    // End the current turn
    public void EndCurrentTurn()
    {
        if (IsPlayerTurn)
        {
            Player.OnEndTurn();
        }
        else
        {
            // Process enemy turn end
            foreach (var enemy in GetAliveEnemies())
            {
                enemy.OnEndTurn();
            }
        }
    }
}