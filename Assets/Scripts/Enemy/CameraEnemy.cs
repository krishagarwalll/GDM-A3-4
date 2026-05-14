using Pathfinding;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core.Pause;

public class CameraEnemy : MonoBehaviour
{

    [Header("Type")]
    [SerializeField] private GuardType guardType = GuardType.Patrolling;

    [Header("References")]
    [SerializeField] private Transform pfFieldOfView;
    [SerializeField] private Patroller patroller;

    [Header("Movement")]
    [SerializeField, Range(1f, 20f)] private float patrolSpeed = 3f;
    [SerializeField, Range(1f, 20f)] private float chaseSpeed = 4.5f;
    [Tooltip("Static guards will not chase further than this radius from their post.")]
    [SerializeField, Range(0f, 30f)] private float chaseLeashRadius = 5f;

    [Header("Detection Timings")]
    [Tooltip("Seconds the guard hesitates after first spotting the player before going Alert.")]
    [SerializeField, Range(0f, 5f)] private float suspicionDelay = 1.5f;
    [Tooltip("Seconds the guard searches the last known position before giving up.")]
    [SerializeField, Range(0.5f, 10f)] private float searchDuration = 4f;
    [Tooltip("Seconds the guard keeps chasing after losing line of sight.")]
    [SerializeField, Range(0.5f, 10f)] private float lostSightDelay = 1.5f;

    [Header("Crouch Detection")]
    [SerializeField] private bool reduceVisionForCrouchedPlayer = true;
    [SerializeField, Range(0.1f, 1f)] private float crouchedDetectionMultiplier = 0.6f;

    [Header("Hearing")]
    [Tooltip("Guard is alerted by an audible non-crouched moving player within this radius.")]
    [SerializeField, Range(0f, 15f)] private float hearingRadius = 7f;
    [Tooltip("Seconds of continuous noise before the guard starts reacting.")]
    [SerializeField, Range(0f, 5f)] private float hearingThreshold = 0.5f;
    [Tooltip("How fast the guard rotates toward heard noise (degrees per second).")]
    [SerializeField, Range(30f, 720f)] private float hearingTurnSpeed = 180f;
    [Header("Facing")]
    [Tooltip("Initial facing for Static guards (and the centre of their FOV sweep). Ignored for Patrolling.")]
    [SerializeField] private GuardFacing initialFacing = GuardFacing.Right;

    private FieldOfView fieldOfView;
    private VisionSweep visionSweep;
    private AIPath aiPath;
    private Vector3 lastMoveDirection = Vector3.right;
    private Vector3 currentAimDirection;
    private Vector3 homePosition;
    private Vector3 lastKnownPlayerPos;
    private State state;
    private float stateTimer;
    private float noiseLevel;
    private Vector3 heardNoisePos;
    private bool IsHearingActive => noiseLevel >= hearingThreshold;
    //--level5 darkmode
    private bool darkMode = false;
    private float EffectivePatrolSpeed => darkMode ? patrolSpeed * 0.6f : patrolSpeed;
    private float EffectiveChaseSpeed => darkMode ? chaseSpeed * 0.8f : chaseSpeed;
    //--level5 darkmode

    private enum State
    {
        Patrol,
        Suspicious,
        Alert,
        Search,
        KnockedOut
    }

    public Vector3 FacingDirection => currentAimDirection.sqrMagnitude > 0.0001f
        ? currentAimDirection
        : lastMoveDirection;

    private static Vector3 FacingToVector(GuardFacing f)
    {
        switch (f)
        {
            case GuardFacing.Up:    return Vector3.up;
            case GuardFacing.Left:  return Vector3.left;
            case GuardFacing.Down:  return Vector3.down;
            default:                return Vector3.right;
        }
    }

    private void Start()
    {
        homePosition = transform.position;

        if (guardType == GuardType.Static)
        {
            lastMoveDirection = FacingToVector(initialFacing);
        }
        currentAimDirection = lastMoveDirection;

        var fovInstance = Instantiate(pfFieldOfView, null);
        fieldOfView = fovInstance.GetComponent<FieldOfView>();
        visionSweep = fovInstance.GetComponent<VisionSweep>();

        aiPath = GetComponent<AIPath>();
        if (aiPath != null) aiPath.maxSpeed = patrolSpeed;

        EnterState(State.Patrol);
    }

