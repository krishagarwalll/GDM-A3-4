using UnityEngine;
using UnityEngine.Events;

namespace Game.Core.Events
{
    /// <summary>
    /// ScriptableObject event channel with no payload.
    /// Create asset via: Assets > Create > Game / Events > Void Event Channel.
    /// Pattern: subscribers add to OnEventRaised in OnEnable, remove in OnDisable.
    /// Raisers call Raise().
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Events/Void Event Channel", fileName = "VoidEventChannel")]
    public class VoidEventChannelSO : ScriptableObject
    {
        public UnityAction OnEventRaised;

        public void Raise()
        {
            OnEventRaised?.Invoke();
        }
    }
}
