using UnityEngine;
using System.Collections.Generic;

public class EventManager : MonoBehaviour
{
    private Dictionary<string, System.Action> eventDictionary = new Dictionary<string, System.Action>();

    public void StartListening(string eventName, System.Action listener)
    {
        if (eventDictionary.TryGetValue(eventName, out System.Action thisEvent))
        {
            thisEvent += listener;
            eventDictionary[eventName] = thisEvent;
        }
        else
        {
            thisEvent += listener;
            eventDictionary.Add(eventName, thisEvent);
        }
    }

    public void StopListening(string eventName, System.Action listener)
    {
        if (eventDictionary.TryGetValue(eventName, out System.Action thisEvent))
        {
            thisEvent -= listener;
            eventDictionary[eventName] = thisEvent;
        }
    }

    public void TriggerEvent(string eventName)
    {
        if (eventDictionary.TryGetValue(eventName, out System.Action thisEvent))
        {
            thisEvent?.Invoke();
        }
    }
}