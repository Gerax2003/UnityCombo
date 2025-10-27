using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskHeal : Task
{
    public override void UpdateTask()
    {
        Debug.Log("Healing self");
        enemy.life = enemy.maxLife;
        onDone.Invoke();
    }

    public override WorldState ApplyEffects(WorldState inWorld)
    {
        WorldState world = Instantiate(inWorld);

        world.isHurt = false;

        return world;
    }

    public override bool CheckConditions(WorldState inWorld)
    {
        return inWorld.isHurt;
    }
}
