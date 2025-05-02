using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class CardData : ScriptableObject
{
    public string cardName;
    public int cost;
    public string description;
    public Sprite artwork;
    public CardType type;
    public int value; // Damage, block, or other value based on type
    public StatusEffect effect; // Optional, for effect cards
}

public enum CardType
{
    Attack,
    Defense,
    Effect
}