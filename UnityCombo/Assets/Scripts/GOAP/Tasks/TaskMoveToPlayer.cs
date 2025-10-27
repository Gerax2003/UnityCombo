using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskMoveToPlayer : Task
{
    [SerializeField]
    float stopDistance = 0.8f;
    [SerializeField]
    float costDistMultiplier = 0.5f;

    public override void UpdateTask()
    {
        enemy.agent.isStopped = false;
        enemy.agent.destination = enemy.player.transform.position;

        if (Vector3.Distance(enemy.transform.position, enemy.player.transform.position) <= stopDistance)
        {
            Debug.Log("Arrived at player pos");
            enemy.agent.isStopped = true;
            onDone.Invoke();
        }
    }

    public override WorldState ApplyEffects(WorldState inWorld)
    {
        WorldState world = Instantiate(inWorld);
        world.isNearWeapon = false;
        world.isNearPlayer = true;

        return world;
    }

    public override float GetCost(WorldState inWorld)
    {
        return baseCost + costDistMultiplier * Vector3.Distance(enemy.transform.position, enemy.player.transform.position);
    }
}
