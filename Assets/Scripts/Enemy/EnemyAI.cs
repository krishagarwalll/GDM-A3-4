using System;
using UnityEngine;

public class EnemyAI : MonoBehaviour {
    [SerializeField] private Transform pfFieldOfView;
    private FieldOfView fieldOfView;
    
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
    }

    private void Update() { //Todo
        fieldOfView.SetOrigin(transform.position);
        //fieldOfView.SetAimDirection(fieldOfView.GetAimDir());
        
        switch (state) {
            case State.Patrol:
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

    private void ChangeState(State newState)
    {
        //Todo
    }

    private void FindTarget() {
        float targetRange = 10f;
        if (Vector3.Distance(transform.position, Player.Instance.GetPosition) < targetRange) {
            //Player is in range
        }
    }
}
