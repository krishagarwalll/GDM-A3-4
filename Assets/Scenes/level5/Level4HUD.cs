using TMPro;
using UnityEngine;

public class Level4HUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerLabel;
    [SerializeField] private TextMeshProUGUI treasureLabel;

    private void Awake()
    {
        if (timerLabel == null)
            timerLabel = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (Level4GameController.Instance == null) return;

        // Timer display
        if (timerLabel != null)
        {
            float time = Level4GameController.Instance.RemainingTime;
            int total = Mathf.CeilToInt(time);
            int minutes = total / 60;
            int seconds = total % 60;
            timerLabel.text = $"{minutes:0}:{seconds:00}";
            timerLabel.color = time <= 30f ? Color.red : Color.white;
        }

        // Treasure count display
        if (treasureLabel != null)
        {
            treasureLabel.text = $"Treasures: {Level4GameController.Instance.CurrentTreasures} / 3";
        }
    }
}
