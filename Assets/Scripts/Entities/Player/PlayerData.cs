using System.Collections.Generic;

/// <summary>
/// Placeholder for PlayerData
/// </summary>
[System.Serializable]
public class PlayerData
{
    public string playerName = "Hero";
    public int maxHealth = 80;
    public int currentHealth = 80;
    public int maxEnergy = 3;
    public int gold = 3;
    public List<CardData> deck = new List<CardData>();
    // Add other player data like relics, gold, etc.
}