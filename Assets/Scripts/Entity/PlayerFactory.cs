using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Factory class for creating and configuring Player instances.
/// </summary>
public static class PlayerFactory
{
    /// <summary>
    /// Creates a player instance at the specified position with the given data
    /// </summary>
    /// <param name="playerData">Data to configure the player</param>
    /// <param name="position">Position to spawn the player at</param>
    /// <returns>The configured Player instance</returns>
    public static Player CreatePlayer(PlayerData playerData, Vector3 position)
    {
        // Load player prefab from Resources
        GameObject playerPrefab = Resources.Load<GameObject>("Prefabs/Entities/Player");
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab not found in Resources!");
            return null;
        }

        // Instantiate player at the specified position
        GameObject playerObject = Object.Instantiate(playerPrefab, position, Quaternion.identity);
        Player player = playerObject.GetComponent<Player>();
        if (player == null)
        {
            Debug.LogError("Player component not found on prefab!");
            return null;
        }

        // Configure player with data
        ConfigurePlayer(player, playerData);

        return player;
    }

    /// <summary>
    /// Configures an existing player instance with the provided data
    /// </summary>
    /// <param name="player">The player instance to configure</param>
    /// <param name="playerData">Data to apply to the player</param>
    public static void ConfigurePlayer(Player player, PlayerData playerData)
    {
        // Set basic properties
        player.entityName = playerData.playerName;
        player.maxHealth = playerData.maxHealth;
        player.currentHealth = playerData.currentHealth > 0 ? playerData.currentHealth : playerData.maxHealth;
        player.baseBlock = playerData.baseBlock;

        // Set energy values
        player.maxEnergy = playerData.maxEnergy;
        player.currentEnergy = playerData.startingEnergy > 0 ? playerData.startingEnergy : playerData.maxEnergy;

        // Set class-specific properties
        player.playerClass = playerData.playerClass;

        // Apply any permanent status effects or abilities
        if (playerData.permanentStatusEffects != null)
        {
            foreach (var statusEffectData in playerData.permanentStatusEffects)
            {
                player.ApplyStatusEffect(statusEffectData.type, statusEffectData.amount);
            }
        }

        // Set up any equipment if your game has that
        if (playerData.equipment != null)
        {
            foreach (var equipItem in playerData.equipment)
            {
                // This would depend on your equipment system implementation
                player.EquipItem(equipItem);
            }
        }

        // Initialize the player
        player.Initialize();

        Debug.Log($"Player created: {player.entityName} (HP: {player.currentHealth}/{player.maxHealth}, Energy: {player.currentEnergy}/{player.maxEnergy})");
    }
}

/// <summary>
/// Data container for player configuration
/// </summary>
[System.Serializable]
public class PlayerData
{
    // Basic properties
    public string playerName = "Hero";
    public int maxHealth = 80;
    public int currentHealth = -1; // -1 means use maxHealth
    public int baseBlock = 0;

    // Energy system
    public int maxEnergy = 3;
    public int startingEnergy = -1; // -1 means use maxEnergy

    // Class and abilities
    public PlayerClass playerClass = PlayerClass.Warrior;
    public List<StatusEffectData> permanentStatusEffects = new List<StatusEffectData>();

    // Equipment
    public List<EquipmentData> equipment = new List<EquipmentData>();

    // Deck data is typically handled separately
    public string defaultDeckId = "starter";
}

/// <summary>
/// Player class types
/// </summary>
public enum PlayerClass
{
    Warrior,
    Rogue,
    Mage,
    Cleric
}

/// <summary>
/// Data for a status effect
/// </summary>
[System.Serializable]
public class StatusEffectData
{
    public StatusEffectType type;
    public int amount;

    public StatusEffectData(StatusEffectType type, int amount)
    {
        this.type = type;
        this.amount = amount;
    }
}

/// <summary>
/// Data for equipment items
/// </summary>
[System.Serializable]
public class EquipmentData
{
    public string equipmentId;
    public string equipmentName;
    public EquipmentSlot slot;
    public List<StatusEffectData> statusEffects = new List<StatusEffectData>();

    public EquipmentData(string id, string name, EquipmentSlot slot)
    {
        this.equipmentId = id;
        this.equipmentName = name;
        this.slot = slot;
    }
}

/// <summary>
/// Equipment slot types
/// </summary>
public enum EquipmentSlot
{
    Weapon,
    Armor,
    Accessory
}