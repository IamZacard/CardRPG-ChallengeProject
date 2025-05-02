using UnityEngine;
using System;

public class CombatUI : MonoBehaviour
{
    public event Action<Card> OnCardSelected;
    public event Action<Entity> OnEntitySelected;
    public event Action OnEndTurnButtonClicked;

    public void UpdateUI(CombatState combatState)
    {
        if (combatState == null) return;

        UpdateHealthDisplay(combatState.Player.CurrentHealth, combatState.Player.MaxHealth);
        UpdateActionPoints(combatState.Player.CurrentActionPoints, combatState.Player.baseActionPoints);

        // TODO: Add updates for card hand, enemy displays, etc.
        Debug.Log("UI updated with current combat state.");
    }

    public void UpdateHealthDisplay(int currentHealth, int maxHealth)
    {
        Debug.Log($"Health: {currentHealth}/{maxHealth}");
        // Implement actual UI update here
    }

    public void UpdateActionPoints(int currentAP, int maxAP)
    {
        Debug.Log($"Action Points: {currentAP}/{maxAP}");
        // Implement actual UI update here
    }

    public void EnableTargetMode(bool enabled)
    {
        Debug.Log($"Target mode {(enabled ? "enabled" : "disabled")}");
        // Implement targeting UI logic
    }

    public void HighlightEntity(Entity entity)
    {
        if (entity != null)
        {
            Debug.Log($"Highlighted entity: {entity.name}");
            // Implement highlight effect
        }
    }

    public void UnhighlightEntity(Entity entity)
    {
        if (entity != null)
        {
            Debug.Log($"Unhighlighted entity: {entity.name}");
            // Remove highlight effect
        }
    }

    // UI interaction methods (connect to buttons in Unity Inspector)
    public void SelectCard(Card card)
    {
        OnCardSelected?.Invoke(card);
    }

    public void SelectEntity(Entity entity)
    {
        OnEntitySelected?.Invoke(entity);
    }

    public void EndTurnButtonClicked()
    {
        OnEndTurnButtonClicked?.Invoke();
    }
}