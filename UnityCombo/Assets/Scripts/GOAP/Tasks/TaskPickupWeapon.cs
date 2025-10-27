using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskPickupWeapon : Task
{
    public override void UpdateTask()
    {
        if (Vector3.Distance(enemy.transform.position, enemy.weaponPos.position) <= enemy.pickupDistance)
        {
            Debug.Log("Picked up weapon");
            enemy.weapon.SetActive(true);
            onDone.Invoke();
        }
        else
        {
            Debug.Log("Failed weapon pickup: too far");
            onFailed.Invoke();
        }
    }

    public override WorldState ApplyEffects(WorldState inWorld)
    {
        WorldState world = Instantiate(inWorld);
        world.enemyHasWeapon = true;
        world.isNearWeapon = false;

        return world;
    }

    public override bool CheckConditions(WorldState inWorld)
    {
        return inWorld.isNearWeapon && !inWorld.enemyHasWeapon;
    }
}
