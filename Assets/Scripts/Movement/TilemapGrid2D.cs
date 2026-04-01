using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
[RequireComponent(typeof(Grid))]
public class TilemapGrid2D : MonoBehaviour
{
    private sealed class CellBlocker
    {
        public readonly UnityEngine.Object Owner;
        public readonly Vector3Int Cell;
        public readonly int Layer;

        public CellBlocker(UnityEngine.Object owner, Vector3Int cell, int layer)
        {
            Owner = owner;
            Cell = cell;
            Layer = layer;
        }
    }

    private readonly Dictionary<Vector3Int, GridMovement2D> occupiedByCell = new();
    private readonly Dictionary<GridMovement2D, Vector3Int> occupiedCellByMover = new();
    private readonly Dictionary<Vector3Int, GridMovement2D> reservedByCell = new();
    private readonly Dictionary<GridMovement2D, Vector3Int> reservedCellByMover = new();
    private readonly Dictionary<Vector3Int, List<CellBlocker>> blockersByCell = new();
    private readonly Dictionary<UnityEngine.Object, CellBlocker> blockerByOwner = new();

    private Grid grid;
    private TilemapCollider2D[] childTilemapColliders;

    private Grid Grid => grid ? grid : grid = GetComponent<Grid>();
    private TilemapCollider2D[] ChildTilemapColliders =>
        childTilemapColliders ??= GetComponentsInChildren<TilemapCollider2D>(true);

    private void OnTransformChildrenChanged()
    {
        childTilemapColliders = null;
    }

    public bool TryWorldToCell(Vector3 worldPosition, out Vector3Int cell)
    {
        if (!Grid)
        {
            cell = default;
            return false;
        }

        cell = Grid.WorldToCell(worldPosition);
        return true;
    }

    public bool TryCellToWorldCenter(Vector3Int cell, float z, out Vector3 worldPosition)
    {
        if (!Grid)
        {
            worldPosition = default;
            return false;
        }

        worldPosition = Grid.GetCellCenterWorld(cell);
        worldPosition.z = z;
        return true;
    }

    public bool TryOccupy(GridMovement2D mover, Vector3Int cell, LayerMask blockingLayers)
        => TryClaimCell(mover, cell, blockingLayers, occupiedByCell, occupiedCellByMover);

    public bool TryReserve(GridMovement2D mover, Vector3Int cell, LayerMask blockingLayers)
        => TryClaimCell(mover, cell, blockingLayers, reservedByCell, reservedCellByMover);

    public bool CommitMove(GridMovement2D mover, Vector3Int targetCell, LayerMask blockingLayers)
    {
        if (!TryClaimCell(mover, targetCell, blockingLayers, occupiedByCell, occupiedCellByMover))
            return false;

        ReleaseReservation(mover);
        return true;
    }

    public void ReleaseReservation(GridMovement2D mover)
    {
        RemoveEntry(mover, reservedByCell, reservedCellByMover);
    }

    public void Unregister(GridMovement2D mover)
    {
        RemoveEntry(mover, occupiedByCell, occupiedCellByMover);
        RemoveEntry(mover, reservedByCell, reservedCellByMover);
    }

    public bool TryRegisterCellBlocker(UnityEngine.Object owner, Vector3Int cell, int layer)
    {
        if (!owner)
            return false;

        if (blockerByOwner.TryGetValue(owner, out var previousBlocker))
        {
            if (previousBlocker.Cell == cell && previousBlocker.Layer == layer)
                return true;

            UnregisterCellBlocker(owner);
        }

        var blocker = new CellBlocker(owner, cell, layer);
        blockerByOwner[owner] = blocker;

        if (!blockersByCell.TryGetValue(cell, out var cellBlockers))
        {
            cellBlockers = new List<CellBlocker>(2);
            blockersByCell[cell] = cellBlockers;
        }

        cellBlockers.Add(blocker);
        return true;
    }

    public void UnregisterCellBlocker(UnityEngine.Object owner)
    {
        if (!owner)
            return;

        if (!blockerByOwner.TryGetValue(owner, out var blocker))
            return;

        blockerByOwner.Remove(owner);

        if (!blockersByCell.TryGetValue(blocker.Cell, out var cellBlockers))
            return;

        for (var i = cellBlockers.Count - 1; i >= 0; i--)
        {
            var candidate = cellBlockers[i];
            if (candidate == blocker || !candidate.Owner)
                cellBlockers.RemoveAt(i);
        }

        if (cellBlockers.Count == 0)
            blockersByCell.Remove(blocker.Cell);
    }

