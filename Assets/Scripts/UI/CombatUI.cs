using UnityEngine;

public class CombatUI : MonoBehaviour
{
    public void UpdateHealthDisplay(int currentHealth, int maxHealth)
    {
        Debug.Log($"Health: {currentHealth}/{maxHealth}");
        // Update UI elements here
    }

    public void UpdateActionPoints(int currentAP, int maxAP)
    {
        Debug.Log($"Action Points: {currentAP}/{maxAP}");
        // Update UI elements here
    }
}