using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskAttack : Task
{
    [SerializeField]
    float costNoWeapon = 10f;

    public override void UpdateTask()
    {
        if (Vector3.Distance(enemy.transform.position, enemy.player.transform.position) <= enemy.attackRange)
        {
            Debug.Log("Attacked player");
            enemy.player.life = 0;
            onDone.Invoke();
        }
        else
        {
            Debug.Log("Failed attack: too far");
            onFailed.Invoke();
        }
    }

    public override WorldState ApplyEffects(WorldState inWorld)
    {
        WorldState world = Instantiate(inWorld);
        world.playerAlive = false;
        world.isNearPlayer = false;

        return world;
    }

    public override bool CheckConditions(WorldState inWorld)
    {
        return inWorld.isNearPlayer && inWorld.playerAlive;
    }

    public override float GetCost(WorldState inWorld)
    {
        return inWorld.enemyHasWeapon ? baseCost : costNoWeapon;
    }
}
