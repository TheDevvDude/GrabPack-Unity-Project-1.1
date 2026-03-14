using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class HuggyAI : MonoBehaviour
{
    public Animator animator;

    public float patrolAnimSpeed = 0.6f;
    public float searchAnimSpeed = 0.8f;
    public float chaseAnimSpeed = 1.2f;

    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;
    public float searchSpeed = 3f;

    public float visionRange = 15f;
    public float visionAngle = 90f;
    public float loseSightTime = 3f;
    public float searchTime = 5f;
    public LayerMask visionMask;

    public Transform[] patrolPoints;
    public float minPauseTime = 1f;
    public float maxPauseTime = 3f;

    private bool isPaused;
    private float pauseTimer;
    private int lastPatrolIndex = -1;

    private NavMeshAgent agent;
    public Transform player;

    private int patrolIndex;
    private float lostSightTimer;
    private float searchTimer;
    private Vector3 lastKnownPosition;

    public enum State { Patrol, Chase, Search, Jumpscare }
    public State currentState;

    public Transform visionOrigin;
    public GameObject jumpscareCam;

    private bool jumpscared = false;

    public AudioSource audiosource;
    public AudioClip alert;

    private bool hasplayed = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        ChangeState(State.Patrol);
    }

    void Update()
    {
        Debug.DrawRay(visionOrigin.position, visionOrigin.forward * 5f, Color.blue);

        switch (currentState)
        {
            case State.Patrol:
                Patrol();

                if (CanSeePlayer())
                {
                    
                    ChangeState(State.Chase);
                }
                break;

            case State.Chase:
                Chase();
                break;

            case State.Search:
                Search();
                break;
        }


    }
    void LateUpdate()
    {
        if (currentState == State.Jumpscare)
        {
            if (!jumpscared)
            {
                animator.SetTrigger("Jumpscare");
                jumpscared = true;
            }
            return;
        }

        bool isTryingToMove =
            currentState == State.Chase ||
            currentState == State.Search ||
            (currentState == State.Patrol && !isPaused);

        bool isActuallyMoving =
            agent.hasPath &&
            agent.remainingDistance > agent.stoppingDistance &&
            agent.velocity.sqrMagnitude > 0.05f;

        animator.SetBool("IsMoving", isTryingToMove && isActuallyMoving);
    }

    void ChangeState(State newState)
    {
        currentState = newState;

        switch (newState)
        {
            case State.Patrol:
                agent.speed = patrolSpeed;
                animator.speed = patrolAnimSpeed;
                GoToRandomPatrolPoint();
                hasplayed = false;
                break;

            case State.Chase:
                agent.speed = chaseSpeed;
                animator.speed = chaseAnimSpeed;
                break;

            case State.Search:
                agent.speed = searchSpeed;
                animator.speed = searchAnimSpeed;
                agent.SetDestination(lastKnownPosition);
                searchTimer = 0f;
                hasplayed = false;
                break;

            case State.Jumpscare:
                agent.speed = 0f;
                animator.speed = 1.0f;
                hasplayed = false;
                break;
        }
    }

    void Patrol()
    {
        if (isPaused)
        {
            pauseTimer -= Time.deltaTime;

            if (pauseTimer <= 0f)
            {
                isPaused = false;
                GoToRandomPatrolPoint();
            }

            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            isPaused = true;
            pauseTimer = Random.Range(minPauseTime, maxPauseTime);
            agent.ResetPath(); 
        }
    }

    void Chase()
    {
        agent.SetDestination(player.position);

        if (!hasplayed)
        {
            audiosource.PlayOneShot(alert, 1.5f);
            hasplayed = true;
        }

        if (CanSeePlayer())
        {
            lastKnownPosition = player.position;
            lostSightTimer = 0f;
        }
        else
        {
            lostSightTimer += Time.deltaTime;

            if (lostSightTimer >= loseSightTime)
            {
                ChangeState(State.Search);
            }
        }
    }

    void Search()
    {
        searchTimer += Time.deltaTime;

        if (CanSeePlayer())
        {
            ChangeState(State.Chase);
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.ResetPath();
        }

        if (searchTimer >= searchTime)
        {
            ChangeState(State.Patrol);
        }
    }

    void GoToRandomPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        int newIndex;

        do
        {
            newIndex = Random.Range(0, patrolPoints.Length);
        }
        while (newIndex == lastPatrolIndex && patrolPoints.Length > 1);

        lastPatrolIndex = newIndex;
        agent.SetDestination(patrolPoints[newIndex].position);
    }

    bool CanSeePlayer()
    {
        if (player == null || visionOrigin == null)
            return false;

        Vector3 eyePos = visionOrigin.position;
        Vector3 dirToPlayer = (player.position - eyePos);
        float distance = dirToPlayer.magnitude;

        if (distance > visionRange)
            return false;

        float angle = Vector3.Angle(visionOrigin.forward, dirToPlayer);

        if (angle > visionAngle * 0.5f)
            return false;

        RaycastHit hit;
        if (Physics.Raycast(eyePos, dirToPlayer.normalized, out hit, visionRange, visionMask, QueryTriggerInteraction.Collide))
        {

            if (hit.transform.CompareTag("Player"))
                return true;
        }

        return false;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb == null) return;
            Jumpscare();
        }
    }

    void Jumpscare()
    {
        ChangeState(State.Jumpscare);

        jumpscareCam.SetActive(true);
        player.gameObject.SetActive(false);
    }

    public void reset()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}