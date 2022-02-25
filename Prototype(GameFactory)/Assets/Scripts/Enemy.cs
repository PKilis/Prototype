using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
 
    [Header("Move Settings")]
    [SerializeField] float chaseRange = 5f;
    [SerializeField] float turnSpeed = 15f;
    [SerializeField] float patrolRadius = 9f;
    [SerializeField] float patrolWaitTime = 2f;
    [SerializeField] float chaseSpeed = 4f;
    [SerializeField] float searchSpeed = 3f;
    [Header("Attack Settings")]
    [SerializeField] int damage = 2;
    [SerializeField] float attackRate = 2;
    [SerializeField] float attackRange = 1.5f;

    private bool isSearched = false;
    private bool isAttacking = false;

    private Animator anim;
    private NavMeshAgent agent;
    private Transform player;
    enum Status
    {
        Idle,
        Search,
        Chase,
        Attack
    }
    [SerializeField] private Status currentState;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        StateCheck();
        StateExecute();
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        switch (currentState)
        {
            case Status.Search:
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, agent.destination);
                break;
            case Status.Chase:
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, player.position);
                break;
            case Status.Attack:
                Gizmos.color = Color.black;
                Gizmos.DrawLine(transform.position, player.position);
                break;
            default:
                break;
        }
    }
    private void StateCheck()
    {
        float distanceToTarget = Vector3.Distance(player.position, transform.position);
        if (distanceToTarget <= chaseRange && distanceToTarget > attackRange)
        {
            currentState = Status.Chase;
        }
        else if (distanceToTarget <= attackRange)
        {
            currentState = Status.Attack;
        }
        else
        {
            currentState = Status.Search;
        }
    }
    private void StateExecute()
    {
        switch (currentState)
        {
            case Status.Idle:
                break;
            case Status.Search:
                if (!isSearched && agent.remainingDistance <= 0.1f || !isSearched && !agent.hasPath)
                {
                    Vector3 agentTarget = new Vector3(agent.destination.x, transform.position.y, agent.destination.z);
                    agent.enabled = false;
                    transform.position = agentTarget;
                    agent.enabled = true;
                    Invoke("Search", patrolWaitTime);
                    isSearched = true;
                    anim.SetBool("Walk", false);
                }
                break;
            case Status.Chase:
                Chase();
                break;
            case Status.Attack:
                Attack();
                break;
        }
    }
    private void Search()
    {
        agent.isStopped = false;
        agent.speed = searchSpeed;
        isSearched = false;
        anim.SetBool("Walk", true);
        agent.SetDestination(GetRandomPositions());
    }
    private void Attack()
    {
        if (player == null)
        {
            return;
        }
        if (!isAttacking)
        {
            StartCoroutine(AttackRoutine());
        }
        anim.SetBool("Walk", false);
        anim.SetTrigger("Attack");
        agent.isStopped = true;
        LookTheTarget(player.position);
    }
    private void Chase()
    {
        if (player == null)
        {
            return;
        }
        agent.isStopped = false;
        anim.SetBool("Walk", true);
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }
    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        yield return new WaitForSeconds(attackRate);
        anim.SetTrigger("Attack");
        yield return new WaitUntil(() => isAttackingAnimatorFinished("Enemy Attack"));
        isAttacking = false;
    }
    private bool isAttackingAnimatorFinished(string animationName)
    {
        if (!anim.IsInTransition(0) && anim.GetCurrentAnimatorStateInfo(0).IsName(animationName) 
            && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= .95f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void LookTheTarget(Vector3 target)
    {
        Vector3 lookPos = new Vector3(target.x, transform.position.y, target.z);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPos - transform.position),
            turnSpeed * Time.deltaTime);
    }
    private Vector3 GetRandomPositions()
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas);
        return hit.position;
    }

    public int GetDamage()
    {
        return damage;
    }
}
