using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Card UI Elements")]
    [SerializeField] private Image cardBackground;
    [SerializeField] private Image cardFrame;
    [SerializeField] private Image cardArt;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI blockText;
    [SerializeField] private GameObject damageIcon;
    [SerializeField] private GameObject blockIcon;
    [SerializeField] private Image rarityBorder;

    [Header("Card Type Backgrounds")]
    [SerializeField] private Sprite attackCardBackground;
    [SerializeField] private Sprite skillCardBackground;
    [SerializeField] private Sprite powerCardBackground;
    [SerializeField] private Sprite statusCardBackground;
    [SerializeField] private Sprite curseCardBackground;

    [Header("Card Type Frames")]
    [SerializeField] private Sprite attackCardFrame;
    [SerializeField] private Sprite skillCardFrame;
    [SerializeField] private Sprite powerCardFrame;
    [SerializeField] private Sprite statusCardFrame;
    [SerializeField] private Sprite curseCardFrame;

    [Header("Rarity Borders")]
    [SerializeField] private Sprite commonBorder;
    [SerializeField] private Sprite uncommonBorder;
    [SerializeField] private Sprite rareBorder;
    [SerializeField] private Sprite specialBorder;

    [Header("Animation")]
    [SerializeField] private float hoverScaleMultiplier = 1.1f;
    [SerializeField] private float hoverDuration = 0.2f;
    [SerializeField] private float dragScaleMultiplier = 1.2f;

    public Card Card { get; private set; }

    public event Action<CardUI> OnCardClicked;
    public event Action<CardUI> OnCardDragBegin;
    public event Action<CardUI> OnCardDragEnd;

    private Vector3 originalScale;
    private bool isHovering;
    private bool isDragging;

    private Canvas parentCanvas;
    private RectTransform rectTransform;
    private Vector2 dragOffset;
    private Tween scaleTween; // For safely managing scaling tweens

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = transform.localScale;
        parentCanvas = GetComponentInParent<Canvas>();
    }

    public void SetupCard(Card card)
    {
        if (card == null)
        {
            Debug.LogWarning("Attempted to setup CardUI with null card");
            return;
        }

        Card = card;

        if (nameText != null) nameText.text = card.cardData.cardName;
        if (costText != null) costText.text = card.cardData.cost.ToString();
        if (descriptionText != null) descriptionText.text = FormatDescription(card);

        if (cardArt != null)
        {
            if (card.cardData.artwork != null)
            {
                cardArt.sprite = card.cardData.artwork;
                cardArt.gameObject.SetActive(true);
            }
            else
            {
                cardArt.gameObject.SetActive(false);
            }
        }

        SetCardBackgroundForType(card.cardData.cardType);
        SetCardFrameForType(card.cardData.cardType);
        SetRarityBorder(card.cardData.rarity);
        UpdateDamageAndBlockDisplay(card);
        ApplyCardSpecificUI(card);
    }

    private string FormatDescription(Card card)
    {
        return card.GetCurrentDescription();
    }

    private void SetCardBackgroundForType(CardType cardType)
    {
        if (cardBackground == null) return;

        switch (cardType)
        {
            case CardType.Attack:
                cardBackground.sprite = attackCardBackground;
                break;
            case CardType.Defense:
                cardBackground.sprite = skillCardBackground;
                break;
            case CardType.Effect:
                cardBackground.sprite = powerCardBackground;
                break;
            default:
                cardBackground.sprite = statusCardBackground;
                break;
        }
    }

    private void SetCardFrameForType(CardType cardType)
    {
        if (cardFrame == null) return;

        switch (cardType)
        {
            case CardType.Attack:
                cardFrame.sprite = attackCardFrame;
                break;
            case CardType.Defense:
                cardFrame.sprite = skillCardFrame;
                break;
            case CardType.Effect:
                cardFrame.sprite = powerCardFrame;
                break;
            default:
                cardFrame.sprite = statusCardFrame;
                break;
        }
    }

    private void SetRarityBorder(CardRarity rarity)
    {
        if (rarityBorder == null) return;

        switch (rarity)
        {
            case CardRarity.Common:
                rarityBorder.sprite = commonBorder;
                break;
            case CardRarity.Uncommon:
                rarityBorder.sprite = uncommonBorder;
                break;
            case CardRarity.Rare:
                rarityBorder.sprite = rareBorder;
                break;
            default:
                rarityBorder.sprite = commonBorder;
                break;
        }
    }

    private void UpdateDamageAndBlockDisplay(Card card)
    {
        if (damageText != null && damageIcon != null)
        {
            if (card.cardData.damage > 0)
            {
                damageText.text = card.cardData.damage.ToString();
                damageIcon.SetActive(true);
                damageText.gameObject.SetActive(true);
            }
            else
            {
                damageIcon.SetActive(false);
                damageText.gameObject.SetActive(false);
            }
        }

        if (blockText != null && blockIcon != null)
        {
            if (card.cardData.block > 0)
            {
                blockText.text = card.cardData.block.ToString();
                blockIcon.SetActive(true);
                blockText.gameObject.SetActive(true);
            }
            else
            {
                blockIcon.SetActive(false);
                blockText.gameObject.SetActive(false);
            }
        }
    }

    private void ApplyCardSpecificUI(Card card)
    {
        if (card.cardData.exhaust)
        {
            // TODO: Add visual indicator for exhaust
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        AnimateScale(originalScale * hoverScaleMultiplier);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if (!isDragging)
        {
            AnimateScale(originalScale);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnCardClicked?.Invoke(this);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out dragOffset);

        AnimateScale(originalScale * dragScaleMultiplier);
        OnCardDragBegin?.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (parentCanvas == null || rectTransform == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform, eventData.position,
            eventData.pressEventCamera, out Vector2 localPoint);
        rectTransform.localPosition = localPoint - dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        float target = isHovering ? hoverScaleMultiplier : 1f;
        AnimateScale(originalScale * target);

        OnCardDragEnd?.Invoke(this);
    }

    private void AnimateScale(Vector3 targetScale)
    {
        scaleTween?.Kill();
        scaleTween = transform.DOScale(targetScale, hoverDuration).SetEase(Ease.OutQuad);
    }
}
