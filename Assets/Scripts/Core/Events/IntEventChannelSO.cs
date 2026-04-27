using UnityEngine;
using UnityEngine.Events;

namespace Game.Core.Events
{
    /// <summary>
    /// ScriptableObject event channel carrying an int payload.
    /// Useful for score, lives, build-index requests, etc.
    /// </summary>
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
