using UnityEngine;

namespace Game.Core.Scenes
{
    /// <summary>
    /// Categorises a scene so the loader can decide how to handle it
    /// (e.g. menus replace each other; gameplay scenes might keep a HUD overlay).
    /// </summary>
    public enum SceneType
    {
        Menu,
        Gameplay,
        PersistentManagers
    }

    /// <summary>
    /// ScriptableObject reference to a scene. Avoids hard-coded scene names / build indices
    /// scattered through the codebase. Buttons, gameplay code, and the SceneLoader all
    /// reference these assets instead.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Scenes/Game Scene", fileName = "Scene_")]
    public class GameSceneSO : ScriptableObject
    {
        [Tooltip("Scene name as it appears in Build Settings.")]
        public string sceneName;

        [Tooltip("Pretty name for UI (e.g. 'Level 1', 'Tutorial').")]
        public string displayName;

        public SceneType sceneType = SceneType.Gameplay;
    }
}
