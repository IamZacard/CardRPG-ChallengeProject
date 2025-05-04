public class Enemy : Entity
{
    public void PerformAction()
    {
        // Simple AI: Deal 10 damage to player
        FindObjectOfType<Player>().TakeDamage(10);
    }
}