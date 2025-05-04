using UnityEngine;

public class DefenseCard : Card
{
    public override void Play(Entity target)
    {
        if (target != null)
        {
            target.AddBlock(Data.Block);
            OnCardPlayed?.Invoke();
        }
    }
}