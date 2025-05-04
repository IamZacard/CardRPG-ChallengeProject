public class PoisonEffect : StatusEffect
{
    private int damagePerTurn;

    public void Initialize(int duration, int damage)
    {
        Duration = duration;
        damagePerTurn = damage;
    }

    public override void Apply(Entity target)
    {
        target.TakeDamage(damagePerTurn);
        Duration--;
    }

    public override void Remove(Entity target) { /* Cleanup if needed */ }
}