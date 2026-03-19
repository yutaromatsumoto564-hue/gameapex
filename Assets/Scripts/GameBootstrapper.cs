using UnityEngine;
using ARIA.Core;
using ARIA.Card;
using ARIA.Building;
using ARIA.Resource;
using ARIA.Power;
using ARIA.DayNight;
using ARIA.Tech;
using ARIA.UI;

namespace ARIA
{
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Manager Prefabs")]
        public GameObject GameManagerPrefab;
        public GameObject SaveSystemPrefab;
        public GameObject EventManagerPrefab;
        public GameObject CardManagerPrefab;
        public GameObject BuildingManagerPrefab;
        public GameObject ResourceManagerPrefab;
        public GameObject ResourceNetworkPrefab;
        public GameObject PowerManagerPrefab;
        public GameObject DayNightCyclePrefab;
        public GameObject WaveManagerPrefab;
        public GameObject TechManagerPrefab;
        public GameObject UIManagerPrefab;

        [Header("Scene Setup")]
        public bool CreateInitialCommandCenter = true;
        public Vector2Int CommandCenterPosition = new Vector2Int(25, 25);

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
        }

        private void Start()
        {
            if (CreateInitialCommandCenter)
            {
                CreateCommandCenter();
            }
            
            GameManager.Instance?.ResumeGame();
        }

        private void InitializeManagers()
        {
            // 创建核心管理器（按依赖顺序）
            CreateManagerIfNeeded(EventManagerPrefab, typeof(EventManager));
            CreateManagerIfNeeded(SaveSystemPrefab, typeof(SaveSystem));
            CreateManagerIfNeeded(GameManagerPrefab, typeof(GameManager));
            
            // 卡牌系统
            CreateManagerIfNeeded(CardManagerPrefab, typeof(CardManager));
            
            // 建筑系统
            CreateManagerIfNeeded(BuildingManagerPrefab, typeof(BuildingManager));
            
            // 资源系统
            CreateManagerIfNeeded(ResourceManagerPrefab, typeof(ResourceManager));
            CreateManagerIfNeeded(ResourceNetworkPrefab, typeof(ResourceNetwork));
            
            // 电力系统
            CreateManagerIfNeeded(PowerManagerPrefab, typeof(PowerManager));
            
            // 昼夜系统
            CreateManagerIfNeeded(DayNightCyclePrefab, typeof(DayNightCycle));
            CreateManagerIfNeeded(WaveManagerPrefab, typeof(WaveManager));
            
            // 科技系统
            CreateManagerIfNeeded(TechManagerPrefab, typeof(TechManager));
            
            // UI系统
            CreateManagerIfNeeded(UIManagerPrefab, typeof(GameUIManager));
        }

        private void CreateManagerIfNeeded(GameObject prefab, System.Type type)
        {
            if (prefab == null)
            {
                GameObject managerObj = new GameObject(type.Name);
                managerObj.AddComponent(type);
                DontDestroyOnLoad(managerObj);
                Debug.Log($"Created {type.Name} manager (no prefab)");
            }
            else if (FindFirstObjectByType(type) == null)
            {
                GameObject managerObj = Instantiate(prefab);
                DontDestroyOnLoad(managerObj);
                Debug.Log($"Created {type.Name} manager from prefab");
            }
        }

        private void CreateCommandCenter()
        {
            // 查找或创建指挥中心卡牌
            CardData commandCenterCard = CardManager.Instance?.GetCardData("card_command_center");
            
            if (commandCenterCard != null)
            {
                // 首先添加指挥中心卡牌到库存
                CardManager.Instance?.AddCard("card_command_center", 1);
                
                // 然后放置指挥中心
                bool placed = CardManager.Instance?.UseCard("card_command_center", CommandCenterPosition) ?? false;
                
                if (placed)
                {
                    Debug.Log("Command center placed successfully");
                }
                else
                {
                    Debug.LogWarning("Failed to place command center");
                }
            }
            else
            {
                Debug.LogWarning("Command center card not found - make sure data is generated");
            }
        }

        public void OnValidate()
        {
            // 在Inspector中检查引用
        }
    }
}
