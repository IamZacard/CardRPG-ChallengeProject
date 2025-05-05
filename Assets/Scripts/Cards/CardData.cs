// CardData.cs - ScriptableObject for card definitions
using UnityEngine;
using System.Collections.Generic;

public enum CardType
{
    Attack,
    Defense,
    Effect
}

public enum CardRarity
{
    Common,
    Uncommon,
    Rare
}

public enum CardTarget
{
    SingleEnemy,
    AllEnemies,
    Self,
    None
}

[CreateAssetMenu(fileName = "New Card", menuName = "Cards/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Basic Info")]
    public string cardName;
    public string description;
    public Sprite artwork;
    public int cost;
    public CardType cardType;
    public CardRarity rarity;
    public CardTarget target;

    [Header("Effects")]
    public int damage;
    public int block;
    public List<StatusEffectData> statusEffects = new List<StatusEffectData>();

    [Header("Special")]
    public bool exhaust;
    public bool retain;
    public bool ethereal;
    public string specialEffectId;

    [System.Serializable]
    public class StatusEffectData
    {
        public StatusEffectType type;
        public int amount;
        public TargetType targetType;

        public enum TargetType
        {
            Self,
            Target,
            AllEnemies
        }
    }
}