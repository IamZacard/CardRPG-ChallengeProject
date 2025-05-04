using UnityEngine;

public class RewardSystem : MonoBehaviour
{
    public void GrantReward(Player player)
    {
        // Simplified: Add a random card
        CardData newCard = Resources.Load<CardData>("Cards/CommonAttack");
        //player.GetComponent<Deck>().AddCard(newCardFactory.CreateCard(newCard));
    }
}