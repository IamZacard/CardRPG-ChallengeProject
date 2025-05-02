using UnityEngine;

public class AttackCard : Card
{
    public int damage;

    public override void Play()
    {
        // Placeholder logic; in a real game, this would target an enemy
        Debug.Log($"Playing {cardName}, dealing {damage} damage.");
    }
}