    private bool IsBlockedByTilemap(GridMovement2D mover, Vector3Int cell, LayerMask blockingLayers)
    {
        var moverCollider = mover.ForecastCollider;
        var moverLayer = mover.gameObject.layer;
        var colliders = ChildTilemapColliders;

        for (var i = 0; i < colliders.Length; i++)
        {
            var tilemapCollider = colliders[i];
            if (!tilemapCollider || !tilemapCollider.enabled || tilemapCollider.isTrigger) continue;

            var tilemap = tilemapCollider.GetComponent<Tilemap>();
            if (!tilemap || !tilemap.enabled) continue;

            if (tilemap.GetColliderType(cell) == Tile.ColliderType.None) continue;

            var tilemapLayer = tilemap.gameObject.layer;
            if (!ContainsLayer(blockingLayers, tilemapLayer)) continue;
            if (Physics2D.GetIgnoreLayerCollision(moverLayer, tilemapLayer)) continue;
            if (moverCollider && Physics2D.GetIgnoreCollision(moverCollider, tilemapCollider)) continue;

            return true;
        }

        return false;
    }

    private bool TryClaimCell(
        GridMovement2D mover,
        Vector3Int cell,
        LayerMask blockingLayers,
        IDictionary<Vector3Int, GridMovement2D> byCell,
        IDictionary<GridMovement2D, Vector3Int> byMover)
    {
        if (!CanClaimCell(mover, cell, blockingLayers))
            return false;

        SetEntry(mover, cell, byCell, byMover);
        return true;
    }

    private bool CanClaimCell(GridMovement2D mover, Vector3Int cell, LayerMask blockingLayers)
    {
        if (!mover) return false;
        if (IsBlockedByTilemap(mover, cell, blockingLayers)) return false;
        if (IsBlockedByCellBlocker(mover, cell, blockingLayers)) return false;
        if (IsTakenByOtherMover(cell, mover, occupiedByCell)) return false;
        if (IsTakenByOtherMover(cell, mover, reservedByCell)) return false;
        return true;
    }

    private bool IsBlockedByCellBlocker(GridMovement2D mover, Vector3Int cell, LayerMask blockingLayers)
    {
        if (!blockersByCell.TryGetValue(cell, out var cellBlockers))
            return false;

        var moverLayer = mover.gameObject.layer;

        for (var i = cellBlockers.Count - 1; i >= 0; i--)
        {
            var blocker = cellBlockers[i];
            if (blocker == null || !blocker.Owner)
            {
                cellBlockers.RemoveAt(i);
                continue;
            }

            if (!ContainsLayer(blockingLayers, blocker.Layer))
                continue;

            if (Physics2D.GetIgnoreLayerCollision(moverLayer, blocker.Layer))
                continue;

            return true;
        }

        if (cellBlockers.Count == 0)
            blockersByCell.Remove(cell);

        return false;
    }

    private static bool IsTakenByOtherMover(
        Vector3Int cell,
        GridMovement2D mover,
        IReadOnlyDictionary<Vector3Int, GridMovement2D> lookup)
    {
        if (!lookup.TryGetValue(cell, out var other)) return false;
        if (!other || other == mover) return false;
        return mover.ConflictsWith(other);
    }

    private static void RemoveEntry(
        GridMovement2D mover,
        IDictionary<Vector3Int, GridMovement2D> byCell,
        IDictionary<GridMovement2D, Vector3Int> byMover)
    {
        if (!mover) return;
        if (!byMover.TryGetValue(mover, out var previousCell)) return;

        byMover.Remove(mover);

        if (byCell.TryGetValue(previousCell, out var previousMover) && previousMover == mover)
            byCell.Remove(previousCell);
    }

    private static void SetEntry(
        GridMovement2D mover,
        Vector3Int cell,
        IDictionary<Vector3Int, GridMovement2D> byCell,
        IDictionary<GridMovement2D, Vector3Int> byMover)
    {
        RemoveEntry(mover, byCell, byMover);
        byCell[cell] = mover;
        byMover[mover] = cell;
    }

    private static bool ContainsLayer(LayerMask mask, int layer)
    {
        if (layer < 0)
            return false;

        return (mask.value & (1 << layer)) != 0;
    }
}
