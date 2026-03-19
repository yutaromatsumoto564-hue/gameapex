using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace ARIA.Editor
{
    public class DataGenerator
    {
        [MenuItem("ARIA/Generate Initial Data")]
        public static void GenerateInitialData()
        {
            string basePath = "Assets/Data";
            
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
            
            GenerateResourceData(basePath + "/Resources");
            GenerateBuildingData(basePath + "/Buildings");  // 先生成建筑数据
            GenerateCardData(basePath + "/Cards");          // 后生成卡牌数据（需要引用建筑）
            GenerateTechData(basePath + "/Techs");
            GenerateWaveData(basePath + "/Waves");
            
            AssetDatabase.Refresh();
            
            Debug.Log("Initial game data generated successfully!");
        }
        
        private static void GenerateResourceData(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            
            // Raw Resources
            CreateResourceData("resource_iron_ore", "铁矿石", "从地下挖掘的原始铁矿石", path, "Raw", 1, 10);
            CreateResourceData("resource_copper_ore", "铜矿石", "从地下挖掘的原始铜矿石", path, "Raw", 1, 8);
            CreateResourceData("resource_coal", "煤炭", "用于燃烧提供能量的化石燃料", path, "Raw", 1, 5);
            CreateResourceData("resource_stone", "石材", "用于建筑的基础材料", path, "Raw", 1, 3);
            CreateResourceData("resource_wood", "木材", "用于建筑和加工的基础材料", path, "Raw", 1, 4);
            
            // Basic Materials
            CreateResourceData("resource_iron_plate", "铁板", "精炼后的铁板", path, "Basic", 2, 25);
            CreateResourceData("resource_copper_wire", "铜线", "用于电力传输的铜线", path, "Basic", 2, 20);
            CreateResourceData("resource_brick", "砖块", "烧制的砖块，用于建筑", path, "Basic", 2, 10);
            CreateResourceData("resource_gear", "齿轮", "机械组件", path, "Basic", 2, 30);
            
            // Intermediate Materials
            CreateResourceData("resource_steel", "钢材", "高强度钢材", path, "Intermediate", 3, 60);
            CreateResourceData("resource_electronic_circuit", "电子电路", "基础电子组件", path, "Intermediate", 3, 80);
            CreateResourceData("resource_concrete", "混凝土", "高强度建筑材料", path, "Intermediate", 3, 40);
            
            // Advanced Materials
            CreateResourceData("resource_advanced_circuit", "高级电路", "复杂电子组件", path, "Advanced", 4, 200);
            CreateResourceData("resource_titanium_alloy", "钛合金", "超高强度合金", path, "Advanced", 4, 250);
            
            // Organic
            CreateResourceData("resource_organic_matter", "有机物", "从虫族身上获取的有机物", path, "Organic", 1, 15);
            CreateResourceData("resource_chitin", "甲壳", "坚固的虫族甲壳", path, "Organic", 2, 40);
            CreateResourceData("resource_zerg_core", "虫族核心", "稀有的虫族核心，价值很高", path, "Organic", 3, 150);
        }
        
        private static void CreateResourceData(string id, string name, string desc, string path, string category, int tier, int value)
        {
            var data = ScriptableObject.CreateInstance<Resource.ResourceData>();
            data.ResourceId = id;
            data.ResourceName = name;
            data.Description = desc;
            data.Category = (Resource.ResourceCategory)System.Enum.Parse(typeof(Resource.ResourceCategory), category);
            data.Tier = tier;
            data.BaseValue = value;
            
            string assetPath = $"{path}/{id}.asset";
            AssetDatabase.CreateAsset(data, assetPath);
        }
        
        private static void GenerateCardData(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            
            // Resource Cards
            CreateCardData("card_iron_ore", "铁矿石卡", "铁矿石卡牌", path, "Resource", "Common", "resource_iron_ore");
            CreateCardData("card_copper_ore", "铜矿石卡", "铜矿石卡牌", path, "Resource", "Common", "resource_copper_ore");
            CreateCardData("card_coal", "煤炭卡", "煤炭卡牌", path, "Resource", "Common", "resource_coal");
            CreateCardData("card_stone", "石材卡", "石材卡牌", path, "Resource", "Common", "resource_stone");
            CreateCardData("card_wood", "木材卡", "木材卡牌", path, "Resource", "Common", "resource_wood");
            
            // Building Cards
            CreateCardData("card_command_center", "指挥中心卡", "核心建筑卡", path, "Building", "Legendary", "building_command_center");
            CreateCardData("card_iron_miner", "铁矿机卡", "铁矿机建筑卡", path, "Building", "Common", "building_iron_miner");
            CreateCardData("card_copper_miner", "铜矿机卡", "铜矿机建筑卡", path, "Building", "Common", "building_copper_miner");
            CreateCardData("card_lumber_mill", "伐木厂卡", "伐木厂建筑卡", path, "Building", "Common", "building_lumber_mill");
            CreateCardData("card_furnace", "熔炉卡", "熔炉建筑卡", path, "Building", "Uncommon", "building_furnace");
            CreateCardData("card_steam_generator", "蒸汽发电机卡", "蒸汽发电机建筑卡", path, "Building", "Uncommon", "building_steam_generator");
            CreateCardData("card_storage", "仓库卡", "仓库建筑卡", path, "Building", "Common", "building_storage");
            CreateCardData("card_turret", "炮塔卡", "炮塔建筑卡", path, "Building", "Uncommon", "building_turret");
            CreateCardData("card_wall", "围墙卡", "围墙建筑卡", path, "Building", "Common", "building_wall");
        }
        
        private static void CreateCardData(string id, string name, string desc, string path, string type, string rarity, string linkedId = "")
        {
            var data = ScriptableObject.CreateInstance<Card.CardData>();
            data.CardId = id;
            data.CardName = name;
            data.Description = desc;
            data.Type = (Card.CardType)System.Enum.Parse(typeof(Card.CardType), type);
            data.Rarity = (Card.CardRarity)System.Enum.Parse(typeof(Card.CardRarity), rarity);
            data.CanStack = true;
            data.MaxStack = 99;
            
            // 如果是建筑卡牌，关联对应的建筑数据
            if (type == "Building" && !string.IsNullOrEmpty(linkedId))
            {
                string buildingPath = $"Assets/Data/Buildings/{linkedId}.asset";
                Building.BuildingData buildingData = AssetDatabase.LoadAssetAtPath<Building.BuildingData>(buildingPath);
                if (buildingData != null)
                {
                    data.BuildingData = buildingData;
                    data.BuildingSizeX = buildingData.SizeX;
                    data.BuildingSizeY = buildingData.SizeY;
                }
                else
                {
                    Debug.LogWarning($"Building data not found for card {id}: {buildingPath}");
                }
            }
            
            string assetPath = $"{path}/{id}.asset";
            AssetDatabase.CreateAsset(data, assetPath);
        }
        
        private static void GenerateBuildingData(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            
            // Command Center - 产生能量
            CreateBuildingDataWithOutput("building_command_center", "指挥中心", "你的基地核心建筑，缓慢产生能量", path, "Resource", 
                5, 5, 5000, 0, 0, 15, 5f, "res_energy", 5);
            
            // Iron Miner
            CreateBuildingDataWithOutput("building_iron_miner", "铁矿机", "自动开采铁矿石", path, "Resource", 
                2, 2, 200, 50, 0, 5, 3f, "res_metal", 2);
            
            // Copper Miner
            CreateBuildingDataWithOutput("building_copper_miner", "铜矿机", "自动开采铜矿石", path, "Resource", 
                2, 2, 200, 50, 0, 5, 3f, "res_crystal", 2);
            
            // Lumber Mill
            CreateBuildingDataWithOutput("building_lumber_mill", "伐木厂", "自动采集木材", path, "Resource", 
                2, 2, 150, 30, 0, 5, 2f, "res_organic", 3);
            
            // Furnace
            CreateBuildingDataSimple("building_furnace", "熔炉", "将矿石精炼成金属", path, "Production", 
                2, 2, 300, 80, 0, 5, 4f);
            
            // Steam Generator
            CreateBuildingDataSimple("building_steam_generator", "蒸汽发电机", "燃烧煤炭产生电力", path, "Power", 
                3, 3, 400, 0, 500, 5, 0);
            
            // Storage
            CreateBuildingDataSimple("building_storage", "仓库", "增加资源存储容量", path, "Storage", 
                2, 2, 300, 0, 0, 5, 0);
            
            // Turret
            CreateBuildingDataSimple("building_turret", "炮塔", "自动攻击范围内的敌人", path, "Defense", 
                2, 2, 500, 100, 0, 5, 0);
            
            // Wall
            CreateBuildingDataSimple("building_wall", "围墙", "阻挡敌人前进", path, "Defense", 
                1, 1, 800, 0, 0, 0, 0);
        }
        
        private static void CreateBuildingDataSimple(string id, string name, string desc, string path, string category, 
            int sizeX, int sizeY, int maxHealth, int powerConsumption, int powerGeneration, int networkRange,
            float productionTime)
        {
            var data = ScriptableObject.CreateInstance<Building.BuildingData>();
            data.BuildingId = id;
            data.BuildingName = name;
            data.Description = desc;
            data.Category = (Building.BuildingCategory)System.Enum.Parse(typeof(Building.BuildingCategory), category);
            data.SizeX = sizeX;
            data.SizeY = sizeY;
            data.MaxHealth = maxHealth;
            data.PowerConsumption = powerConsumption;
            data.PowerGeneration = powerGeneration;
            data.NetworkRange = networkRange;
            data.ProductionTime = productionTime;
            data.Inputs = new List<Building.ResourceIO>();
            data.Outputs = new List<Building.ResourceIO>();
            
            string assetPath = $"{path}/{id}.asset";
            AssetDatabase.CreateAsset(data, assetPath);
        }
        
        private static void CreateBuildingDataWithOutput(string id, string name, string desc, string path, string category, 
            int sizeX, int sizeY, int maxHealth, int powerConsumption, int powerGeneration, int networkRange,
            float productionTime, string outputResourceId, int outputAmount)
        {
            var data = ScriptableObject.CreateInstance<Building.BuildingData>();
            data.BuildingId = id;
            data.BuildingName = name;
            data.Description = desc;
            data.Category = (Building.BuildingCategory)System.Enum.Parse(typeof(Building.BuildingCategory), category);
            data.SizeX = sizeX;
            data.SizeY = sizeY;
            data.MaxHealth = maxHealth;
            data.PowerConsumption = powerConsumption;
            data.PowerGeneration = powerGeneration;
            data.NetworkRange = networkRange;
            data.ProductionTime = productionTime;
            data.Inputs = new List<Building.ResourceIO>();
            data.Outputs = new List<Building.ResourceIO>();
            
            // 添加产出
            if (!string.IsNullOrEmpty(outputResourceId) && outputAmount > 0)
            {
                data.Outputs.Add(new Building.ResourceIO { ResourceId = outputResourceId, Amount = outputAmount });
            }
            
            string assetPath = $"{path}/{id}.asset";
            AssetDatabase.CreateAsset(data, assetPath);
        }
        
        private static void GenerateTechData(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            
            CreateTechData("tech_improved_mining", "改进采矿", "提高采矿机效率", path, "Engineering", 1,
                new string[] {}, new string[] { "resource_iron_plate" }, new int[] { 20 }, 60f,
                new string[] {}, new string[] {},
                new Tech.TechBonusType[] { Tech.TechBonusType.ProductionSpeed }, new float[] { 0.2f });
            
            CreateTechData("tech_advanced_power", "高级电力", "提高发电效率", path, "Physics", 1,
                new string[] {}, new string[] { "resource_copper_wire", "resource_iron_plate" }, new int[] { 30, 20 }, 90f,
                new string[] {}, new string[] {},
                new Tech.TechBonusType[] { Tech.TechBonusType.PowerEfficiency }, new float[] { 0.25f });
            
            CreateTechData("tech_reinforced_walls", "强化围墙", "提高围墙生命值", path, "Engineering", 1,
                new string[] {}, new string[] { "resource_brick", "resource_iron_plate" }, new int[] { 40, 25 }, 75f,
                new string[] {}, new string[] {},
                new Tech.TechBonusType[] { Tech.TechBonusType.BuildingHealth }, new float[] { 0.5f });
            
            CreateTechData("tech_improved_turrets", "改进炮塔", "提高炮塔伤害和射程", path, "Engineering", 2,
                new string[] { "tech_reinforced_walls" }, new string[] { "resource_steel", "resource_electronic_circuit" }, new int[] { 30, 20 }, 120f,
                new string[] {}, new string[] {},
                new Tech.TechBonusType[] { Tech.TechBonusType.TurretDamage, Tech.TechBonusType.TurretRange }, new float[] { 0.3f, 0.2f });
        }
        
        private static void CreateTechData(string id, string name, string desc, string path, string category, int tier,
            string[] prereqTechs, string[] costResources, int[] costAmounts, float researchTime,
            string[] unlockedBuildings, string[] unlockedCards,
            Tech.TechBonusType[] bonusTypes, float[] bonusValues)
        {
            var data = ScriptableObject.CreateInstance<Tech.TechData>();
            data.TechId = id;
            data.TechName = name;
            data.Description = desc;
            data.Category = (Tech.TechCategory)System.Enum.Parse(typeof(Tech.TechCategory), category);
            data.Tier = tier;
            data.ResearchTime = researchTime;
            
            foreach (var prereq in prereqTechs)
            {
                data.PrerequisiteTechIds.Add(prereq);
            }
            
            for (int i = 0; i < costResources.Length; i++)
            {
                data.ResearchCosts.Add(new Tech.TechCost { ResourceId = costResources[i], Amount = costAmounts[i] });
            }
            
            foreach (var building in unlockedBuildings)
            {
                data.UnlockedBuildingIds.Add(building);
            }
            
            foreach (var card in unlockedCards)
            {
                data.UnlockedCardIds.Add(card);
            }
            
            for (int i = 0; i < bonusTypes.Length; i++)
            {
                data.Bonuses.Add(new Tech.TechBonus { Type = bonusTypes[i], Value = bonusValues[i] });
            }
            
            string assetPath = $"{path}/{id}.asset";
            AssetDatabase.CreateAsset(data, assetPath);
        }
        
        private static void GenerateWaveData(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            
            CreateWaveData("wave_day1_wave1", 1, 1, path, 5, 2, 100);
            CreateWaveData("wave_day2_wave1", 2, 1, path, 5, 2, 200);
            CreateWaveData("wave_day3_wave1", 3, 1, path, 5, 1.5f, 300);
        }
        
        private static void CreateWaveData(string id, int day, int wave, string path, float delay, float spawnInterval, int reward)
        {
            var data = ScriptableObject.CreateInstance<DayNight.WaveData>();
            data.WaveNumber = wave;
            data.DayNumber = day;
            data.WaveDelay = delay;
            data.SpawnInterval = spawnInterval;
            data.RewardGold = reward;
            
            string assetPath = $"{path}/{id}.asset";
            AssetDatabase.CreateAsset(data, assetPath);
        }
    }
}
