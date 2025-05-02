// Deck.cs - Handles deck operations
using System.Collections.Generic;
using UnityEngine;
using System;

public class Deck
{
    private List<Card> cards = new List<Card>();

    public int Count => cards.Count;
    public bool IsEmpty => cards.Count == 0;

    public event Action OnDeckShuffled;
    public event Action<Card> OnCardAdded;
    public event Action<Card> OnCardRemoved;

    public void AddCard(Card card)
    {
        cards.Add(card);
        OnCardAdded?.Invoke(card);
    }

    public bool RemoveCard(Card card)
    {
        bool removed = cards.Remove(card);
        if (removed)
        {
            OnCardRemoved?.Invoke(card);
        }
        return removed;
    }

    public void Shuffle()
    {
        int n = cards.Count;
        System.Random rng = new System.Random();

        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Card temp = cards[k];
            cards[k] = cards[n];
            cards[n] = temp;
        }

        OnDeckShuffled?.Invoke();
    }

    public Card DrawCard()
    {
        if (IsEmpty)
        {
            return null;
        }

        Card drawnCard = cards[0];
        cards.RemoveAt(0);
        return drawnCard;
    }

    public List<Card> DrawCards(int amount)
    {
        List<Card> drawnCards = new List<Card>();
        for (int i = 0; i < Mathf.Min(amount, cards.Count); i++)
        {
            drawnCards.Add(DrawCard());
        }
        return drawnCards;
    }

    public void Clear()
    {
        cards.Clear();
    }

    public List<Card> GetAllCards()
    {
        return new List<Card>(cards);
    }
}