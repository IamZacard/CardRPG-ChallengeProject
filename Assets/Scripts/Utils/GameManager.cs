using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Fields for storing data between scenes
    private CombatData pendingCombatData;
    private RewardData pendingRewardData;
    private PlayerData playerData;
    private List<CardData> playerDeckData = new List<CardData>();

    private void Awake()
    {
        // Singleton check
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Prevent duplicates
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize player on first launch
        InitializePlayerData();
    }

    private void InitializePlayerData()
    {
        // Create initial data if not present
        if (playerData == null)
        {
            playerData = new PlayerData();
            // You can add player starting data initialization here
        }

        if (playerDeckData == null || playerDeckData.Count == 0)
        {
            playerDeckData = new List<CardData>();
            // You can add the starting deck here
        }
    }

    internal void SetPendingCombatData(CombatData combatData)
    {
        pendingCombatData = combatData;
        Debug.Log("Combat data set successfully");
    }

    internal void SetPendingRewardData(RewardData rewardData)
    {
        pendingRewardData = rewardData;
        Debug.Log("Reward data set successfully");
    }

    internal CombatData GetPendingCombatData()
    {
        // You can add a null check
        if (pendingCombatData == null)
        {
            Debug.LogWarning("Trying to get null combat data");
        }
        return pendingCombatData;
    }

    internal PlayerData GetPlayerData()
    {
        return playerData;
    }

    internal List<CardData> GetPlayerDeckData()
    {
        return playerDeckData;
    }

    // Additional methods for updating game state

    internal void UpdatePlayerAfterCombat(PlayerData updatedPlayerData)
    {
        if (updatedPlayerData != null)
        {
            playerData = updatedPlayerData;
            Debug.Log("Player data updated after combat");
        }
    }

    internal void AddCardsToPlayerDeck(List<CardData> newCards)
    {
        if (newCards != null && newCards.Count > 0)
        {
            playerDeckData.AddRange(newCards);
            Debug.Log($"Added {newCards.Count} new cards to the player deck");
        }
    }

    internal RewardData GetAndClearRewardData()
    {
        RewardData data = pendingRewardData;
        pendingRewardData = null; // Clear data after use
        return data;
    }

    internal void ClearPendingCombatData()
    {
        pendingCombatData = null;
        Debug.Log("Combat data cleared");
    }
}
