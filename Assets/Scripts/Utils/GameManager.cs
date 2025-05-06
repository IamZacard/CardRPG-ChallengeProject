using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
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
    }
    

    internal void SetPendingCombatData(CombatData combatData)
    {
        throw new NotImplementedException();
    }

    internal void SetPendingRewardData(RewardData rewardData)
    {
        throw new NotImplementedException();
    }

    internal CombatData GetPendingCombatData()
    {
        throw new NotImplementedException();
    }

    internal PlayerData GetPlayerData()
    {
        throw new NotImplementedException();
    }

    internal List<CardData> GetPlayerDeckData()
    {
        throw new NotImplementedException();
    }
}
