using System;
using Pathfinding;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private Transform pfFieldOfView;
    [SerializeField] private Patroller patroller;
    [SerializeField, Range(1f, 20f)] private float moveSpeed = 3f;
    [SerializeField, Range(1f, 20f)] private float targetRange = 10f;
    [SerializeField] private float suspiciousDuration = 2f; // 可疑状态持续时间
    private Transform playerTransform;
    private FieldOfView fieldOfView;
    private AIPath aiPath;
    private Vector3 lastMoveDirection = Vector3.right;
    private float suspiciousTimer = 0f;

    private enum State
    {
        Patrol,
        Suspicious,
        Alert,
        Search,
        KnockedOut
    }
    private State state;

    private void Start()
    {
        fieldOfView = Instantiate(pfFieldOfView, null).GetComponent<FieldOfView>();
        aiPath = GetComponent<AIPath>();
        if (aiPath != null)
            aiPath.maxSpeed = moveSpeed;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
    }

    private void Update()
    {
        UpdateFacing();
        fieldOfView.SetOrigin(transform.position);
        fieldOfView.SetAimDirection(lastMoveDirection);

        switch (state)
        {
            case State.Patrol:
                PatrolUpdate();
                CheckForPlayer();
                break;
            case State.Suspicious:
                SuspiciousUpdate();
                break;
            case State.Alert:
                AlertUpdate();
                break;
            case State.Search:
                break;
            case State.KnockedOut:
                break;
        }
    }

    private void CheckForPlayer()
    {
        bool visible = fieldOfView.IsPlayerVisible();
        Debug.Log($"[CheckForPlayer] IsPlayerVisible: {visible} | State: {state}");

        if (visible)
            ChangeState(State.Suspicious);

    }

    private void SuspiciousUpdate()
    {
        if (fieldOfView.IsPlayerVisible())
        {
            suspiciousTimer += Time.deltaTime;
            if (playerTransform != null)
                aiPath.destination = playerTransform.position;

            if (suspiciousTimer >= suspiciousDuration)
                ChangeState(State.Alert);
        }
        else
        {
            ChangeState(State.Patrol);
        }
    }

    private void AlertUpdate()
    {
        if (playerTransform != null)
            aiPath.destination = playerTransform.position;

        if (!fieldOfView.IsPlayerVisible())
            ChangeState(State.Search);
    }


    private void PatrolUpdate()
    {
        if (patroller == null || !patroller.IsReady() || aiPath == null) return;
        aiPath.destination = patroller.GetTarget();
        if (aiPath.pathPending) return;
        bool pathDone = aiPath.hasPath && aiPath.reachedEndOfPath;
        if (patroller.HasReached(transform.position) || pathDone)
        {
            patroller.Advance();
            aiPath.destination = patroller.GetTarget();
            aiPath.SearchPath();
        }
    }

    private void UpdateFacing()
    {
        if (aiPath != null && aiPath.velocity.sqrMagnitude > 0.01f)
            lastMoveDirection = ((Vector3)aiPath.velocity).normalized;
    }

    private void ChangeState(State newState)
    {
        state = newState;

        switch (newState)
        {
            case State.Patrol:
                suspiciousTimer = 0f;
                fieldOfView.ResetSpotted();
                aiPath.maxSpeed = moveSpeed;
                // 恢复巡逻目标
                if (patroller != null)
                    aiPath.destination = patroller.GetTarget();
                break;

            case State.Suspicious:
                suspiciousTimer = 0f;
                aiPath.maxSpeed = moveSpeed * 0.5f;
                break;

            case State.Alert:
                aiPath.maxSpeed = moveSpeed * 1.5f;
                break;

            case State.Search:
                // Todo
                break;
        }
    }

    private void FindTarget()
    {
        if (Vector3.Distance(transform.position, Player.Instance.GetPosition) < targetRange)
        {
            // Todo
        }
    }

    private void OnDisable()
    {
        if (fieldOfView != null) fieldOfView.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (fieldOfView != null) fieldOfView.gameObject.SetActive(true);
    }
}