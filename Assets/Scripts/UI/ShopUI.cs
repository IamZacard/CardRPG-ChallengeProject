using UnityEngine;

public class ShopUI : MonoBehaviour
{
    public void DisplayShopItems()
    {
        Debug.Log("Displaying shop items.");
        // Show available cards/items
    }

    public void BuyItem(CardData card) // Example item as a card
    {
        Debug.Log($"Bought {card.cardName}.");
        // Add to deck, deduct gold, etc.
    }
}