using UnityEngine;
using System;
using System.Collections.Generic;
using ARIA.Core;
using ARIA.Resource;
using ARIA.Power;
using ARIA.Tech;

namespace ARIA.Building
{
    public class BuildingManager : MonoBehaviour
    {
        public static BuildingManager Instance { get; private set; }

        [Header("Grid Settings")]
        public int GridWidth = 50;
        public int GridHeight = 50;
        public float CellSize = 1f;
        public Vector2 GridOrigin = Vector2.zero;

        [Header("Prefabs")]
        public GameObject BuildingPrefab;
        public GameObject CommandCenterPrefab;

        [Header("Layer Settings")]
        public LayerMask BuildingLayer;
        public LayerMask ResourceLayer;

        private Building[,] grid;
        private List<Building> allBuildings = new List<Building>();
        private int nextBuildingInstanceId = 1;

        public event Action<Building> OnBuildingPlaced;
        public event Action<Building> OnBuildingRemoved;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            grid = new Building[GridWidth, GridHeight];
        }

        public bool CanPlaceBuilding(Card.CardData cardData, Vector2Int position)
        {
            if (cardData == null || cardData.BuildingData == null) return false;

            int sizeX = cardData.BuildingSizeX;
            int sizeY = cardData.BuildingSizeY;

            if (!IsPositionValid(position, sizeX, sizeY)) return false;

            for (int x = position.x; x < position.x + sizeX; x++)
            {
                for (int y = position.y; y < position.y + sizeY; y++)
                {
                    if (grid[x, y] != null) return false;
                }
            }

            return true;
        }

        public bool PlaceBuilding(Card.CardData cardData, Vector2Int position)
        {
            if (!CanPlaceBuilding(cardData, position)) return false;

            BuildingData buildingData = cardData.BuildingData;
            
            // 使用卡牌上的大小，确保检测和实际放置使用相同的大小
            int sizeX = cardData.BuildingSizeX;
            int sizeY = cardData.BuildingSizeY;
            
            // 计算精确的世界坐标
            Vector2 exactWorldPos = GetWorldPosition(position, sizeX, sizeY);
            
            // 如果没有设置预制体，创建一个默认的
            GameObject buildingObj;
            if (BuildingPrefab != null)
            {
                buildingObj = Instantiate(BuildingPrefab, exactWorldPos, Quaternion.identity);
            }
            else
            {
                buildingObj = new GameObject(buildingData.BuildingName);
                buildingObj.transform.position = exactWorldPos;
                
                // 添加SpriteRenderer
                SpriteRenderer sr = buildingObj.AddComponent<SpriteRenderer>();
                sr.sprite = CreateDefaultSprite(buildingData.Category);
                sr.sortingOrder = 0;
                
                // 添加Collider
                BoxCollider2D collider = buildingObj.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(sizeX, sizeY);
            }
            
            Building building = buildingObj.GetComponent<Building>();
            if (building == null)
            {
                building = buildingObj.AddComponent<Building>();
            }

            building.Initialize(nextBuildingInstanceId++, buildingData, position);
            
            for (int x = position.x; x < position.x + sizeX; x++)
            {
                for (int y = position.y; y < position.y + sizeY; y++)
                {
                    grid[x, y] = building;
                }
            }

            allBuildings.Add(building);

            if (buildingData.PowerGeneration > 0)
            {
                PowerManager.Instance?.AddPowerGeneration(building.InstanceId, buildingData.PowerGeneration);
            }
            if (buildingData.PowerConsumption > 0)
            {
                PowerManager.Instance?.AddPowerConsumption(building.InstanceId, buildingData.PowerConsumption);
            }

            ResourceNetwork.Instance?.RegisterBuilding(building);

            OnBuildingPlaced?.Invoke(building);
            EventManager.Instance?.TriggerEvent(GameEvents.BUILDING_PLACED, building);
            
            // 播放建筑完成特效
            building.PlayBuildEffect();

            return true;
        }

