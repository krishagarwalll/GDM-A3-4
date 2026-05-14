using UnityEngine;
using UnityEngine.Events;
using Game.Core.Events;
using Game.Core.Game;

public class Level4GameController : MonoBehaviour
{
    public static Level4GameController Instance { get; private set; }

    [Header("Game Config")]
    [SerializeField] private int treasuresRequired = 3;
    [SerializeField] private float totalTime = 120f;

    [Header("Events")]
    [SerializeField] private VoidEventChannelSO onTreasureCollected;
    [SerializeField] private VoidEventChannelSO onAllTreasuresCollected;
    [SerializeField] private VoidEventChannelSO onTimeUp;
    [SerializeField] private UnityEvent<int, int> onTreasureCountChanged;
    [SerializeField] private UnityEvent<float> onTimeChanged;

    [Header("Exit")]
    [SerializeField] private Collider2D exitCollider;

    public int CurrentTreasures { get; private set; }
    public bool AllTreasuresCollected => CurrentTreasures >= treasuresRequired;
    public float RemainingTime { get; private set; }
    public bool IsTimeUp => RemainingTime <= 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        RemainingTime = totalTime;
        CurrentTreasures = 0;

        if (exitCollider != null)
            exitCollider.enabled = false;

        onTreasureCountChanged?.Invoke(CurrentTreasures, treasuresRequired);
    }

    private void Update()
    {
        if (IsTimeUp) return;

        RemainingTime -= Time.deltaTime;
        if (RemainingTime <= 0)
        {
            RemainingTime = 0;
            onTimeUp?.Raise();
        }

        onTimeChanged?.Invoke(RemainingTime);
    }

    public void CollectTreasure()
    {
        if (AllTreasuresCollected) return;

        CurrentTreasures++;
        onTreasureCollected?.Raise();
        onTreasureCountChanged?.Invoke(CurrentTreasures, treasuresRequired);

        if (AllTreasuresCollected)
        {
            onAllTreasuresCollected?.Raise();
            EnableExit();
        }
    }

    private void EnableExit()
    {
        if (exitCollider != null)
            exitCollider.enabled = true;
    }

    public bool CanExit()
    {
        return AllTreasuresCollected;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
