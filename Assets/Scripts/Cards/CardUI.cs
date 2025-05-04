using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    public Card Card { get; private set; }
    [SerializeField] private TextMeshProUGUI nameText, descriptionText, costText;
    [SerializeField] private Image artwork;

    public void Initialize(Card card)
    {
        Card = card;
        nameText.text = card.Data.Name;
        descriptionText.text = card.GetCurrentDescription();
        costText.text = card.Data.Cost.ToString();
        artwork.sprite = card.Data.Artwork;
    }

    public void OnClick()
    {
        // Trigger card play logic
        FindObjectOfType<CombatState>().PlayCard(Card); // No target specified, defaults to first enemy
    }
}