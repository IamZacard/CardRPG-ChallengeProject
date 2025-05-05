using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatUI : MonoBehaviour
{
    public Action<Card> OnCardSelected { get; internal set; }
    public Action<Entity> OnEntitySelected { get; internal set; }
    public Action OnEndTurnButtonClicked { get; internal set; }

    internal void EnableTargetMode(bool v)
    {
        throw new NotImplementedException();
    }

    internal void HighlightEntity(Enemy enemy)
    {
        throw new NotImplementedException();
    }

    internal void UnhighlightEntity(Enemy enemy)
    {
        throw new NotImplementedException();
    }

    internal void UpdateUI(CombatState combatState)
    {
        throw new NotImplementedException();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