    private void Update()
    {
        if (PauseService.IsPaused) return;
        if (state == State.KnockedOut) return;

        UpdateFacing();
        UpdateHearing();
        if (fieldOfView != null) fieldOfView.SetOrigin(transform.position);

        switch (state)
        {
            case State.Patrol: PatrolUpdate(); break;
            case State.Suspicious: SuspiciousUpdate(); break;
            case State.Alert: AlertUpdate(); break;
            case State.Search: SearchUpdate(); break;
        }

        bool sweepDriving = visionSweep != null && visionSweep.enabled;
        if (sweepDriving)
        {
            currentAimDirection = visionSweep.GetCurrentSweepDirection();
        }
        else
        {
            if (fieldOfView != null) fieldOfView.SetAimDirection(lastMoveDirection);
            currentAimDirection = lastMoveDirection;
        }
    }

    // --- States ---

    private void PatrolUpdate()
    {
        if (IsHearingActive)
        {
            if (aiPath != null) aiPath.canMove = false;
            if (visionSweep != null) visionSweep.enabled = false;
            TurnTowards(heardNoisePos);
        }
        else if (guardType == GuardType.Static)
        {
            // Static guard: stay put; lastMoveDirection follows the slow sweep
            // direction so the FOV cone, sprite and (disabled) movement all face
            // the same way.
            if (aiPath != null) aiPath.canMove = false;
            if (visionSweep != null && !visionSweep.enabled) visionSweep.enabled = true;
            TurnTowardsDir(GetNaturalPatrolDirection());
        }
        else
        {
            // Patrolling guard: just walk the route. AIPath handles steering;
            // alignment gating here would deadlock against AIPath's velocity.
            if (aiPath != null && !aiPath.canMove) aiPath.canMove = true;
            DriveAlongPatroller();
        }

        if (CanSeePlayer())
        {
            lastKnownPlayerPos = Player.Instance.GetPosition;
            ChangeState(State.Suspicious);
        }
    }

    private Vector3 GetNaturalPatrolDirection()
    {
        if (guardType == GuardType.Static && visionSweep != null)
        {
            return visionSweep.GetCurrentSweepDirection();
        }
        if (patroller != null && patroller.IsReady())
        {
            Vector3 to = patroller.GetTarget() - transform.position;
            if (to.sqrMagnitude > 0.0001f) return to.normalized;
        }
        return lastMoveDirection;
    }

