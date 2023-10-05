using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum AIstate
{
    Idle,
    Wandering,
    Attacking,
    Fleeing
}

public class NPC : MonoBehaviour, IDamagable
{
    [Header("# Stats")]
    public int health;
    public float walkSpeed;
    public float runSpeed;
    public ItemData[] dropOnDeath;

    [Header("# AI")]
    private AIstate aiState;
    public float detectDistance;
    public float safeDistance;

    [Header("# Wandering")]
    public float minWanderDistance;
    public float maxWanderDistance;
    public float minWanderWaitTime;
    public float maxWanderWaitTime;

    [Header("# Combat")]
    public int damage;
    public float attackRate;
    private float lastAttackTime;
    public float attackDistance;

    private float playerDistance;

    public float fieldOfView = 120f;

    private NavMeshAgent agent;
    private Animator animator;
    private SkinnedMeshRenderer[] meshRenderers;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    private void Start()
    {
        SetState(AIstate.Wandering);
    }

    private void Update()
    {
        playerDistance = Vector3.Distance(transform.position, PlayerController.instance.transform.position);

        animator.SetBool("Moving", aiState != AIstate.Idle);

        switch(aiState)
        {
            case AIstate.Idle: PassiveUpdate(); break;
            case AIstate.Wandering: PassiveUpdate(); break;
            case AIstate.Attacking: AttackingUpdate(); break;
            case AIstate.Fleeing: FleeingUpdate(); break;
        }
    }

    private void FleeingUpdate()
    {
        if(agent.remainingDistance < 0.1f)
        {
            agent.SetDestination(GetFleeLocation());
        }
        else
        {
            SetState(AIstate.Wandering);
        }
    }

    private void AttackingUpdate()
    {
        if(playerDistance > attackDistance || IsPlaterInFieldOfView())
        {
            agent.isStopped = false;
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(PlayerController.instance.transform.position, path))
            {
                agent.SetDestination(PlayerController.instance.transform.position);
            }
            else
            {
                SetState(AIstate.Fleeing);
            }
        }
        else
        {
            agent.isStopped = true;
            if(Time.time - lastAttackTime > attackRate)
            {
                lastAttackTime = Time.time;
                PlayerController.instance.GetComponent<IDamagable>().TakePhysicalDamage(damage);
                animator.speed = 1f;
                animator.SetTrigger("Attack");
            }
        }
    }

    private void PassiveUpdate()
    {
        if(aiState == AIstate.Wandering && agent.remainingDistance < 0.1f)
        {
            SetState(AIstate.Idle);
            Invoke(nameof(WanderToNewLocation), Random.Range(minWanderWaitTime, maxWanderWaitTime));
        }
        
        if(playerDistance < detectDistance)
        {
            SetState(AIstate.Attacking);
        }
    }

    bool IsPlaterInFieldOfView()
    {
        Vector3 directionToPlayer = PlayerController.instance.transform.position - transform.position;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        return angle < fieldOfView * 0.5f;
    }



    private void SetState(AIstate newState)
    {
        aiState = newState;
        switch(aiState)
        {
            case AIstate.Idle:
                {
                    agent.speed = walkSpeed;
                    agent.isStopped = true;
                }
                break;
            case AIstate.Wandering:
                {
                    agent.speed = walkSpeed;
                    agent.isStopped = false;
                }
                break;
            case AIstate.Attacking:
                {
                    agent.speed = runSpeed;
                    agent.isStopped = false;
                }
                break;
            case AIstate.Fleeing:
                {
                    agent.speed = runSpeed;
                    agent.isStopped = false;
                }
                break;
        }
        animator.speed = agent.speed / walkSpeed;
    }

    void WanderToNewLocation()
    {
        if(aiState != AIstate.Idle)
        {
            return;
        }
        SetState(AIstate.Wandering);
        agent.SetDestination(GetWanderLocation());
    }

    Vector3 GetWanderLocation()
    {
        NavMeshHit hit;
        NavMesh.SamplePosition(transform.position + (Random.onUnitSphere) * Random.Range(minWanderDistance,maxWanderDistance), out hit, maxWanderDistance, NavMesh.AllAreas);

        int i = 0;
        while(Vector3.Distance(transform.position, hit.position) < detectDistance)
        {
            NavMesh.SamplePosition(transform.position + (Random.onUnitSphere) * Random.Range(minWanderDistance, maxWanderDistance), out hit, maxWanderDistance, NavMesh.AllAreas);
            i++;
            if (i == 30)
                break;
        }

        return hit.position;
    }

    Vector3 GetFleeLocation()
    {
        NavMeshHit hit;
        NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * safeDistance), out hit, maxWanderDistance, NavMesh.AllAreas);

        int i = 0;
        while (GetDestinationAngle(hit.position) > 90 || playerDistance < safeDistance)
        {
            NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * safeDistance), out hit, maxWanderDistance, NavMesh.AllAreas);
            i++;
            if (i == 30)
                break;
        }

        return hit.position;
    }

    float GetDestinationAngle(Vector3 targetPos)
    {
        return Vector3.Angle(transform.position - PlayerController.instance.transform.position, transform.position + targetPos);
    }

    public void TakePhysicalDamage(int damageAmount)
    {
        health -= damageAmount;
        if (health <= 0)
            Die();

        StartCoroutine(DamageFlash());
    }

    void Die()
    {
        for(int x = 0; x < dropOnDeath.Length; x++)
        {
            Instantiate(dropOnDeath[x].dropPrefab, transform.position + Vector3.up * 2, Quaternion.identity);
        }
    }

    IEnumerator DamageFlash()
    {
        for(int x = 0; x<meshRenderers.Length; x++)
        {
            meshRenderers[x].material.color = new Color(1.0f, 0.6f, 0.6f);
        }

        yield return new WaitForSeconds(0.1f);

        for(int x = 0; x<meshRenderers.Length; x++)
        {
            meshRenderers[x].material.color = Color.white;
        }
    }
}
