using UnityEngine;
using UnityEngine.Events;

namespace Game.Core.Events
{
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
