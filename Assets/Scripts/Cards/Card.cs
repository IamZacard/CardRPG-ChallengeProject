using UnityEngine;

public abstract class Card : ScriptableObject
{
    public string cardName;
    public int cost;
    public string description;
    public Sprite artwork;

    // Abstract method to define card behavior when played
    public abstract void Play();
}