using UnityEngine;
using UnityEngine.Events;

namespace Game.Core.Events
{
    [CreateAssetMenu(menuName = "Game/Events/Int Event Channel", fileName = "IntEventChannel")]
    public class IntEventChannelSO : ScriptableObject
    {
        public UnityAction<int> OnEventRaised;

        public void Raise(int value)
        {
            OnEventRaised?.Invoke(value);
        }
    }
}
