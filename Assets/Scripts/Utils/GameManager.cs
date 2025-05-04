using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private CombatState combatState;
    [SerializeField] private MapUI mapUI;
    [SerializeField] private MapGenerator mapGenerator;

    private void Start()
    {
        // Initialize deck with sample cards
        Deck deck = FindObjectOfType<Deck>();
        CardFactory factory = FindObjectOfType<CardFactory>();
        CardData[] sampleCards = Resources.LoadAll<CardData>("Cards");
        foreach (var cardData in sampleCards)
            deck.AddCard(factory.CreateCard(cardData));

        // Initialize combat
        Player player = FindObjectOfType<Player>();
        List<Enemy> enemies = new List<Enemy>(FindObjectsOfType<Enemy>());
        combatState.Initialize(player, enemies, deck, FindObjectOfType<Hand>(), FindObjectOfType<DiscardPile>());

        // Initialize map
        MapNode startNode = mapGenerator.GenerateFloor(1);
        mapUI.Initialize(startNode);
    }
}