    private void SuspiciousUpdate()
    {
        AimAt(lastKnownPlayerPos);
        if (CanSeePlayer()) lastKnownPlayerPos = Player.Instance.GetPosition;
        else if (IsHearingActive) lastKnownPlayerPos = heardNoisePos;

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            ChangeState(CanSeePlayer() ? State.Alert : State.Search);
        }
    }

    private void AlertUpdate()
    {
        bool sees = CanSeePlayer();
        if (sees)
        {
            lastKnownPlayerPos = Player.Instance.GetPosition;
            stateTimer = lostSightDelay;
        }
        else if (IsHearingActive)
        {
            lastKnownPlayerPos = heardNoisePos;
            stateTimer = lostSightDelay;
        }
        else
        {
            stateTimer -= Time.deltaTime;
        }

        if (aiPath != null)
        {
            aiPath.destination = ClampToLeash(lastKnownPlayerPos);
        }

        if (!sees && stateTimer <= 0f)
        {
            ChangeState(State.Search);
        }
    }

    private void SearchUpdate()
    {
        if (CanSeePlayer())
        {
            lastKnownPlayerPos = Player.Instance.GetPosition;
            ChangeState(State.Alert);
            return;
        }

        if (IsHearingActive) lastKnownPlayerPos = heardNoisePos;

        if (aiPath != null)
        {
            aiPath.destination = ClampToLeash(lastKnownPlayerPos);
        }

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            ChangeState(State.Patrol);
        }
    }

    // --- Helpers ---

    private void DriveAlongPatroller()
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

    private bool CanSeePlayer()
    {
        if (fieldOfView == null || Player.Instance == null) return false;
        float mult = (reduceVisionForCrouchedPlayer && Player.Instance.IsCrouching)
            ? crouchedDetectionMultiplier : 1f;
        return fieldOfView.IsTargetVisible(Player.Instance.GetPosition, mult);
    }

    private void UpdateHearing()
    {
        
    }

    private void TurnTowards(Vector3 worldPoint)
    {
        TurnTowardsDir(worldPoint - transform.position);
    }

    private void TurnTowardsDir(Vector3 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();
        float maxRad = hearingTurnSpeed * Mathf.Deg2Rad * Time.deltaTime;
        lastMoveDirection = Vector3.RotateTowards(lastMoveDirection, dir, maxRad, 0f);
    }

    private Vector3 ClampToLeash(Vector3 target)
    {
        if (guardType != GuardType.Static) return target;
        Vector3 fromHome = target - homePosition;
        if (fromHome.sqrMagnitude <= chaseLeashRadius * chaseLeashRadius) return target;
        return homePosition + fromHome.normalized * chaseLeashRadius;
    }

    private void AimAt(Vector3 worldPoint)
    {
        Vector3 dir = worldPoint - transform.position;
        if (dir.sqrMagnitude > 0.0001f) lastMoveDirection = dir.normalized;
    }

    private void UpdateFacing()
    {
        if (aiPath != null && aiPath.velocity.sqrMagnitude > 0.01f)
        {
            lastMoveDirection = ((Vector3)aiPath.velocity).normalized;
        }
    }

    // --- Transitions ---

    private void ChangeState(State newState)
    {
        if (state == newState) return;
        state = newState;
        EnterState(newState);
    }

    private void EnterState(State s)
    {
        switch (s)
        {
            case State.Patrol:
                if (visionSweep != null)
                {
                    visionSweep.enabled = (guardType == GuardType.Static);
                    if (guardType == GuardType.Static)
                        visionSweep.SetBaseDirection(lastMoveDirection);
                }
                if (aiPath != null)
                {
                    aiPath.canMove = true;
                    aiPath.maxSpeed = patrolSpeed;
                }
                stateTimer = 0f;
                break;

            case State.Suspicious:
                if (visionSweep != null) visionSweep.enabled = false;
                if (aiPath != null) aiPath.canMove = false;
                stateTimer = suspicionDelay;
                break;

            case State.Alert:
                if (visionSweep != null) visionSweep.enabled = false;
                if (aiPath != null)
                {
                    aiPath.canMove = true;
                    aiPath.maxSpeed = chaseSpeed;
                }
                stateTimer = lostSightDelay;
                break;

            case State.Search:
                if (visionSweep != null) visionSweep.enabled = false;
                if (aiPath != null)
                {
                    aiPath.canMove = true;
                    aiPath.maxSpeed = patrolSpeed;
                }
                stateTimer = searchDuration;
                break;

            case State.KnockedOut:
                if (visionSweep != null) visionSweep.enabled = false;
                if (aiPath != null)
                {
                    aiPath.canMove = false;
                    aiPath.destination = transform.position;
                }
                if (fieldOfView != null) fieldOfView.gameObject.SetActive(false);
                break;
        }
    }

    public void KnockOut()
    {
        if (state == State.KnockedOut) return;
        ChangeState(State.KnockedOut);
    }

    public bool IsKnockedOut => state == State.KnockedOut;

    private static readonly string[] DistractionScenes = { "level1Scene", "level2Scene" };

    private static bool IsDistractionScene()
    {
        string active = SceneManager.GetActiveScene().name;
        for (int i = 0; i < DistractionScenes.Length; i++)
        {
            if (DistractionScenes[i] == active) return true;
        }
        return false;
    }

    public void OnDistraction(Vector3 worldPos, DistractionIntensity intensity)
    {
        if (!IsDistractionScene()) return;
        if (state == State.KnockedOut || state == State.Alert) return;

        if (intensity == DistractionIntensity.Footstep)
        {
            heardNoisePos = worldPos;
            noiseLevel = hearingThreshold + 1f;
        }
        else
        {
            lastKnownPlayerPos = worldPos;
            ChangeState(State.Search);
        }
    }

    private void OnDisable()
    {
        if (fieldOfView != null) fieldOfView.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (fieldOfView != null && state != State.KnockedOut)
        {
            fieldOfView.gameObject.SetActive(true);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, hearingRadius);
    }

    //--Level 5 DarkMode
    public void SetDarkMode(bool isDark)
    {
        darkMode = isDark;
        ApplyCurrentSpeed();
    }

    private void ApplyCurrentSpeed()
    {
        if (aiPath == null) return;
        switch (state)
        {
            case State.Alert:
                aiPath.maxSpeed = EffectiveChaseSpeed;
                break;
            case State.Patrol:
            case State.Search:
            case State.Suspicious:
                aiPath.maxSpeed = EffectivePatrolSpeed;
                break;
        }
    }
    //--Level 5 DarkMode
}
