using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class GridMovement2D : MonoBehaviour
{
    private const float ReachedTargetEpsilonSqr = 0.000001f;
    private const float CollisionInset = 0.05f;

    [Header("Movement")]
    [SerializeField, Min(0.01f)] private float moveSpeed = 4f;
    [SerializeField] private bool snapToGridOnEnable = true;
    [SerializeField] private LayerMask blockingLayers = ~0;

    [Header("Input Buffer")]
    [SerializeField] private bool useMoveBuffer = true;

    [Header("Move Negation")]
    [SerializeField] private bool allowMoveNegation = true;

    private readonly Collider2D[] overlapHits = new Collider2D[8];

    private Coroutine moveRoutine;
    private Vector2 desiredDirection;
    private Vector2 currentDirection;
    private Vector2 queuedDirection;
    private Vector3Int currentCell;
    private BoxCollider2D bodyCollider;
    private TilemapGrid2D grid;

    private BoxCollider2D BodyCollider => bodyCollider ? bodyCollider : bodyCollider = GetComponent<BoxCollider2D>();
    private TilemapGrid2D Grid => grid ? grid : grid = FindNearestTilemapGrid(this);

    public bool IsMoving => moveRoutine != null;
    public Vector2 CurrentDirection => currentDirection;
    public BoxCollider2D ForecastCollider => BodyCollider;
    public LayerMask BlockingLayers
    {
        get => blockingLayers;
        set => blockingLayers = value;
    }

    public event Action<Vector2> StepBlocked;

    private void Awake()
    {
        if (!Grid)
            Debug.LogWarning("GridMovement2D could not find TilemapGrid2D in scene.", this);
    }

    private void OnEnable()
    {
        RegisterCurrentCell(snapToGridOnEnable);
    }

    private void OnDisable()
    {
        Stop(false);
        Grid?.Unregister(this);
    }

    private void Update()
    {
        if (IsMoving || desiredDirection == Vector2.zero) return;
        if (TryBeginStep(desiredDirection)) return;
        StepBlocked?.Invoke(desiredDirection);
    }

    public void SetDesiredDirection(Vector2 direction)
    {
        desiredDirection = NormalizeCardinal(direction);

        if (!IsMoving) return;
        if (!useMoveBuffer) return;
        if (desiredDirection == Vector2.zero) return;

        queuedDirection = desiredDirection;
    }

    public bool RequestStep(Vector2 direction)
    {
        var normalizedDirection = NormalizeCardinal(direction);
        if (normalizedDirection == Vector2.zero)
            return false;

        if (IsMoving)
        {
            if (useMoveBuffer || (allowMoveNegation && normalizedDirection == -currentDirection))
            {
                queuedDirection = normalizedDirection;
                return true;
            }

            return false;
        }

        if (TryBeginStep(normalizedDirection))
            return true;

        StepBlocked?.Invoke(normalizedDirection);
        return false;
    }

    public void ClearDesiredDirection()
    {
        desiredDirection = Vector2.zero;
        queuedDirection = Vector2.zero;
    }

    public void Stop(bool registerAfterStop = true)
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }

        currentDirection = Vector2.zero;
        Grid?.ReleaseReservation(this);
        queuedDirection = Vector2.zero;

        if (registerAfterStop)
            RegisterCurrentCell(false);
    }

    public void SnapToGridImmediate()
    {
        if (!TrySyncCurrentCell(false)) return;
        if (Grid.TryCellToWorldCenter(currentCell, transform.position.z, out var snappedPosition))
            transform.position = snappedPosition;
    }

    public bool ConflictsWith(GridMovement2D other)
    {
        if (!other || other == this) return false;

        var thisLayer = gameObject.layer;
        var otherLayer = other.gameObject.layer;

        if (!ContainsLayer(blockingLayers, otherLayer)) return false;
        if (!ContainsLayer(other.blockingLayers, thisLayer)) return false;
        if (Physics2D.GetIgnoreLayerCollision(thisLayer, otherLayer)) return false;

        var otherCollider = other.BodyCollider;
        if (!otherCollider) return true;
        if (Physics2D.GetIgnoreCollision(BodyCollider, otherCollider)) return false;

        return true;
    }

    public static Vector2 ToCardinal(Vector2 input, float deadzone = 0.25f, Vector2 preferredAxisDirection = default)
    {
        var threshold = Mathf.Max(0f, deadzone);
        if (input.sqrMagnitude < threshold * threshold) return Vector2.zero;

        var absX = Mathf.Abs(input.x);
        var absY = Mathf.Abs(input.y);

        if (absX > absY)
            return new Vector2(Mathf.Sign(input.x), 0f);

        if (absY > absX)
            return new Vector2(0f, Mathf.Sign(input.y));

        if (preferredAxisDirection != Vector2.zero)
            return Mathf.Abs(preferredAxisDirection.x) > 0f
                ? new Vector2(Mathf.Sign(input.x), 0f)
                : new Vector2(0f, Mathf.Sign(input.y));

        return new Vector2(Mathf.Sign(input.x), 0f);
    }

    private bool TryBeginStep(Vector2 direction)
    {
        if (!TrySyncCurrentCell(false)) return false;

        var targetCell = currentCell + ToCardinalCellOffset(direction, direction);

        if (!Grid.TryReserve(this, targetCell, blockingLayers))
            return false;

        if (!Grid.TryCellToWorldCenter(targetCell, transform.position.z, out var targetWorld))
        {
            Grid.ReleaseReservation(this);
            return false;
        }

        if (!CanEnterTarget(targetWorld))
        {
            Grid.ReleaseReservation(this);
            return false;
        }

        var startWorld = transform.position;
        if (Grid.TryCellToWorldCenter(currentCell, transform.position.z, out var startWorldCenter))
        {
            startWorld = startWorldCenter;
            transform.position = startWorldCenter;
        }

        moveRoutine = StartCoroutine(MoveStepRoutine(startWorld, targetWorld, targetCell, direction));
        return true;
    }

    private IEnumerator MoveStepRoutine(Vector3 startWorld, Vector3 targetWorld, Vector3Int targetCell, Vector2 direction)
    {
        currentDirection = direction;
        var activeDirection = direction;
        var activeTargetWorld = targetWorld;
        var isReversing = false;

        while (((Vector2)(activeTargetWorld - transform.position)).sqrMagnitude > ReachedTargetEpsilonSqr)
        {
            if (!isReversing && ShouldNegateCurrentStep(activeDirection))
            {
                isReversing = true;
                Grid?.ReleaseReservation(this);
                activeTargetWorld = startWorld;
                activeDirection = -direction;
                currentDirection = activeDirection;
            }

            transform.position = Vector3.MoveTowards(transform.position, activeTargetWorld, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = activeTargetWorld;
        currentDirection = Vector2.zero;
        moveRoutine = null;

        if (isReversing)
        {
            RegisterCurrentCell(false);
        }
        else if (Grid && Grid.CommitMove(this, targetCell, blockingLayers))
        {
            currentCell = targetCell;
        }
        else
        {
            Grid?.ReleaseReservation(this);
            RegisterCurrentCell(false);
        }

        if (TryGetNextDirection(out var nextDirection))
            TryBeginStep(nextDirection);
    }

    private void RegisterCurrentCell(bool snapToGrid)
    {
        if (!TrySyncCurrentCell(snapToGrid)) return;
        Grid.TryOccupy(this, currentCell, blockingLayers);
    }

    private bool TryGetNextDirection(out Vector2 nextDirection)
    {
        if (useMoveBuffer && queuedDirection != Vector2.zero)
        {
            nextDirection = queuedDirection;
            queuedDirection = Vector2.zero;
            return true;
        }

        if (desiredDirection != Vector2.zero)
        {
            nextDirection = desiredDirection;
            return true;
        }

        nextDirection = Vector2.zero;
        return false;
    }

    private bool ShouldNegateCurrentStep(Vector2 stepDirection)
    {
        if (!allowMoveNegation) return false;

        var intentDirection = queuedDirection != Vector2.zero ? queuedDirection : desiredDirection;
        if (intentDirection == Vector2.zero) return false;

        return intentDirection == -stepDirection;
    }

    private bool TrySyncCurrentCell(bool snapToGrid)
    {
        if (!Grid)
            return false;

        if (!Grid.TryWorldToCell(transform.position, out var cell))
            return false;

        currentCell = cell;

        if (!snapToGrid) return true;

        if (!Grid.TryCellToWorldCenter(currentCell, transform.position.z, out var worldCenter))
            return false;

        transform.position = worldCenter;
        return true;
    }

    private bool CanEnterTarget(Vector3 targetWorld)
    {
        var filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = blockingLayers,
            useTriggers = false
        };

        var hitCount = Physics2D.OverlapBox(targetWorld, ResolveProbeSize(), 0f, filter, overlapHits);
        for (var i = 0; i < hitCount; i++)
        {
            if (ShouldBlockHit(overlapHits[i]))
                return false;
        }

        return true;
    }

    private bool ShouldBlockHit(Collider2D hit)
    {
        if (!hit) return false;
        if (hit.transform.root == transform.root) return false;

        var hitLayer = GetCollisionLayer(hit);
        if (!ContainsLayer(blockingLayers, hitLayer)) return false;
        if (Physics2D.GetIgnoreLayerCollision(gameObject.layer, hitLayer)) return false;
        if (Physics2D.GetIgnoreCollision(BodyCollider, hit)) return false;

        var otherMover = hit.GetComponentInParent<GridMovement2D>();
        if (!otherMover && hit.attachedRigidbody)
            otherMover = hit.attachedRigidbody.GetComponentInParent<GridMovement2D>();

        if (otherMover && otherMover != this)
            return ConflictsWith(otherMover);

        return true;
    }

    private Vector2 ResolveProbeSize()
    {
        var bounds = BodyCollider.bounds.size;
        var width = Mathf.Max(0.01f, bounds.x - CollisionInset * 2f);
        var height = Mathf.Max(0.01f, bounds.y - CollisionInset * 2f);
        return new Vector2(width, height);
    }

    private static bool ContainsLayer(LayerMask mask, int layer)
    {
        if (layer < 0)
            return false;

        return (mask.value & (1 << layer)) != 0;
    }

    private static int GetCollisionLayer(Collider2D collider)
    {
        if (!collider)
            return -1;

        if (collider.attachedRigidbody)
            return collider.attachedRigidbody.gameObject.layer;

        return collider.gameObject.layer;
    }

    private static Vector3Int ToCardinalCellOffset(Vector2 direction, Vector2 preferredAxisDirection = default)
    {
        var preferred = preferredAxisDirection != Vector2.zero ? preferredAxisDirection : direction;
        var cardinal = ToCardinal(direction, 0.0001f, preferred);

        if (cardinal == Vector2.zero)
            return Vector3Int.zero;

        return new Vector3Int(Mathf.RoundToInt(cardinal.x), Mathf.RoundToInt(cardinal.y), 0);
    }

    private static TilemapGrid2D FindNearestTilemapGrid(Component source)
    {
        if (!source)
            return null;

        var local = source.GetComponent<TilemapGrid2D>();
        if (local)
            return local;

        var parent = source.GetComponentInParent<TilemapGrid2D>();
        if (parent)
            return parent;

        var grids = UnityEngine.Object.FindObjectsByType<TilemapGrid2D>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (grids == null || grids.Length == 0)
            return null;

        TilemapGrid2D nearest = null;
        var nearestDistanceSqr = float.MaxValue;
        var position = source.transform.position;

        for (var i = 0; i < grids.Length; i++)
        {
            var candidate = grids[i];
            if (!candidate)
                continue;

            var distanceSqr = (candidate.transform.position - position).sqrMagnitude;
            if (distanceSqr >= nearestDistanceSqr)
                continue;

            nearestDistanceSqr = distanceSqr;
            nearest = candidate;
        }

        return nearest;
    }

    private static Vector2 NormalizeCardinal(Vector2 direction)
    {
        if (direction == Vector2.zero)
            return Vector2.zero;

        var absX = Mathf.Abs(direction.x);
        var absY = Mathf.Abs(direction.y);

        if (absX >= absY)
            return new Vector2(Mathf.Sign(direction.x), 0f);

        return new Vector2(0f, Mathf.Sign(direction.y));
    }
}
