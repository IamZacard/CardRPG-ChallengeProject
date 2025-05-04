using TMPro;
using UnityEngine;

public class PlayerInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI energyText;
    private Player player;

    private void Start()
    {
        player = FindObjectOfType<Player>();
        UpdateEnergy();
    }

    public void UpdateEnergy()
    {
        energyText.text = $"Energy: {player.Energy}/3";
    }

    public void OnEndTurn()
    {
        FindObjectOfType<CombatState>().StartTurn();
    }
}