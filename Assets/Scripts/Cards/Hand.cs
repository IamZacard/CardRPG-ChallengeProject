using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    private List<Card> cards = new List<Card>();
    public int MaxHandSize = 10;

    public bool AddCard(Card card)
    {
        if (cards.Count < MaxHandSize)
        {
            cards.Add(card);
            return true;
        }
        return false;
    }

    public void RemoveCard(Card card) => cards.Remove(card);

    public List<Card> GetPlayableCards(int currentEnergy) =>
        cards.FindAll(card => card.CanBePlayed(currentEnergy));
}