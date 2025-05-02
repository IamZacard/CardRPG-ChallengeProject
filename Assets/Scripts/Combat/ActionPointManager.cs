using System;
using UnityEngine;

/// <summary>
/// Manages the player's action points (AP) during combat.
/// Action points are used to play cards and are refreshed each turn.
/// </summary>
public class ActionPointManager : MonoBehaviour
{
    [Header("AP Settings")]
    [SerializeField] private int baseActionPoints = 3;
    [SerializeField] private int maxActionPoints = 5;

    // Current state
    private int currentActionPoints;
    private int maxActionPointsThisCombat;

    // Events
    public event Action<int, int> OnActionPointsChanged; // Current AP, Max AP

    /// <summary>
    /// Current available action points
    /// </summary>
    public int CurrentActionPoints => currentActionPoints;

    /// <summary>
    /// Maximum action points available this combat
    /// </summary>
    public int MaxActionPoints => maxActionPointsThisCombat;

    /// <summary>
    /// Setup player action points for a new combat
    /// </summary>
    /// <param name="startingPoints">Number of action points to start with (defaults to baseActionPoints if not specified)</param>
    public void SetupPlayer(int startingPoints = -1)
    {
        // Use default if no starting points specified
        if (startingPoints < 0)
            startingPoints = baseActionPoints;

        maxActionPointsThisCombat = startingPoints;
        currentActionPoints = startingPoints;

        // Ensure max AP doesn't exceed the absolute maximum
        if (maxActionPointsThisCombat > maxActionPoints)
            maxActionPointsThisCombat = maxActionPoints;

        NotifyActionPointsChanged();

        Debug.Log($"Player starting with {currentActionPoints}/{maxActionPointsThisCombat} action points");
    }

    /// <summary>
    /// Refill action points to maximum at the start of player's turn
    /// </summary>
    public void RefillActionPoints()
    {
        currentActionPoints = maxActionPointsThisCombat;
        NotifyActionPointsChanged();

        Debug.Log($"Action points refilled to {currentActionPoints}/{maxActionPointsThisCombat}");
    }

    /// <summary>
    /// Use action points for playing a card
    /// </summary>
    /// <param name="amount">AP cost of the card</param>
    /// <returns>True if there were enough points and they were successfully used</returns>
    public bool UseActionPoints(int amount)
    {
        if (amount <= 0)
            return true; // Free actions always succeed

        if (currentActionPoints < amount)
            return false; // Not enough AP

        currentActionPoints -= amount;
        NotifyActionPointsChanged();

        Debug.Log($"Used {amount} action points. {currentActionPoints}/{maxActionPointsThisCombat} remaining.");
        return true;
    }

    /// <summary>
    /// Add bonus action points during the current turn
    /// </summary>
    /// <param name="amount">Number of AP to add</param>
    public void AddTemporaryActionPoints(int amount)
    {
        if (amount <= 0)
            return;

        currentActionPoints += amount;

        // Cap at maximum
        if (currentActionPoints > maxActionPointsThisCombat)
            currentActionPoints = maxActionPointsThisCombat;

        NotifyActionPointsChanged();

        Debug.Log($"Added {amount} temporary action points. Now at {currentActionPoints}/{maxActionPointsThisCombat}");
    }

    /// <summary>
    /// Permanently increase the maximum AP for this combat
    /// </summary>
    /// <param name="amount">Amount to increase max AP by</param>
    public void IncreaseMaxActionPoints(int amount)
    {
        if (amount <= 0)
            return;

        int oldMax = maxActionPointsThisCombat;
        maxActionPointsThisCombat += amount;

        // Cap at absolute maximum
        if (maxActionPointsThisCombat > maxActionPoints)
            maxActionPointsThisCombat = maxActionPoints;

        // If max actually increased, also add that many current AP
        int actualIncrease = maxActionPointsThisCombat - oldMax;
        if (actualIncrease > 0)
        {
            currentActionPoints += actualIncrease;
            NotifyActionPointsChanged();

            Debug.Log($"Increased max AP by {actualIncrease}. Now at {currentActionPoints}/{maxActionPointsThisCombat}");
        }
    }

    /// <summary>
    /// Decrease maximum AP (from enemy debuffs, etc.)
    /// </summary>
    /// <param name="amount">Amount to decrease by</param>
    public void DecreaseMaxActionPoints(int amount)
    {
        if (amount <= 0)
            return;

        maxActionPointsThisCombat -= amount;

        // Ensure minimum of 1 max AP
        if (maxActionPointsThisCombat < 1)
            maxActionPointsThisCombat = 1;

        // Adjust current AP if it exceeds new maximum
        if (currentActionPoints > maxActionPointsThisCombat)
            currentActionPoints = maxActionPointsThisCombat;

        NotifyActionPointsChanged();

        Debug.Log($"Decreased max AP by {amount}. Now at {currentActionPoints}/{maxActionPointsThisCombat}");
    }

    /// <summary>
    /// Reset to default AP values
    /// </summary>
    public void ResetActionPoints()
    {
        maxActionPointsThisCombat = baseActionPoints;
        currentActionPoints = baseActionPoints;
        NotifyActionPointsChanged();

        Debug.Log($"Action points reset to default: {currentActionPoints}/{maxActionPointsThisCombat}");
    }

    /// <summary>
    /// Notify listeners about AP changes
    /// </summary>
    private void NotifyActionPointsChanged()
    {
        OnActionPointsChanged?.Invoke(currentActionPoints, maxActionPointsThisCombat);
    }

    /// <summary>
    /// Check if player has enough AP to play a card
    /// </summary>
    /// <param name="cost">AP cost to check</param>
    /// <returns>True if there are enough points available</returns>
    public bool HasEnoughActionPoints(int cost)
    {
        return currentActionPoints >= cost;
    }
}