using System;
using Pathfinding;
using UnityEngine;

public class EnemyAI : MonoBehaviour {
    [SerializeField] private Transform pfFieldOfView;
    [SerializeField] private Patroller patroller;
    [SerializeField, Range(1f, 20f)] private float moveSpeed = 3f;
    [SerializeField, Range(1f, 20f)] private float targetRange = 10f;
    private FieldOfView fieldOfView;
    private AIPath aiPath;
    private Vector3 lastMoveDirection = Vector3.right;

    private enum State {
        Patrol,
        Suspicious,
        Alert,
        Search,
        KnockedOut
    }

    private State state;
    
    private void Start() {
        fieldOfView = Instantiate(pfFieldOfView, null).GetComponent<FieldOfView>();
        aiPath = GetComponent<AIPath>();
        if (aiPath != null) {
            aiPath.maxSpeed = moveSpeed;
        }
    }

    private void Update() {
        UpdateFacing();
        fieldOfView.SetOrigin(transform.position);
        fieldOfView.SetAimDirection(lastMoveDirection);
        
        switch (state) {
            case State.Patrol:
                PatrolUpdate();
                break;
            case State.Suspicious:
                break;
            case State.Alert:
                break;
            case State.Search:
                break;
            case State.KnockedOut:
                break;
        }
    }

    private void PatrolUpdate() {
        if (patroller == null || !patroller.IsReady() || aiPath == null) return;

        aiPath.destination = patroller.GetTarget();

        if (aiPath.pathPending) return;

        bool pathDone = aiPath.hasPath && aiPath.reachedEndOfPath;
        if (patroller.HasReached(transform.position) || pathDone) {
            patroller.Advance();
            aiPath.destination = patroller.GetTarget();
            aiPath.SearchPath();
        }
    }

    private void UpdateFacing() {
        if (aiPath != null && aiPath.velocity.sqrMagnitude > 0.01f) {
            lastMoveDirection = ((Vector3)aiPath.velocity).normalized;
        }
    }

    private void ChangeState(State newState)
    {
        //Todo
    }

    private void FindTarget() {
        if (Vector3.Distance(transform.position, Player.Instance.GetPosition) < targetRange) {
            //Player is in range
        }
    }

    private void OnDisable() {
        if (fieldOfView != null) fieldOfView.gameObject.SetActive(false);
    }

    private void OnEnable() {
        if (fieldOfView != null) fieldOfView.gameObject.SetActive(true);
    }
}
