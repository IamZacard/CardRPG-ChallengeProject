using UnityEngine;

public class DefenseCard : Card
{
    public int block;

    public override void Play()
    {
        // Placeholder logic; would typically apply block to the player
        Debug.Log($"Playing {cardName}, gaining {block} block.");
    }
}