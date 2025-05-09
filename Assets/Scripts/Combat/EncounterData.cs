using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Encounter", menuName = "Combat/Encounter Data")]
public class EncounterData : ScriptableObject
{
    [Header("Encounter Info")]
    public string encounterId;
    public string encounterName;
    public string description;

    [Header("Enemies")]
    public List<EnemyData> enemies = new List<EnemyData>();

    [Header("Environment")]
    public string backgroundId;
    public string musicTrackId;
    public List<string> ambientSoundIds = new List<string>();

    [Header("Rewards")]
    public int minGoldReward;
    public int maxGoldReward;
    public int fixedExpReward;
    public int cardRewardCount = 3;
    public float rareCardChance = 0.10f;
    public float uncommonCardChance = 0.35f;
    public bool guaranteeRelic = false;
    public float relicChance = 0.05f;

    [Header("Special Encounter Rules")]
    public bool isEliteEncounter = false;
    public bool isBossEncounter = false;
    public bool hasSpecialRules = false;

    [TextArea(3, 10)]
    public string specialRulesDescription;
}

// Example usage:
/* 
Create a ScriptableObject in your project:

[In Editor]
1. Right-click in Project window
2. Create > Combat > Encounter Data
3. Fill in the fields in Inspector

[In Code]
EncounterData encounter = Resources.Load<EncounterData>("Encounters/MyEncounter");
*/