using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public List<CardData> cardsInHand = new List<CardData>();
    public int maxHandSize = 5;

    public void AddCard(CardData card)
    {
        if (cardsInHand.Count < maxHandSize)
        {
            cardsInHand.Add(card);
        }
        else
        {
            Debug.Log("Hand is full!");
        }
    }

    public void RemoveCard(CardData card)
    {
        if (cardsInHand.Contains(card))
        {
            cardsInHand.Remove(card);
        }
    }
}