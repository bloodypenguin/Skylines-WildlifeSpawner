using UnityEngine;

namespace WildlifeSpawner
{
    public class WildlifeSpawnerMonitor : MonoBehaviour
    {
        private const string GameObjectName = "WildlifeSpawnerMonitor";

        public static void Initialize()
        {
            var gameObject = GameObject.Find(GameObjectName);
            if (gameObject != null)
            {
                return;
            }
            gameObject = new GameObject(GameObjectName);
            gameObject.AddComponent<WildlifeSpawnerMonitor>();
        }

        public static void Dispose()
        {
            var gameObject = GameObject.Find(GameObjectName);
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }

        public void Update()
        {
            var tool = ToolsModifierControl.GetCurrentTool<BuildingTool>();
            if (tool?.m_prefab == null)
            {
                ModUI.Hide();
                return;
            }
            if (tool.m_prefab.name.Contains("Wild"))
            {
                ModUI.Show();
            }
        }
    }
}