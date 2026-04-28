using UnityEngine;

namespace Game.Core.Scenes
{
    public enum SceneType
    {
        Menu,
        Gameplay,
        PersistentManagers
    }

    [CreateAssetMenu(menuName = "Game/Scenes/Game Scene", fileName = "Scene_")]
    public class GameSceneSO : ScriptableObject
    {
        public string sceneName;
        public string displayName;
        public SceneType sceneType = SceneType.Gameplay;
    }
}
