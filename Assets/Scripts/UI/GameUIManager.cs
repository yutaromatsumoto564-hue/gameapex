using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ARIA.Resource;
using ARIA.Card;
using ARIA.Power;
using ARIA.Core;
using ARIA.DayNight;

namespace ARIA.UI
{
    public class GameUIManager : MonoBehaviour
    {
        public static GameUIManager Instance { get; private set; }

        [Header("Top Bar")]
        public GameObject TopBarPanel;
        public Transform ResourceContainer;
        public GameObject ResourceItemPrefab;
        public Text DayText;
        public Text TimeText;
        public Image DayNightIcon;
        public Sprite DayIcon;
        public Sprite NightIcon;

        [Header("Power Display")]
        public Text PowerText;
        public Image PowerIcon;
        public Color PowerPositiveColor = Color.green;
        public Color PowerNegativeColor = Color.red;

        [Header("Bottom Bar")]
        public GameObject CardBarPanel;
        public Transform CardSlotContainer;
        public GameObject CardSlotPrefab;
        public int MaxVisibleCards = 8;

        [Header("Wave Info")]
        public GameObject WavePanel;
        public Text WaveText;
        public Slider WaveProgressSlider;

        [Header("Building Menu")]
        public GameObject BuildingMenuPanel;
        public Transform BuildingButtonContainer;
        public GameObject BuildingButtonPrefab;

        [Header("Tech Panel")]
        public GameObject TechPanel;

        [Header("Tooltip")]
        public GameObject TooltipPanel;
        public Text TooltipTitle;
        public Text TooltipDescription;

        private Dictionary<string, ResourceItemUI> resourceItems = new Dictionary<string, ResourceItemUI>();
        private List<CardSlotUI> cardSlots = new List<CardSlotUI>();
        private bool isBuildingMenuOpen = false;
        private bool isTechPanelOpen = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // 延迟一帧初始化，确保所有Manager都准备好
            Invoke(nameof(DelayedInitialize), 0.1f);
        }
        
        private void DelayedInitialize()
        {
            InitializeResourceDisplay();
            InitializeCardSlots();
            InitializeBuildingMenu();
            
            // 绑定功能按钮事件
            BindFunctionButtons();
            
            // 订阅资源变化事件
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourcesUpdated += UpdateResourceDisplay;
                Debug.Log("[GameUIManager] 已订阅资源变化事件");
            }
            
