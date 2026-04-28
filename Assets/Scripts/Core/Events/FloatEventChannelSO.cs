using UnityEngine;
using UnityEngine.Events;

namespace Game.Core.Events
{
    [CreateAssetMenu(menuName = "Game/Events/Float Event Channel", fileName = "FloatEventChannel")]
    public class FloatEventChannelSO : ScriptableObject
    {
        public UnityAction<float> OnEventRaised;

        public void Raise(float value)
        {
            OnEventRaised?.Invoke(value);
        }
    }
}
