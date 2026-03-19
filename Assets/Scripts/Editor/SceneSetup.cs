using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using ARIA.UI;

namespace ARIA.Editor
{
    public class SceneSetup
    {
        [MenuItem("ARIA/一键搭建完整场景")]
        public static void SetupCompleteScene()
        {
            Debug.Log("=== 开始一键搭建场景 ===");

            // 1. 生成数据
            Debug.Log("[1/6] 生成游戏数据...");
            DataGenerator.GenerateInitialData();

            // 2. 创建场景
            Debug.Log("[2/6] 创建游戏场景...");
            CreateGameScene();

            // 3. 设置相机
            Debug.Log("[3/6] 设置相机...");
            SetupCamera();

            // 4. 创建场景对象结构
            Debug.Log("[4/6] 创建场景对象结构...");
            CreateSceneHierarchy();

            // 5. 创建预制体
            Debug.Log("[5/6] 创建预制体...");
            PrefabGenerator.GenerateAllPrefabs();

            // 6. 配置管理器
            Debug.Log("[6/6] 配置管理器...");
            SetupManagers();

            // 保存场景
            EditorSceneManager.SaveOpenScenes();

            Debug.Log("=== 场景搭建完成！===");
            Debug.Log("现在可以点击 Play 按钮测试游戏了");
        }

        [MenuItem("ARIA/快速重置场景")]
        public static void ResetScene()
        {
            if (EditorUtility.DisplayDialog("确认重置", "确定要清空当前场景并重新搭建吗？", "确定", "取消"))
            {
                // 删除所有对象
                var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                foreach (var obj in allObjects)
                {
                    if (obj.transform.parent == null && obj.hideFlags == HideFlags.None)
                    {
                        Object.DestroyImmediate(obj);
                    }
                }

                SetupCompleteScene();
            }
        }

        private static void CreateGameScene()
        {
            // 创建Scenes文件夹
            string scenesPath = "Assets/Scenes";
            if (!Directory.Exists(scenesPath))
            {
                Directory.CreateDirectory(scenesPath);
            }

            // 创建新场景
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // 设置为2D模式
            SceneView.lastActiveSceneView.in2DMode = true;

            // 保存场景
            string scenePath = "Assets/Scenes/Game.unity";
            EditorSceneManager.SaveScene(newScene, scenePath);

            Debug.Log($"场景已创建: {scenePath}");
        }

        private static void SetupCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObj = new GameObject("Main Camera");
                mainCamera = cameraObj.AddComponent<Camera>();
                cameraObj.tag = "MainCamera";
            }

            // 设置相机参数
            mainCamera.transform.position = new Vector3(25, 25, -10);
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 20;
            mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.18f); // #1a1a2e
            mainCamera.clearFlags = CameraClearFlags.SolidColor;

            Debug.Log("相机设置完成");
        }

        private static void CreateSceneHierarchy()
        {
            // 创建GameBootstrapper
            GameObject bootstrapper = new GameObject("GameBootstrapper");
            var bootstrapperComponent = bootstrapper.AddComponent<GameBootstrapper>();
            bootstrapperComponent.CreateInitialCommandCenter = true;
            bootstrapperComponent.CommandCenterPosition = new Vector2Int(25, 25);

            // 创建Managers容器
            GameObject managers = new GameObject("Managers");

            // 创建Building容器
            GameObject buildings = new GameObject("Buildings");

            // 创建Enemy容器
            GameObject enemies = new GameObject("Enemies");

            // 创建SpawnPoints
            GameObject spawnPoints = new GameObject("SpawnPoints");
            CreateSpawnPoint(spawnPoints.transform, "SpawnPoint_1", new Vector3(5, 25, 0));
            CreateSpawnPoint(spawnPoints.transform, "SpawnPoint_2", new Vector3(45, 25, 0));
            CreateSpawnPoint(spawnPoints.transform, "SpawnPoint_3", new Vector3(25, 5, 0));
            CreateSpawnPoint(spawnPoints.transform, "SpawnPoint_4", new Vector3(25, 45, 0));

            // 创建TargetPoint
            GameObject targetPoint = new GameObject("TargetPoint");
            targetPoint.transform.position = new Vector3(25, 25, 0);

            // 创建地图背景
            CreateMapBackground();

            Debug.Log("场景对象结构创建完成");
        }

        private static void CreateSpawnPoint(Transform parent, string name, Vector3 position)
        {
            GameObject spawnPoint = new GameObject(name);
            spawnPoint.transform.SetParent(parent);
            spawnPoint.transform.position = position;
        }

        private static void CreateMapBackground()
        {
            // 创建简单的地面背景
            GameObject ground = new GameObject("Ground");
            SpriteRenderer sr = ground.AddComponent<SpriteRenderer>();

            // 创建纯色纹理
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, new Color(0.15f, 0.2f, 0.15f));
            texture.Apply();

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = new Vector2(50, 50);
            sr.sortingOrder = -100; // 放在最底层

            ground.transform.position = new Vector3(25, 25, 0);

            Debug.Log("地图背景创建完成");
        }

        private static void SetupManagers()
        {
            // 查找GameBootstrapper
            GameBootstrapper bootstrapper = Object.FindFirstObjectByType<GameBootstrapper>();
            if (bootstrapper == null)
            {
                Debug.LogError("找不到GameBootstrapper");
                return;
            }

            // 查找Managers容器
            GameObject managers = GameObject.Find("Managers");
            if (managers == null)
            {
                managers = new GameObject("Managers");
            }

            // 添加BuildingPlacementController
            var placementController = managers.GetComponent<BuildingPlacementController>();
            if (placementController == null)
            {
                placementController = managers.AddComponent<BuildingPlacementController>();
                Debug.Log("BuildingPlacementController已添加到场景");
            }

            // 加载所有数据资源
            string[] dataFolders = new string[] { "Cards", "Buildings", "Resources", "Enemies", "Techs", "Waves" };

            foreach (var folder in dataFolders)
            {
                string path = $"Assets/Data/{folder}";
                if (Directory.Exists(path))
                {
                    Debug.Log($"找到数据文件夹: {path}");
                }
            }

            Debug.Log("管理器配置完成");
        }
    }
}
