using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public string playerName;
    public int playerHealth;
    public int playerMaxHealth;
    public int gold;
    public List<string> deckCardNames; // Simplified; store card names
    public int currentNodeIndex;
}