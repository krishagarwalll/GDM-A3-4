using System;
using UnityEngine;

public class EnemyAI : MonoBehaviour {
    private enum State {
        Patrol,
        Suspicious,
        Alert,
        Search,
        KnockedOut
    }

    private State state;
    
    private void Awake()
    {
        
    }

    private void Update() { //Todo
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
}