        public void RemoveBuilding(Building building)
        {
            if (building == null) return;

            Vector2Int pos = building.GridPosition;
            int sizeX = building.Data.SizeX;
            int sizeY = building.Data.SizeY;

            for (int x = pos.x; x < pos.x + sizeX; x++)
            {
                for (int y = pos.y; y < pos.y + sizeY; y++)
                {
                    if (x < GridWidth && y < GridHeight)
                    {
                        grid[x, y] = null;
                    }
                }
            }

            allBuildings.Remove(building);

            if (building.Data.PowerGeneration > 0)
            {
                PowerManager.Instance?.RemovePowerGeneration(building.InstanceId);
            }
            if (building.Data.PowerConsumption > 0)
            {
                PowerManager.Instance?.RemovePowerConsumption(building.InstanceId);
            }

            ResourceNetwork.Instance?.UnregisterBuilding(building);

            Card.CardManager.Instance?.AddCard(building.Data.BuildingId, 1);

            OnBuildingRemoved?.Invoke(building);
            EventManager.Instance?.TriggerEvent(GameEvents.BUILDING_REMOVED, building);

            Destroy(building.gameObject);
        }

        public bool CanUpgradeBuilding(Building building)
        {
            if (building == null) return false;
            if (string.IsNullOrEmpty(building.Data.NextUpgradeId)) return false;

            Card.CardData upgradeCard = Card.CardManager.Instance?.GetCardData(building.Data.NextUpgradeId);
            if (upgradeCard == null || upgradeCard.BuildingData == null) return false;

            // 检查资源是否足够
            foreach (var resource in building.Data.UpgradeResources)
            {
                if (!ResourceManager.Instance.HasResource(resource.ResourceId, resource.Amount))
                {
                    return false;
                }
            }

            // 检查技术需求
            BuildingData upgradeData = upgradeCard.BuildingData;
            foreach (var techId in upgradeData.RequiredTechIds)
            {
                if (!Tech.TechManager.Instance?.IsTechUnlocked(techId) ?? true)
                {
                    return false;
                }
            }

            return true;
        }

        public bool UpgradeBuilding(Building building)
        {
            if (!CanUpgradeBuilding(building)) return false;

            Vector2Int position = building.GridPosition;
            string upgradeId = building.Data.NextUpgradeId;

            // 消耗升级资源
            foreach (var resource in building.Data.UpgradeResources)
            {
                ResourceManager.Instance.RemoveResource(resource.ResourceId, resource.Amount);
            }

            // 移除旧建筑
            RemoveBuilding(building);

            // 放置新建筑
            Card.CardData upgradeCard = Card.CardManager.Instance?.GetCardData(upgradeId);
            if (upgradeCard != null)
            {
                bool placed = PlaceBuilding(upgradeCard, position);
                if (placed)
                {
                    EventManager.Instance?.TriggerEvent(GameEvents.BUILDING_UPGRADED, position);
                    return true;
                }
            }

            return false;
        }

        public Building GetBuildingAt(Vector2Int position)
        {
            if (!IsPositionValid(position, 1, 1)) return null;
            return grid[position.x, position.y];
        }

        public Building GetBuildingAtWorld(Vector2 worldPosition)
        {
            Vector2Int gridPos = GetGridPosition(worldPosition);
            return GetBuildingAt(gridPos);
        }

        public List<Building> GetAllBuildings()
        {
            return new List<Building>(allBuildings);
        }

        public List<Building> GetBuildingsByCategory(BuildingCategory category)
        {
            return allBuildings.FindAll(b => b.Data.Category == category);
        }

        public List<Building> GetBuildingsInRange(Vector2Int center, int range)
        {
            List<Building> result = new List<Building>();
            foreach (var building in allBuildings)
            {
                float distance = Vector2Int.Distance(center, building.GridPosition);
                if (distance <= range)
                {
                    result.Add(building);
                }
            }
            return result;
        }

        // ----------------- 完美的坐标系转换算法 -----------------

