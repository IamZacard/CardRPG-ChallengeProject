using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "CardGame/CardData")]
public class CardData : ScriptableObject
{
    public string Name;
    public string Description;
    public Sprite Artwork;
    public int Cost; // Energy cost to play
    public CardType Type; // Attack, Defense, Effect, etc.
    public CardRarity Rarity; // Common, Rare, Epic
    public CardTarget Target; // SingleEnemy, AllEnemies, Self, None
    public int Damage; // For attack cards
    public int Block; // For defense cards
    public int Healing; // For effect cards
    public StatusEffectData StatusEffect; // Type, amount, target
    public bool Exhaust; // Discarded permanently after play
    public bool Retain; // Stays in hand after turn
    public bool Ethereal; // Exhausts if not played this turn
}

public enum CardType { Attack, Defense, Effect }
public enum CardRarity { Common, Rare, Epic }
public enum CardTarget { SingleEnemy, AllEnemies, Self, None }

[System.Serializable]
public struct StatusEffectData
{
    public StatusEffectType Type;
    public int Amount;
    public CardTarget Target;
}