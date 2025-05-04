using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Deck : MonoBehaviour
{
    private List<Card> cards = new List<Card>();
    public UnityEvent OnDeckShuffled = new UnityEvent();
    public UnityEvent OnCardDrawn = new UnityEvent();
    public int Count => cards.Count;
    public bool IsEmpty => cards.Count == 0;

    public void AddCard(Card card) => cards.Add(card);

    public void Shuffle()
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Card temp = cards[i];
            cards[i] = cards[j];
            cards[j] = temp;
        }
        OnDeckShuffled?.Invoke();
    }

    public Card Draw()
    {
        if (IsEmpty) return null;
        Card card = cards[0];
        cards.RemoveAt(0);
        OnCardDrawn?.Invoke();
        return card;
    }

    internal void AddCard(object value)
    {
        throw new System.NotImplementedException();
    }
}