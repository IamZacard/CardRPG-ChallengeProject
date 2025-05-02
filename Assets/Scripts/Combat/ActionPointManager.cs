using UnityEngine;

public class ActionPointManager : MonoBehaviour
{
    public int maxActionPoints = 3;
    public int currentActionPoints;

    void Start()
    {
        currentActionPoints = maxActionPoints;
    }

    public bool SpendActionPoints(int amount)
    {
        if (currentActionPoints >= amount)
        {
            currentActionPoints -= amount;
            return true;
        }
        Debug.Log("Not enough action points!");
        return false;
    }

    public void ResetActionPoints()
    {
        currentActionPoints = maxActionPoints;
    }
}