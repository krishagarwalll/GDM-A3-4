using UnityEngine;
using UnityEngine.Events;
using Game.Core.Scenes;

namespace Game.Core.Events
{
    [CreateAssetMenu(menuName = "Game/Events/Game Scene Event Channel", fileName = "GameSceneEventChannel")]
    public class GameSceneEventChannelSO : ScriptableObject
    {
        public UnityAction<GameSceneSO> OnEventRaised;

        public void Raise(GameSceneSO scene)
        {
            OnEventRaised?.Invoke(scene);
        }
    }
}
