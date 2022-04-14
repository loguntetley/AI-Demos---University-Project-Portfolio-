using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCPatrolling : MonoBehaviour
{
    [SerializeField] private Vector3[] points;
    [SerializeField] private Bullet bullet;
    [SerializeField] private Material patrolMaterial;
    [SerializeField] private Material chaseMaterial;
    [SerializeField] private Material attackMaterial;
    [SerializeField] private float chaseRange = 50.0f;
    [SerializeField] private float attackRange = 35.0f;
    [SerializeField] private float fireRate = 2.5f;
    private float nextShootTime = 0;
    private int nextpPoint = 0;
    private GameObject[] player;
    private NavMeshAgent NavMeshAgent;
    [SerializeField] private MeshRenderer meshRenderer;
    private NPCStates currentState = NPCStates.Patrol;

    private enum NPCStates
    { 
        Patrol,
        Chase,
        Attack 
    }

    void Start()
    {
        NavMeshAgent = GetComponent<NavMeshAgent>();
        NavMeshAgent.SetDestination(points[nextpPoint]);
        player = GameObject.FindGameObjectsWithTag("Player");
    }


    void Update()
    {
        if (currentState == NPCStates.Patrol)
            patrol(); 
        else if (currentState == NPCStates.Chase)
            Chase();
        else if (currentState == NPCStates.Attack)
            Attack();

        //Debug.Log("State: " + currentState);
        //Debug.Log("Player: " + player[0].transform.position);

    }

    private void patrol()
    {
        meshRenderer.material = patrolMaterial;

        if (Vector3.Distance(transform.position, player[0].transform.position) < chaseRange)
            currentState = NPCStates.Chase;

        if (!NavMeshAgent.pathPending && NavMeshAgent.remainingDistance <= NavMeshAgent.stoppingDistance)
        {
           // Debug.Log("Destination reached");
            nextpPoint++;
            if (nextpPoint >= points.Length)
                nextpPoint = 0;
            //Debug.Log("Now heading towrds: " + points[nextpPoint]);
            NavMeshAgent.SetDestination(points[nextpPoint]);
        }
    }

    private void Chase()
    {
        NavMeshAgent.SetDestination(player[0].transform.position);
        meshRenderer.material = chaseMaterial;

        if (Vector3.Distance(transform.position, player[0].transform.position) < attackRange)
            currentState = NPCStates.Attack;

        if (Vector3.Distance(transform.position, player[0].transform.position) > chaseRange)
        {
            NavMeshAgent.ResetPath();
            currentState = NPCStates.Patrol;
        }
    }

    private void Attack()
    {
        NavMeshAgent.ResetPath();
        meshRenderer.material = attackMaterial;
        transform.LookAt(player[0].transform.position);

        if (Time.time > nextShootTime)
        {
            GameObject newBullet = Instantiate(bullet.gameObject, transform.position + (transform.forward * 1.5f), Quaternion.identity);
            newBullet.GetComponent<Rigidbody>().velocity = transform.forward * bullet.speed;
            nextShootTime = Time.time + fireRate;
        }

        if (Vector3.Distance(transform.position, player[0].transform.position) > attackRange)
            currentState = NPCStates.Chase;
    }
}
