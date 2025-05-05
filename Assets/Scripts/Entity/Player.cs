// Player.cs - Player character implementation
using UnityEngine;
using System;
using System.Collections.Generic;

public class Player : Entity
{
    [Header("Player Specific")]
    public int baseActionPoints = 3;
    public int currentActionPoints;

    private Deck playerDeck;
    private Hand playerHand;
    private Deck drawPile;
    private Deck discardPile;
    private Deck exhaustPile;

    public event Action<int> OnActionPointsChanged;
    public event Action<List<Card>> OnHandChanged;

    public int CurrentActionPoints => currentActionPoints;
    public int BaseActionPoints => baseActionPoints;

    protected override void Awake()
    {
        base.Awake();

        playerDeck = new Deck();
        playerHand = new Hand();
        drawPile = new Deck();
        discardPile = new Deck();
        exhaustPile = new Deck();
    }

    public void InitializeForCombat()
    {
        base.Initialize();

        // Reset action points
        currentActionPoints = baseActionPoints;

        // Clear all piles
        playerHand.Clear();
        drawPile.Clear();
        discardPile.Clear();
        exhaustPile.Clear();

        // Copy all cards from player deck to draw pile
        foreach (var card in playerDeck.GetAllCards())
            drawPile.AddCard(card);

        // Shuffle the draw pile
        drawPile.Shuffle();

        // Draw initial hand
        DrawToHandSize(5);
        OnActionPointsChanged?.Invoke(currentActionPoints);
    }

    public override void OnStartTurn()
    {
        base.OnStartTurn();

        // Reset action points
        SetActionPoints(baseActionPoints);

        // Draw cards
        DrawToHandSize(5);
    }

    public override void OnEndTurn()
    {
        base.OnEndTurn();

        // Discard hand
        DiscardHand();
    }

    public void SetActionPoints(int amount)
    {
        int oldAP = currentActionPoints;
        currentActionPoints = Mathf.Max(0, amount);

        if (oldAP != currentActionPoints)
            OnActionPointsChanged?.Invoke(currentActionPoints);
    }

    /// <summary>
    /// Deducts the given amount from the player's current action points.
    /// </summary>
    public void ConsumeActionPoints(int amount)
    {
        SetActionPoints(currentActionPoints - amount);
    }

    public void AddCard(Card card)
    {
        playerDeck.AddCard(card);
    }

    public void RemoveCard(Card card)
    {
        playerDeck.RemoveCard(card);
    }

    public void DrawToHandSize(int targetHandSize)
    {
        int cardsToDraw = targetHandSize - playerHand.Count;

        for (int i = 0; i < cardsToDraw; i++)
        {
            // If draw pile is empty, shuffle discard into draw
            if (drawPile.IsEmpty && !discardPile.IsEmpty)
            {
                foreach (var card in discardPile.GetAllCards())
                    drawPile.AddCard(card);
                discardPile.Clear();
                drawPile.Shuffle();
            }

            // Draw a card if possible
            if (!drawPile.IsEmpty)
            {
                Card drawnCard = drawPile.DrawCard();
                playerHand.AddCard(drawnCard);
            }
            else
            {
                break;
            }
        }

        OnHandChanged?.Invoke(playerHand.GetAllCards());
    }

    public void DiscardHand()
    {
        foreach (var card in playerHand.GetAllCards())
            discardPile.AddCard(card);

        playerHand.Clear();
        OnHandChanged?.Invoke(playerHand.GetAllCards());
    }

    public bool PlayCard(Card card, Entity target)
    {
        if (!playerHand.GetAllCards().Contains(card))
        {
            Debug.LogWarning("Attempting to play a card that is not in hand");
            return false;
        }

        if (card.CanBePlayed(GetCombatState()))
        {
            playerHand.RemoveCard(card);
            card.Play(GetCombatState(), target);
            OnHandChanged?.Invoke(playerHand.GetAllCards());
            return true;
        }

        return false;
    }

    public Hand GetHand() => playerHand;
    public Deck GetPlayerDeck() => playerDeck;
    public Deck GetDrawPile() => drawPile;
    public Deck GetDiscardPile() => discardPile;
    public Deck GetExhaustPile() => exhaustPile;

    public CombatState GetCombatState()
    {
        // This would be implemented better with a proper reference to the CombatManager
        CombatState state = new CombatState
        {
            Player = this,
            Enemies = FindObjectsOfType<Enemy>(),
            DrawPile = drawPile,
            DiscardPile = discardPile,
            ExhaustPile = exhaustPile
        };
        return state;
    }

    public void SetupStarterDeck()
    {
        if (playerDeck.Count == 0)
        {
            List<Card> starterCards = CardFactory.CreateStarterDeck();
            foreach (var card in starterCards)
                playerDeck.AddCard(card);
        }
    }
}
