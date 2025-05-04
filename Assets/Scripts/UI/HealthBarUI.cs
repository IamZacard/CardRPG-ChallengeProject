using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    private Entity entity;

    public void Initialize(Entity entity)
    {
        this.entity = entity;
        UpdateHealth();
    }

    public void UpdateHealth()
    {
        healthSlider.value = entity.Health;
        healthText.text = $"{entity.Health}/100";
    }
}