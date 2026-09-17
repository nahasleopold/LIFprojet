using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RobotNavigation : MonoBehaviour
{
    private NavMeshAgent agent;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }


    public bool AllerVers(Vector3 destination)
    {
        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning(
                "Le robot n'est pas placé sur le NavMesh."
            );

            return false;
        }

        return agent.SetDestination(destination);
    }


    public bool EstArrive()
    {
        if (!agent.isOnNavMesh)
            return false;

        if (agent.pathPending)
            return false;

        if (agent.remainingDistance >
            agent.stoppingDistance + 0.1f)
            return false;

        if (agent.hasPath &&
            agent.velocity.sqrMagnitude > 0.01f)
            return false;

        return true;
    }


    public void Arreter()
    {
        if (agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
    }
}