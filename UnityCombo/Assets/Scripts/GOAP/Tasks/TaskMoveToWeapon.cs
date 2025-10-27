using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskMoveToWeapon : Task
{
    [SerializeField]
    float stopDistance = 0.8f;
    [SerializeField]
    float costDistMultiplier = 0.5f;

    public override void UpdateTask()
    {
        enemy.agent.isStopped = false;
        enemy.agent.destination = enemy.weaponPos.position;

        if (Vector3.Distance(enemy.transform.position, enemy.weaponPos.position) <= stopDistance)
        {
            Debug.Log("Arrived at weapon pos");
            enemy.agent.isStopped = true;
            onDone.Invoke();
        }
    }

    public override WorldState ApplyEffects(WorldState inWorld)
    {
        WorldState world = Instantiate(inWorld);
        world.isNearWeapon = true;
        world.isNearPlayer = false;

        return world;
    }

    public override float GetCost(WorldState inWorld)
    {
        return baseCost + costDistMultiplier * Vector3.Distance(enemy.transform.position, enemy.weaponPos.position);
    }
}
