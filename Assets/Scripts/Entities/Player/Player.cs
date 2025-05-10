/// <summary>
/// Placeholder for the Player class, which should extend Entity
/// </summary>
public class Player : Entity
{
    public void InitializeWithData(PlayerData data)
    {
        // Initialize from PlayerData
        entityName = data.playerName;
        maxHealth = data.maxHealth;
        currentHealth = data.currentHealth;
        maxEnergy = data.maxEnergy;

        // Initialize status effects, etc.
        Initialize();
    }
}