            // 初始更新
            UpdateAllDisplays();
        }
        
        private void OnDestroy()
        {
            // 取消订阅资源变化事件
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourcesUpdated -= UpdateResourceDisplay;
            }
        }
        
        private void BindFunctionButtons()
        {
            // 查找功能按钮并绑定事件
            GameObject buildingBtn = GameObject.Find("建筑Button");
            if (buildingBtn != null)
            {
                Button btn = buildingBtn.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => {
                        Debug.LogError("[运行时绑定] 建筑按钮被点击!");
                        ToggleBuildingMenu();
                    });
                    Debug.Log("[GameUIManager] 建筑按钮事件绑定成功");
                }
            }
            else
            {
                Debug.LogWarning("[GameUIManager] 找不到建筑按钮");
            }
        }

        private void Update()
        {
            // 每帧更新时间和波次信息
            UpdateTimeDisplay();
            UpdateWaveDisplay();
            
            // 测试：按B键打开建筑菜单
            if (Input.GetKeyDown(KeyCode.B))
            {
                Debug.LogError("[键盘测试] 按下了B键，尝试打开建筑菜单");
                ToggleBuildingMenu();
            }
            
            // 测试：按N键切换到夜晚（触发敌人波次）
            if (Input.GetKeyDown(KeyCode.N))
            {
                Debug.LogError("[键盘测试] 按下了N键，切换到夜晚");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ForceNight();
                }
                // 直接调用 WaveManager 开始波次
                if (DayNight.WaveManager.Instance != null)
                {
                    Debug.LogError("[键盘测试] 直接调用 WaveManager.StartNightWaves()");
                    DayNight.WaveManager.Instance.StartNightWaves();
                }
            }
        }

        #region Resource Display

        private void InitializeResourceDisplay()
        {
            if (ResourceManager.Instance == null)
            {
                Debug.LogWarning("ResourceManager.Instance is null");
                return;
            }
            
            if (ResourceItemPrefab == null)
            {
                Debug.LogWarning("ResourceItemPrefab is null");
                return;
            }
            
            if (ResourceContainer == null)
            {
                Debug.LogWarning("ResourceContainer is null");
                return;
            }

            // 获取所有资源数据（包括数量为0的）
            var allResourceData = ResourceManager.Instance.AllResources;
            Debug.Log($"Initializing resource display with {allResourceData?.Count ?? 0} resources");
            
            if (allResourceData == null || allResourceData.Count == 0)
            {
                Debug.LogWarning("No resources found in ResourceManager");
                return;
            }
            
            foreach (var resource in allResourceData)
            {
                if (resource != null)
                {
                    CreateResourceItem(resource.ResourceId, resource.ResourceName, resource.Icon);
                }
            }
        }

        private void CreateResourceItem(string resourceId, string name, Sprite icon)
        {
            if (resourceItems.ContainsKey(resourceId)) return;
            if (ResourceItemPrefab == null || ResourceContainer == null) return;

            GameObject itemObj = Instantiate(ResourceItemPrefab, ResourceContainer);
            itemObj.SetActive(true); // 激活对象
            
            // 强制设置位置和大小
            RectTransform rt = itemObj.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.localScale = Vector3.one;
                rt.localPosition = Vector3.zero;
                rt.anchoredPosition = Vector2.zero;
            }
            
            ResourceItemUI itemUI = itemObj.GetComponent<ResourceItemUI>();
            if (itemUI == null)
            {
                itemUI = itemObj.AddComponent<ResourceItemUI>();
            }
            
            itemUI.Setup(resourceId, name, icon);
            resourceItems[resourceId] = itemUI;
            
            // 初始数量
            int amount = ResourceManager.Instance?.GetResourceAmount(resourceId) ?? 0;
            itemUI.UpdateAmount(amount);
            
            Debug.Log($"Created resource item: {name} ({resourceId})");
        }

        #endregion

        #region Time Display

        private void UpdateTimeDisplay()
        {
            if (GameManager.Instance == null) return;

            if (DayText != null)
            {
                DayText.text = $"Day {GameManager.Instance.CurrentDay}";
            }

            if (TimeText != null)
            {
                float dayProgress = GameManager.Instance.CurrentTime / GameManager.Instance.GetCurrentStateDuration();
                int hour = Mathf.FloorToInt(dayProgress * 24f);
                int minute = Mathf.FloorToInt((dayProgress * 24f - hour) * 60);
                TimeText.text = $"{hour:00}:{minute:00}";
            }

            if (DayNightIcon != null)
            {
                // 根据GameState判断
                DayNightIcon.sprite = IsDayTime() ? DayIcon : NightIcon;
            }
        }
        
        private bool IsDayTime()
        {
            if (DayNightCycle.Instance == null) return true;
            return DayNightCycle.Instance.IsDay();
        }

        #endregion

        #region Power Display

        private void UpdatePowerDisplay()
        {
            if (PowerManager.Instance == null) return;
            
            int generation = PowerManager.Instance.TotalGeneration;
            int consumption = PowerManager.Instance.TotalConsumption;
            
            if (PowerText != null)
            {
                int surplus = generation - consumption;
                PowerText.text = $"{surplus:+#;-#;0}";
                PowerText.color = surplus >= 0 ? PowerPositiveColor : PowerNegativeColor;
            }

            if (PowerIcon != null)
            {
                PowerIcon.color = (generation >= consumption) ? PowerPositiveColor : PowerNegativeColor;
            }
        }

        #endregion

        #region Card Display

        private void InitializeCardSlots()
        {
            if (CardSlotPrefab == null || CardSlotContainer == null) return;
            
            for (int i = 0; i < MaxVisibleCards; i++)
            {
                GameObject slotObj = Instantiate(CardSlotPrefab, CardSlotContainer);
                CardSlotUI slotUI = slotObj.GetComponent<CardSlotUI>();
                if (slotUI == null)
                {
                    slotUI = slotObj.AddComponent<CardSlotUI>();
                }
                slotUI.SetIndex(i);
                cardSlots.Add(slotUI);
            }
        }

        private void UpdateCardDisplay()
        {
            var cards = CardManager.Instance?.GetAllCards();
            if (cards == null) return;

            for (int i = 0; i < cardSlots.Count; i++)
            {
                if (i < cards.Count)
                {
                    cardSlots[i].SetCard(cards[i].Card, cards[i].Amount);
                }
                else
                {
                    cardSlots[i].ClearSlot();
                }
            }
        }

        #endregion

        #region Wave Display

        private void UpdateWaveDisplay()
        {
            if (WaveManager.Instance == null) return;
            
            bool isWaveActive = WaveManager.Instance.IsWaveActive;
            
            if (WavePanel != null)
            {
                WavePanel.SetActive(isWaveActive);
            }
            
            if (!isWaveActive) return;
            
            if (WaveText != null)
            {
                WaveText.text = $"Wave {WaveManager.Instance.CurrentDay}";
            }
            
            if (WaveProgressSlider != null)
            {
                WaveProgressSlider.value = WaveManager.Instance.GetWaveProgress();
            }
        }

        #endregion

        #region Building Menu

        private void InitializeBuildingMenu()
        {
            if (BuildingMenuPanel != null)
            {
                BuildingMenuPanel.SetActive(false);
                
                // 检查按钮数量
                int buttonCount = BuildingMenuPanel.transform.GetComponentsInChildren<Button>(true).Length;
                Debug.Log($"[GameUIManager] BuildingMenu has {buttonCount} buttons");
            }
            else
            {
                Debug.LogWarning("[GameUIManager] BuildingMenuPanel is null in InitializeBuildingMenu!");
            }
        }

        public void ToggleBuildingMenu()
        {
            isBuildingMenuOpen = !isBuildingMenuOpen;
            Debug.Log($"[GameUIManager] ToggleBuildingMenu called, new state: {isBuildingMenuOpen}");
            
            if (BuildingMenuPanel != null)
            {
                BuildingMenuPanel.SetActive(isBuildingMenuOpen);
                Debug.Log($"[GameUIManager] BuildingMenuPanel set to: {isBuildingMenuOpen}, actual state: {BuildingMenuPanel.activeSelf}");
                
                // 检查按钮
                var buttons = BuildingMenuPanel.GetComponentsInChildren<UnityEngine.UI.Button>(true);
                Debug.Log($"[GameUIManager] Found {buttons.Length} buttons in BuildingMenuPanel");
            }
            else
            {
                Debug.LogError("[GameUIManager] BuildingMenuPanel is null! Cannot toggle menu.");
            }
        }

        public void CloseBuildingMenu()
        {
            isBuildingMenuOpen = false;
            if (BuildingMenuPanel != null)
            {
                BuildingMenuPanel.SetActive(false);
            }
        }

        #endregion

        #region Tech Panel

        public void ToggleTechPanel()
        {
            isTechPanelOpen = !isTechPanelOpen;
            if (TechPanel != null)
            {
                TechPanel.SetActive(isTechPanelOpen);
            }
        }

        public void CloseTechPanel()
        {
            isTechPanelOpen = false;
            if (TechPanel != null)
            {
                TechPanel.SetActive(false);
            }
        }

        #endregion

        #region Tooltip

        public void ShowTooltip(string title, string description, Vector2 position)
        {
            if (TooltipPanel == null) return;

            TooltipPanel.SetActive(true);
            
            if (TooltipTitle != null)
                TooltipTitle.text = title;
            
            if (TooltipDescription != null)
                TooltipDescription.text = description;

            RectTransform rt = TooltipPanel.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.position = position;
            }
        }

        public void HideTooltip()
        {
            if (TooltipPanel != null)
            {
                TooltipPanel.SetActive(false);
            }
        }

        #endregion

        #region Utility

        private void UpdateAllDisplays()
        {
            UpdateResourceDisplay();
            UpdateTimeDisplay();
            UpdatePowerDisplay();
            UpdateCardDisplay();
            UpdateWaveDisplay();
        }
        
        private void UpdateResourceDisplay()
        {
            if (ResourceManager.Instance == null) return;
            
            foreach (var item in resourceItems)
            {
                string resourceId = item.Key;
                ResourceItemUI ui = item.Value;
                
                int amount = ResourceManager.Instance.GetResourceAmount(resourceId);
                ui.UpdateAmount(amount);
            }
        }

        public void ShowMessage(string message, float duration = 2f)
        {
            Debug.Log($"[UI Message] {message}");
        }

        #endregion
    }
}
