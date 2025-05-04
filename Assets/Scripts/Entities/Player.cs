public class Player : Entity
{
    public void ResetEnergy() => Energy = 3;

    public void PlayCard(Card card, Entity target)
    {
        if (card.CanBePlayed(Energy))
        {
            card.Play(target);
            Energy -= card.Data.Cost;
        }
    }
}