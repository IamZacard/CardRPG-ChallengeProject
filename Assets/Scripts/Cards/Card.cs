using UnityEngine;
using UnityEngine.Events;

public abstract class Card : MonoBehaviour
{
    public CardData Data { get; private set; }
    public UnityEvent OnCardPlayed = new UnityEvent();
    public UnityEvent OnCardDiscarded = new UnityEvent();
    public UnityEvent OnCardExhausted = new UnityEvent();

    public void Initialize(CardData data) => Data = data;

    public bool CanBePlayed(int currentEnergy) => currentEnergy >= Data.Cost;

    public abstract void Play(Entity target);

    public virtual string GetCurrentDescription() => Data.Description;
}