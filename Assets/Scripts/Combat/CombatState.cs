using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Contains all relevant information about the current combat state
/// </summary>
public class CombatState
{
    // Core references
    public Player Player { get; set; }
    public Enemy[] Enemies { get; set; }
    public Deck PlayerDeck { get; set; }
    public Deck DrawPile { get; set; }
    public Deck DiscardPile { get; set; }
    public Deck ExhaustPile { get; set; }

    // State tracking
    public int TurnNumber { get; set; }
    public int TurnCount { get; set; }
    public bool IsPlayerTurn { get; set; }
    public CombatDifficulty CombatDifficulty { get; set; }

    // Reference to the combat manager for callbacks
    public CombatManager CombatManager { get; set; }

    /// <summary>
    /// Find a specific enemy by ID or reference
    /// </summary>
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

    /// <summary>
    /// Check if all enemies are dead
    /// </summary>
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

    /// <summary>
    /// Find all alive enemies
    /// </summary>
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

    /// <summary>
    /// Check if player has enough action points for a specific cost
    /// </summary>
    public bool HasEnoughActionPoints(int cost)
    {
        return Player.CurrentActionPoints >= cost;
    }

    /// <summary>
    /// Reset state for a new turn
    /// </summary>
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

    /// <summary>
    /// End the current turn
    /// </summary>
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