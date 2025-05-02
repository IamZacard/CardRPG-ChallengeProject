using UnityEngine;

public class CardUI : MonoBehaviour
{
    public GameObject cardPrefab;

    public void DisplayHand(Hand hand)
    {
        Debug.Log($"Displaying hand with {hand.cardsInHand.Count} cards.");
        // Instantiate card UI prefabs for each card in hand
    }
}