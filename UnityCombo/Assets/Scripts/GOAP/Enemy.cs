using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;
using Unity.Collections;

public class PlanNode
{
    public float cost = 0f;
    public WorldState world;
    public PlanNode parent = null;
    public Task task;

    public PlanNode(PlanNode inParent, WorldState inWorld, Task inTask)
    {
        world = inWorld;
        task = inTask;
        parent = inParent;
        cost = parent.cost + task.GetCost(world);
    }

    public PlanNode(WorldState inWorld, Task inTask)
    {
        world = inWorld;
        task = inTask;
        cost = task.GetCost(inWorld);
    }
}

[Serializable]
public struct DictPair<T>
{
    public string name;
    public T value;
}

public class Enemy : MonoBehaviour
{
    [SerializeField]
    DictPair<bool>[] dictionnary;

    [SerializeField]
    List<Task> totalTasks = new List<Task>();

    [SerializeField]
    List<Task> plannedTasks = new List<Task>();

    [SerializeField]
    WorldState goalWorld;

    WorldState world;

    public GameObject weapon;

    public float life;
    public float maxLife = 10f;

    public float pickupDistance = 1f;
    public float attackRange = 1f;

    [HideInInspector]
    public NavMeshAgent agent;

    [SerializeField]
    public Player player;
    [SerializeField]
    public Transform weaponPos;

    // Start is called before the first frame update
    void Start()
    {
        life = maxLife - 1;
        weapon.SetActive(false);

        world = ScriptableObject.CreateInstance<WorldState>();
        agent = GetComponent<NavMeshAgent>();

        GetComponentsInChildren(true, totalTasks);
        Task task = this.AddComponent<EmptyTask>();
        totalTasks.Insert(0, task);

        PlanTasks();
    }

    // Update is called once per frame
    void Update()
    {
        // Update world state
        world.isHurt = (life < maxLife);
        world.playerAlive = (player.life < player.maxLife);
        world.enemyHasWeapon = (weapon.activeSelf);

        world.isNearWeapon = (Vector3.Distance(weaponPos.position, transform.position) <= pickupDistance);
        world.isNearPlayer = (Vector3.Distance(player.transform.position, transform.position) <= attackRange);

        if (plannedTasks.Count > 0)
            plannedTasks[0].UpdateTask();
    }

    void OnTaskFailed()
    {
        Debug.Log("Task " + plannedTasks[0].taskName + " failed");
        plannedTasks[0].onDone.RemoveAllListeners();
        plannedTasks[0].onFailed.RemoveAllListeners();
        plannedTasks.Clear();
        PlanTasks();
    }

    void OnTaskDone()
    {
        Debug.Log("Task " + plannedTasks[0].taskName + " done");
        plannedTasks[0].onDone.RemoveAllListeners();
        plannedTasks[0].onFailed.RemoveAllListeners();

        plannedTasks.RemoveAt(0);

        if (plannedTasks.Count > 0)
        {
            plannedTasks[0].onDone.AddListener(OnTaskDone);
            plannedTasks[0].onFailed.AddListener(OnTaskFailed);
        }
    }

    private void PlanTasks()
    {
        Debug.Log("Starting new plan generation");
        Stopwatch stopwatch = Stopwatch.StartNew();

        List<PlanNode> newPlan = new List<PlanNode>();
        List<Task> availableTasks = new List<Task>(totalTasks);

        PlanNode baseNode = new PlanNode(world, totalTasks[0]);

        bool success = BuildGraph(baseNode, newPlan, availableTasks, goalWorld);

        if (!success)
        {
            stopwatch.Stop();
            Debug.Log("No actions found");
            plannedTasks.Clear();
            return;
        }

        Debug.Log("Graph building return " + success.ToString());

        PlanNode currentNode = newPlan[0];
        foreach (PlanNode node in newPlan)
        {
            if (node.cost < currentNode.cost)
                currentNode = node;
        }

        plannedTasks.Clear();

        while (currentNode != null)
        {
            // Remove empty task used as first node
            if (currentNode.task is not EmptyTask)
                plannedTasks.Insert(0, currentNode.task);

            currentNode = currentNode.parent;
        }

        if (plannedTasks.Count > 0)
        {
            plannedTasks[0].onDone.AddListener(OnTaskDone);
            plannedTasks[0].onFailed.AddListener(OnTaskFailed);
        }

        stopwatch.Stop();
        TimeSpan timeSpan = stopwatch.Elapsed;
        Debug.Log("Plan found in " + timeSpan.TotalSeconds + " seconds");
    }

    private bool BuildGraph(PlanNode parent, List<PlanNode> leaves, List<Task> inAvailableTasks, WorldState goal)
    {
        bool success = false;
        foreach (Task task in inAvailableTasks)
        {
            if (task.CheckConditions(parent.world)) // check preconditions
            {
                WorldState newWorld = task.ApplyEffects(parent.world);
                PlanNode node = new PlanNode(parent, newWorld, task);
                if (WorldState.CheckGoal(newWorld, goal))
                {
                    leaves.Add(node);
                    success = true;
                }
                else
                {
                    // used task is removed
                    List<Task> availableTasks = new List<Task>(inAvailableTasks);
                    availableTasks.Remove(task);

                    // recursive call on the new node
                    success = BuildGraph(node, leaves, availableTasks, goal);
                }
            }
        }
        return success;
    }
}
