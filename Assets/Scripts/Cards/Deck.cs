using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public List<CardData> cards = new List<CardData>();

    // Shuffles the deck using Fisher-Yates algorithm
    public void Shuffle()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            CardData temp = cards[i];
            int randomIndex = Random.Range(i, cards.Count);
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }
    }

    // Draws the top card and removes it from the deck
    public CardData DrawCard()
    {
        if (cards.Count > 0)
        {
            CardData drawnCard = cards[0];
            cards.RemoveAt(0);
            return drawnCard;
        }
        Debug.Log("Deck is empty!");
        return null;
    }
}