using UnityEngine;
using Pathfinding;

// Direction int values (must match Guard.controller):
// 0=IdleDown  1=WalkRight  2=WalkUp  3=WalkLeft  4=WalkDown
// 5=IdleRight 6=IdleUp     7=IdleLeft
[RequireComponent(typeof(Animator))]
public class GuardAnimator : MonoBehaviour
{
    static readonly int DirectionParam = Animator.StringToHash("Direction");

    Animator animator;
    IAstarAI ai;
    EnemyAI enemyAI;
    int lastWalkDir = 4; // default: was walking down

    void Awake()
    {
        animator = GetComponent<Animator>();
        ai = GetComponent<IAstarAI>();
        enemyAI = GetComponent<EnemyAI>();
    }

    void Update()
    {
        if (ai == null) return;

        Vector2 vel = (Vector2)ai.velocity;

        if (vel.sqrMagnitude < 0.01f)
        {
            // Stopped — pick idle direction from EnemyAI facing if available,
            // otherwise fall back to last walk direction
            int idleDir = GetIdleDir();
            animator.SetInteger(DirectionParam, idleDir);
            return;
        }

        int walkDir;
        if (Mathf.Abs(vel.x) >= Mathf.Abs(vel.y))
            walkDir = vel.x > 0 ? 1 : 3; // WalkRight : WalkLeft
        else
            walkDir = vel.y > 0 ? 2 : 4; // WalkUp : WalkDown

        lastWalkDir = walkDir;
        animator.SetInteger(DirectionParam, walkDir);
    }

    int GetIdleDir()
    {
        // Use EnemyAI's authoritative facing direction when available
        Vector3 facing = enemyAI != null ? enemyAI.FacingDirection : Vector3.zero;

        if (facing.sqrMagnitude > 0.0001f)
        {
            if (Mathf.Abs(facing.x) >= Mathf.Abs(facing.y))
                return facing.x > 0 ? 5 : 7; // IdleRight : IdleLeft
            else
                return facing.y > 0 ? 6 : 0; // IdleUp : IdleDown
        }

        // Fallback to last walk direction
        return lastWalkDir switch
        {
            1 => 5, // was walking right → IdleRight
            2 => 6, // was walking up    → IdleUp
            3 => 7, // was walking left  → IdleLeft
            _ => 0, // was walking down  → IdleDown
        };
    }
}
