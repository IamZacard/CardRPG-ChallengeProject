using System.Collections.Generic;
using UnityEngine;

public class CombatState : MonoBehaviour
{
    public Player Player { get; private set; }
    public List<Enemy> Enemies { get; private set; }
    public Deck Deck { get; private set; }
    public Hand Hand { get; private set; }
    public DiscardPile Discard { get; private set; }
    private bool isPlayerTurn = true;

    public void Initialize(Player player, List<Enemy> enemies, Deck deck, Hand hand, DiscardPile discard)
    {
        Player = player;
        Enemies = enemies;
        Deck = deck;
        Hand = hand;
        Discard = discard;
    }

    public void StartTurn()
    {
        if (isPlayerTurn)
        {
            DrawCards(5); // Draw 5 cards at turn start
            Player.ResetEnergy();
        }
        else
        {
            foreach (var enemy in Enemies)
                enemy.PerformAction();
            isPlayerTurn = true;
        }
    }

    public void PlayCard(Card card, Entity target = null)
    {
        if (!isPlayerTurn || !card.CanBePlayed(Player.Energy))
            return;

        // Play the card
        Player.PlayCard(card, target ?? Enemies[0]); // Default to first enemy if no target
        Hand.RemoveCard(card);

        // Handle card properties (exhaust, ethereal, etc.)
        if (card.Data.Exhaust || card.Data.Ethereal)
            FindObjectOfType<ExhaustPile>().AddCard(card);
        else
            Discard.AddCard(card);

        // Update game state
        isPlayerTurn = !CheckWin() && !CheckLose(); // End player turn if combat ends
        UpdateUI();
    }

    private void DrawCards(int count)
    {
        for (int i = 0; i < count && !Deck.IsEmpty; i++)
            Hand.AddCard(Deck.Draw());
    }

    public bool CheckWin() => Enemies.TrueForAll(e => e.Health <= 0);
    public bool CheckLose() => Player.Health <= 0;

    private void UpdateUI()
    {
        FindObjectOfType<PlayerInfoUI>()?.UpdateEnergy();
        foreach (var healthBar in FindObjectsOfType<HealthBarUI>())
            healthBar.UpdateHealth();
    }
}