        /// <summary>
        /// (核心) 给定一个世界坐标（例如鼠标位置，视为建筑中心），计算出该建筑左下角的网格坐标
        /// </summary>
        public Vector2Int GetGridPositionFromCenter(Vector2 centerWorldPos, int sizeX = 1, int sizeY = 1)
        {
            // 计算鼠标所在的网格坐标（作为建筑中心）
            int centerGridX = Mathf.RoundToInt((centerWorldPos.x - GridOrigin.x) / CellSize);
            int centerGridY = Mathf.RoundToInt((centerWorldPos.y - GridOrigin.y) / CellSize);

            // 根据建筑尺寸计算左下角的网格坐标，确保建筑中心在鼠标位置
            int x = centerGridX - (sizeX - 1) / 2;
            int y = centerGridY - (sizeY - 1) / 2;

            return new Vector2Int(x, y);
        }

        /// <summary>
        /// 根据建筑左下角的网格坐标和自身尺寸，推算出严格吸附网格后的【完美中心世界坐标】
        /// </summary>
        public Vector2 GetWorldPosition(Vector2Int gridPosition, int sizeX = 1, int sizeY = 1)
        {
            return new Vector2(
                GridOrigin.x + gridPosition.x * CellSize + (sizeX * CellSize) * 0.5f,
                GridOrigin.y + gridPosition.y * CellSize + (sizeY * CellSize) * 0.5f
            );
        }

        /// <summary>
        /// 兼容旧方法的重载 (如果获取单格的位置)
        /// </summary>
        public Vector2Int GetGridPosition(Vector2 worldPosition)
        {
            return GetGridPositionFromCenter(worldPosition, 1, 1);
        }

        private bool IsPositionValid(Vector2Int position, int sizeX, int sizeY)
        {
            return position.x >= 0 && position.x + sizeX <= GridWidth &&
                   position.y >= 0 && position.y + sizeY <= GridHeight;
        }

        public BuildingSaveData[] GetSaveData()
        {
            List<BuildingSaveData> saveData = new List<BuildingSaveData>();
            foreach (var building in allBuildings)
            {
                saveData.Add(new BuildingSaveData
                {
                    BuildingId = building.Data.BuildingId,
                    PosX = building.GridPosition.x,
                    PosY = building.GridPosition.y,
                    Health = building.CurrentHealth,
                    IsActive = building.IsActive
                });
            }
            return saveData.ToArray();
        }

        public void ApplySaveData(BuildingSaveData[] saveData)
        {
            foreach (var building in allBuildings.ToArray())
            {
                RemoveBuilding(building);
            }

            foreach (var data in saveData)
            {
                Card.CardData cardData = Card.CardManager.Instance?.GetCardData(data.BuildingId);
                if (cardData != null)
                {
                    Vector2Int pos = new Vector2Int(data.PosX, data.PosY);
                    if (PlaceBuilding(cardData, pos))
                    {
                        Building building = GetBuildingAt(pos);
                        if (building != null)
                        {
                            building.CurrentHealth = data.Health;
                            building.IsActive = data.IsActive;
                        }
                    }
                }
            }
        }
        
        private Sprite CreateDefaultSprite(BuildingCategory category)
        {
            // 创建纯色纹理
            Texture2D texture = new Texture2D(32, 32);
            Color color = GetCategoryColor(category);
            
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        }
        
        private Color GetCategoryColor(BuildingCategory category)
        {
            switch (category)
            {
                case BuildingCategory.Special: return new Color(1f, 0.8f, 0.2f);      // 金色
                case BuildingCategory.Resource: return new Color(0.4f, 0.7f, 1f);     // 蓝色
                case BuildingCategory.Production: return new Color(0.9f, 0.5f, 0.2f); // 橙色
                case BuildingCategory.Power: return new Color(1f, 0.9f, 0.2f);        // 黄色
                case BuildingCategory.Storage: return new Color(0.6f, 0.4f, 0.2f);    // 棕色
                case BuildingCategory.Defense: return new Color(0.8f, 0.2f, 0.2f);    // 红色
                default: return Color.gray;
            }
        }
    }
}
