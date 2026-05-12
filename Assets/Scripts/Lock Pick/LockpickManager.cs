using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class LockpickManager : MonoBehaviour
{
    public static LockpickManager Instance { get; private set;  }

    [Header("UI References")] 
    [SerializeField] private GameObject lockpickPanel;
    [SerializeField] private RectTransform innerLock;
    [SerializeField] private RectTransform pick;
    
    [Header("Settings")]
    [SerializeField, Range(10f, 90f)] private float lockRange = 15f;
    [SerializeField, Range(1f, 20f)] private float lockSpeed = 10f;
    [SerializeField] private float maxAngle = 90f;

    private float unlockAngle;
    private Vector2 unlockRange;
    private float eulerAngle;
    private float keyPressTime;
    private bool isActive;
    private System.Action onSuccess;

    private void Awake()
    {
        Instance = this;
        lockpickPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void OpenLockpick(System.Action successCallback)
    {
        onSuccess = successCallback;
        isActive = true;
        lockpickPanel.SetActive(true);
        Player.Instance.SetMovementLocked(true);
        GenerateNewLock();
    }

    private void CloseLockpick()
    {
        isActive = false;
        lockpickPanel.SetActive(false);
        Player.Instance.SetMovementLocked(false);
    }

    private void GenerateNewLock()
    {
        unlockAngle = Random.Range(-maxAngle + lockRange, maxAngle - lockRange);
        unlockRange = new Vector2(unlockAngle - lockRange, unlockAngle + lockRange);
        keyPressTime = 0f;
        innerLock.eulerAngles = Vector3.zero;
    }

    private void Update()
    {
        if (!isActive) return;
        
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 screenCentre = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 dir = mousePos - screenCentre;
        eulerAngle = Vector3.SignedAngle(Vector3.up, new Vector3(dir.x, dir.y, 0), Vector3.forward);
        eulerAngle = Mathf.Clamp(eulerAngle, -maxAngle, maxAngle);
        pick.rotation = Quaternion.AngleAxis(eulerAngle, Vector3.forward);

        if (Keyboard.current.aKey.isPressed)
        {
            keyPressTime = 1f;
        }
        else
        {
            keyPressTime = 0f;
        }

        float percentage = Mathf.Round(100 - Mathf.Abs(((eulerAngle - unlockAngle) / 100) * 100));
        float lockRotation = ((percentage / 100) * maxAngle) * keyPressTime;
        float maxRotation = (percentage / 100) * maxAngle;
        float lockLerp = Mathf.LerpAngle(innerLock.eulerAngles.z, lockRotation, Time.deltaTime * lockSpeed);
        innerLock.eulerAngles = new Vector3(0, 0, lockLerp);

        if (lockLerp >= maxRotation - 1f && keyPressTime > 0f)
        {
            if (eulerAngle < unlockRange.y && eulerAngle > unlockRange.x)
            {
                CloseLockpick();
                onSuccess?.Invoke();
            }
            else
            {
                float randomRotation = Random.Range(-15f, 15f);
                pick.eulerAngles += new Vector3(0, 0,  randomRotation);
            }
        }
    }
}
