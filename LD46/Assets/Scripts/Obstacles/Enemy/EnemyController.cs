﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    enum State { Idle, Patrol, Chase, Hit }
    private State currentState;

    [Header("Sounds")]
    public AudioClip[] shoutsSound;
    public AudioClip[] attackSound;
    private AudioSource audio;

    [Header("Patrol")]
    private float walkSpeed;
    private float distanceThreshold = 2f;

    private Transform nextPosition;
    private int currentPosition;

    public Transform waypoints;
    private List<Transform> positions;

    [Header("Chase")]
    public Transform target;

    private Animator animator;
    private NavMeshAgent agent;

    [Header("Hit")]
    private bool isHitting;

    // Start is called before the first frame update
    void Start()
    {
        audio = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        walkSpeed = agent.speed;

        positions = new List<Transform>();

        foreach (Transform child in waypoints)
        {
            //Debug.Log(child.name);
            positions.Add(child);
        }

        currentPosition = 0;
        nextPosition = positions[currentPosition];

        PatrolInit();
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case State.Patrol:
                PatrolProcess();
                break;
            case State.Chase:
                ChaseProcess();
                break;
            case State.Hit:
                break;
            default:
                Debug.LogError("Error State!!");
                break;
        }

        ExitCondition();
    }

    private void PatrolInit()
    {
        //Debug.Log("PatrolInit");
        animator.SetBool("Running", false);
        agent.isStopped = false;
        agent.speed = walkSpeed;
        agent.SetDestination(nextPosition.position);
        currentState = State.Patrol;
    }

    private void PatrolProcess()
    {
        CheckPatrolNextPosition();
    }

    private void ChaseInit()
    {
        //Debug.Log("ChaseInit");
        audio.PlayOneShot(shoutsSound[Random.Range(0, shoutsSound.Length)]);
        animator.SetBool("Running", true);
        agent.isStopped = false;
        agent.speed = walkSpeed * 2;
        currentState = State.Chase;
    }

    private void ChaseProcess()
    {
        if (target != null)
            agent.SetDestination(target.position);
    }

    public void HitInit()
    {
        animator.SetBool("Running", false);
        animator.SetBool("Attacking", true);
        agent.isStopped = true;
        Invoke("HitSound", 1f);
        Invoke("HitProcess", 2f);
        currentState = State.Hit;
    }

    private void HitSound()
    {
        audio.PlayOneShot(attackSound[Random.Range(0, attackSound.Length)]);
    }
    private void HitProcess()
    {
        isHitting = false;
        animator.SetBool("Attacking", false);
    }

    private void ExitCondition()
    {
        switch (currentState)
        {
            case State.Patrol:
                if (target != null)
                    ChaseInit();
                break;
            case State.Chase:
                if (target == null)
                    PatrolInit();
                else if (isHitting)
                    HitInit();
                break;
            case State.Hit:
                if (!isHitting)
                {
                    if (target != null)
                        ChaseInit();
                    else if (target == null)
                        PatrolInit();
                }
                break;
            default:
                Debug.LogError("Está malito D':");
                break;
        }
    }

    private void CheckPatrolNextPosition()
    {
        if (Vector3.Distance(transform.position, nextPosition.position) < distanceThreshold)
        {
            UpdateCurrentPosition();
        }
    }

    private void UpdateCurrentPosition()
    {
        currentPosition = GetNextPosition();
        nextPosition = positions[currentPosition];
        agent.destination = nextPosition.position;
    }

    private int GetNextPosition()
    {
        int nextPosition = currentPosition + 1;

        if (nextPosition >= positions.Count)
            nextPosition = 0;

        return nextPosition;
    }

    public void TargetDetection(Transform target)
    {
        //Debug.Log("TargetDetection: " + target);
        this.target = target;
    }

    public void Hit()
    {
        isHitting = true;
    }

}