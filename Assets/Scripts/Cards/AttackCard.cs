public class AttackCard : Card
{
    public override void Play(Entity target)
    {
        if (target != null)
        {
            target.TakeDamage(Data.Damage);
            OnCardPlayed?.Invoke();
        }
    }
}