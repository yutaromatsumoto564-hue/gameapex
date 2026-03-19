using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

namespace ARIA.Editor
{
    public class PrefabGenerator
    {
        [MenuItem("ARIA/Generate All Prefabs")]
        public static void GenerateAllPrefabs()
        {
            Debug.Log("=== 开始生成预制体 ===");

            CreateFolderStructure();
            GenerateBuildingPrefabs();
            GenerateUIPrefabs();

            AssetDatabase.Refresh();
            Debug.Log("=== 预制体生成完成 ===");
        }

        private static void CreateFolderStructure()
        {
            string[] folders = new string[]
            {
                "Assets/Prefabs",
                "Assets/Prefabs/Buildings",
                "Assets/Prefabs/Enemies",
                "Assets/Prefabs/UI",
                "Assets/Prefabs/Effects"
            };

            foreach (var folder in folders)
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                    Debug.Log($"创建文件夹: {folder}");
                }
            }
        }

        private static void GenerateBuildingPrefabs()
        {
            Debug.Log("生成建筑预制体...");

            // 指挥中心 - 5x5 紫色
            CreateBuildingPrefab("building_command_center", "指挥中心", new Color(0.6f, 0.3f, 0.8f), 5, 5, 5000);

            // 铁矿机 - 2x2 灰色
            CreateBuildingPrefab("building_iron_miner", "铁矿机", new Color(0.5f, 0.5f, 0.5f), 2, 2, 200);

            // 铜矿机 - 2x2 橙色
            CreateBuildingPrefab("building_copper_miner", "铜矿机", new Color(0.8f, 0.5f, 0.2f), 2, 2, 200);

            // 伐木厂 - 2x2 棕色
            CreateBuildingPrefab("building_lumber_mill", "伐木厂", new Color(0.4f, 0.3f, 0.2f), 2, 2, 150);

            // 熔炉 - 2x2 红色
            CreateBuildingPrefab("building_furnace", "熔炉", new Color(0.8f, 0.3f, 0.2f), 2, 2, 300);

            // 蒸汽发电机 - 3x3 黄色
            CreateBuildingPrefab("building_steam_generator", "蒸汽发电机", new Color(0.9f, 0.8f, 0.2f), 3, 3, 400);

            // 仓库 - 2x2 蓝色
            CreateBuildingPrefab("building_storage", "仓库", new Color(0.3f, 0.5f, 0.8f), 2, 2, 300);

            // 炮塔 - 2x2 绿色
            CreateBuildingPrefab("building_turret", "炮塔", new Color(0.3f, 0.7f, 0.4f), 2, 2, 500);

            // 围墙 - 1x1 深灰色
            CreateBuildingPrefab("building_wall", "围墙", new Color(0.3f, 0.3f, 0.3f), 1, 1, 800);

            Debug.Log("建筑预制体生成完成");
        }

        private static void CreateBuildingPrefab(string id, string name, Color color, int sizeX, int sizeY, int maxHealth)
        {
            string prefabPath = $"Assets/Prefabs/Buildings/{id}.prefab";

            // 检查是否已存在
            if (File.Exists(prefabPath))
            {
                Debug.Log($"预制体已存在: {id}");
                return;
            }

            // 创建游戏对象
            GameObject buildingObj = new GameObject(name);

            // 添加SpriteRenderer
            SpriteRenderer sr = buildingObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateColoredSprite(color);
            sr.sortingOrder = 0;

            // 添加Collider
            BoxCollider2D collider = buildingObj.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(sizeX, sizeY);

            // 添加Building脚本
            Building.Building building = buildingObj.AddComponent<Building.Building>();

            // 保存为预制体
            PrefabUtility.SaveAsPrefabAsset(buildingObj, prefabPath);
            Object.DestroyImmediate(buildingObj);

            Debug.Log($"创建建筑预制体: {id}");
        }

        private static void GenerateUIPrefabs()
        {
            Debug.Log("生成UI预制体...");

            // 创建Canvas
            CreateCanvasPrefab();

            // 创建卡牌槽预制体
            CreateCardSlotPrefab();

            Debug.Log("UI预制体生成完成");
        }

        private static void CreateCanvasPrefab()
        {
            string prefabPath = "Assets/Prefabs/UI/GameCanvas.prefab";

            if (File.Exists(prefabPath))
            {
                Debug.Log("Canvas预制体已存在");
                return;
            }

            // 创建Canvas
            GameObject canvasObj = new GameObject("GameCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            // 添加CanvasScaler
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // 添加GraphicRaycaster
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // 保存为预制体
            PrefabUtility.SaveAsPrefabAsset(canvasObj, prefabPath);
            Object.DestroyImmediate(canvasObj);

            Debug.Log("创建Canvas预制体");
        }

        private static void CreateCardSlotPrefab()
        {
            string prefabPath = "Assets/Prefabs/UI/CardSlot.prefab";

            if (File.Exists(prefabPath))
            {
                Debug.Log("CardSlot预制体已存在");
                return;
            }

            // 创建卡牌槽对象
            GameObject slotObj = new GameObject("CardSlot");
            RectTransform rt = slotObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100, 120);

            // 添加背景图片
            UnityEngine.UI.Image bgImage = slotObj.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);

            // 保存为预制体
            PrefabUtility.SaveAsPrefabAsset(slotObj, prefabPath);
            Object.DestroyImmediate(slotObj);

            Debug.Log("创建CardSlot预制体");
        }

        private static Sprite CreateColoredSprite(Color color)
        {
            // 创建纯色纹理
            Texture2D texture = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply();

            // 创建Sprite
            return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        }
    }
}
