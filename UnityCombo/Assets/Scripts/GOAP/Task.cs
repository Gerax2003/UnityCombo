using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Task : MonoBehaviour
{
    protected Enemy enemy;

    [SerializeField]
    public string taskName = "Task";

    [SerializeField]
    protected float baseCost = 1f;

    [HideInInspector]
    public UnityEvent onDone = new UnityEvent();
    [HideInInspector]
    public UnityEvent onFailed = new UnityEvent();

    private void Start()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    public virtual void UpdateTask()
    {
        onDone.Invoke();
    }

    public virtual WorldState ApplyEffects(WorldState inWorld)
    {
        WorldState world = Instantiate(inWorld);
        return world;
    }

    public virtual float GetCost(WorldState inWorld)
    {
        return baseCost;
    }

    public virtual bool CheckConditions(WorldState inWorld)
    {
        return true;
    }
}
