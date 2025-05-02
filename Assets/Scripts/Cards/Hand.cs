// Hand.cs - Manages the player's current hand of cards
using System.Collections.Generic;
using UnityEngine;
using System;

public class Hand
{
    private List<Card> cards = new List<Card>();
    private int maxHandSize;

    public int Count => cards.Count;
    public bool IsFull => cards.Count >= maxHandSize;

    public event Action<Card> OnCardAdded;
    public event Action<Card> OnCardRemoved;

    public Hand(int maxSize = 10)
    {
        maxHandSize = maxSize;
    }

    public bool AddCard(Card card)
    {
        if (IsFull)
        {
            return false;
        }

        cards.Add(card);
        OnCardAdded?.Invoke(card);
        return true;
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

    public Card GetCard(int index)
    {
        if (index < 0 || index >= cards.Count)
        {
            return null;
        }

        return cards[index];
    }

    public List<Card> GetAllCards()
    {
        return new List<Card>(cards);
    }

    public void Clear()
    {
        List<Card> cardsCopy = new List<Card>(cards);
        foreach (var card in cardsCopy)
        {
            RemoveCard(card);
        }
    }

    public List<Card> GetPlayableCards(CombatState combatState)
    {
        return cards.FindAll(card => card.CanBePlayed(combatState));
    }
}