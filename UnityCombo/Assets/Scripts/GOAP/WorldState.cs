using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldState", menuName = "ScriptableObjects/WorldState", order = 1)]
public class WorldState : ScriptableObject
{
    public bool isNearPlayer = false;
    public bool isNearWeapon = false;
    public bool enemyHasWeapon = false;
    public bool playerAlive = true;
    public bool isHurt = true;

    public static bool CheckGoal(WorldState world, WorldState goal)
    {
        return world.isHurt == goal.isHurt&&
            world.playerAlive == goal.playerAlive &&
            world.isNearWeapon == goal.isNearWeapon &&
            world.isNearPlayer == goal.isNearPlayer &&
            world.enemyHasWeapon == goal.enemyHasWeapon;
        
        // Equals doesn't work because it checks memory location rather than content
        //return world.Equals(goal);
    }

    public override string ToString()
    {
        return "World state:\n -Near player: " + isNearPlayer.ToString() +
            "\n -Near weapon: " + isNearWeapon.ToString() +
            "\n -Enemy has weapon: " + enemyHasWeapon.ToString() +
            "\n -Player alive: " + playerAlive.ToString() +
            "\n -Enemy hurt: " + isHurt.ToString();
    }
}
