using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Container for all data needed to initialize a combat encounter.
/// This class acts as a data transfer object between map/encounter 
/// selection and the combat scene.
/// </summary>
[Serializable]
public class CombatData
{
    // Combat encounter metadata
    public string EncounterID;
    public string EncounterName;
    public CombatDifficulty Difficulty;
    public CombatType Type;

    // Enemy data
    public List<EnemyData> EnemyDataList = new List<EnemyData>();

    // Special combat rules or modifiers
    public List<CombatModifier> Modifiers = new List<CombatModifier>();

    // Environment/arena settings
    public string ArenaType;
    public bool HasHazards;

    // Rewards data
    public RewardData RewardData;

    // Optional: Background music track
    public string MusicTrackName;

    // Optional: Custom starting state for the player
    public PlayerCombatStartState PlayerStartState;

    public CombatData(string encounterId, CombatDifficulty difficulty)
    {
        EncounterID = encounterId;
        Difficulty = difficulty;
        Type = CombatType.Normal;
    }

    public void AddEnemy(EnemyData enemyData)
    {
        EnemyDataList.Add(enemyData);
    }

    public void AddModifier(CombatModifier modifier)
    {
        Modifiers.Add(modifier);
    }
}

/// <summary>
/// Defines difficulty levels for combat encounters
/// </summary>
public enum CombatDifficulty
{
    Easy,
    Normal,
    Hard,
    Elite,
    Boss
}

/// <summary>
/// Defines different types of combat encounters
/// </summary>
public enum CombatType
{
    Normal,
    Elite,
    Boss,
    Event,
    Tutorial
}

/// <summary>
/// Represents a modifier that changes combat rules
/// </summary>
[Serializable]
public class CombatModifier
{
    public string ID;
    public string Name;
    public string Description;
    public ModifierEffect Effect;

    public CombatModifier(string id, string name, string description, ModifierEffect effect)
    {
        ID = id;
        Name = name;
        Description = description;
        Effect = effect;
    }
}

/// <summary>
/// Defines the effect of a combat modifier
/// </summary>
public enum ModifierEffect
{
    PlayerStatBoost,
    EnemyStatBoost,
    ModifyCardCost,
    ModifyStatusEffects,
    CustomRule
}

/// <summary>
/// Defines reward data for the combat encounter
/// </summary>
[Serializable]
public class RewardData
{
    public int GoldReward;
    public int ExperienceReward;
    public List<CardData> PotentialCardRewards = new List<CardData>();
    public List<string> GuaranteedItemRewards = new List<string>();
    public float RareItemChance;

    public RewardData(int gold, int experience)
    {
        GoldReward = gold;
        ExperienceReward = experience;
        RareItemChance = 0.1f;
    }
}

/// <summary>
/// Optional data for custom player starting conditions
/// </summary>
[Serializable]
public class PlayerCombatStartState
{
    public int StartingEnergy;
    public int ExtraCardsDrawn;
    public List<StatusEffect> StartingStatusEffects = new List<StatusEffect>();

    public PlayerCombatStartState()
    {
        StartingEnergy = -1; // -1 means use default energy
        ExtraCardsDrawn = 0;
    }
}