using System;
using UnityEngine;

/// <summary>
/// Manages the turn-based flow of combat, tracking whose turn it is and handling turn transitions.
/// </summary>
public class TurnSystem : MonoBehaviour
{
    [Header("Turn Settings")]
    [SerializeField] private int maxTurnCount = 100; // Safety limit to prevent infinite combat

    // Turn state tracking
    private bool isPlayerTurn = true;
    private int currentTurn = 0;
    private bool isCombatActive = false;

    // Events
    public event Action<bool> OnTurnChanged; // bool parameter is true if it's the player's turn
    public event Action<int> OnTurnCountChanged; // Fires when turn counter changes

    /// <summary>
    /// Current turn number (starts at 1)
    /// </summary>
    public int CurrentTurn => currentTurn;

    /// <summary>
    /// Whether it's currently the player's turn
    /// </summary>
    public bool IsPlayerTurn => isPlayerTurn;

    /// <summary>
    /// Starts a new combat sequence, resetting the turn counter
    /// </summary>
    public void StartCombat()
    {
        isCombatActive = true;
        currentTurn = 1;
        isPlayerTurn = true;

        OnTurnCountChanged?.Invoke(currentTurn);
        OnTurnChanged?.Invoke(isPlayerTurn);

        Debug.Log($"Combat started. Turn {currentTurn}: Player's turn.");
    }

    /// <summary>
    /// Advances to the next turn, toggling between player and enemy turns
    /// </summary>
    public void AdvanceTurn()
    {
        if (!isCombatActive)
            return;

        // If it's the enemy's turn ending, increment the turn counter
        if (!isPlayerTurn)
        {
            currentTurn++;
            OnTurnCountChanged?.Invoke(currentTurn);

            // Safety check to prevent infinite combats
            if (currentTurn > maxTurnCount)
            {
                Debug.LogWarning("Combat exceeded maximum turn count. Forcing end.");
                EndCombat();
                return;
            }
        }

        // Toggle whose turn it is
        isPlayerTurn = !isPlayerTurn;

        // Notify listeners about the turn change
        OnTurnChanged?.Invoke(isPlayerTurn);

        Debug.Log($"Turn {currentTurn}: {(isPlayerTurn ? "Player" : "Enemy")}'s turn.");
    }

    /// <summary>
    /// Forcibly sets whose turn it is
    /// </summary>
    /// <param name="playerTurn">True if it should be the player's turn, false for enemy turn</param>
    public void SetTurn(bool playerTurn)
    {
        if (!isCombatActive || isPlayerTurn == playerTurn)
            return;

        isPlayerTurn = playerTurn;
        OnTurnChanged?.Invoke(isPlayerTurn);

        Debug.Log($"Turn changed to: {(isPlayerTurn ? "Player" : "Enemy")}'s turn.");
    }

    /// <summary>
    /// Ends the current combat
    /// </summary>
    public void EndCombat()
    {
        isCombatActive = false;
        Debug.Log("Combat ended.");
    }

    /// <summary>
    /// Get a formatted string of the current turn information
    /// </summary>
    /// <returns>String with turn number and whose turn it is</returns>
    public string GetTurnDisplayText()
    {
        return $"Turn {currentTurn}: {(isPlayerTurn ? "Your" : "Enemy")} Turn";
    }
}