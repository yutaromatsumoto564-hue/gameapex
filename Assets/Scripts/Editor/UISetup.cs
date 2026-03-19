using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using ARIA.UI;

namespace ARIA.Editor
{
    public class UISetup
    {
        [MenuItem("ARIA/Setup Game UI")]
        public static void SetupGameUI()
        {
            // 查找或创建Canvas
            // 创建Event System（如果没有）
            GameObject eventSystem = GameObject.Find("EventSystem");
            if (eventSystem == null)
            {
                eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("[UISetup] 创建EventSystem");
            }

            GameObject canvasObj = GameObject.Find("GameCanvas");
            if (canvasObj == null)
            {
                canvasObj = new GameObject("GameCanvas");
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // 添加GameUIManager
            GameUIManager uiManager = canvasObj.GetComponent<GameUIManager>();
            if (uiManager == null)
            {
                uiManager = canvasObj.AddComponent<GameUIManager>();
            }

            // 创建顶部资源栏
            CreateTopBar(canvasObj.transform, uiManager);

            // 创建底部卡牌栏
            CreateCardBar(canvasObj.transform, uiManager);

            // 创建波次信息面板
            CreateWavePanel(canvasObj.transform, uiManager);

            // 创建提示面板
            CreateTooltipPanel(canvasObj.transform, uiManager);

            // 创建建筑菜单
            CreateBuildingMenu(canvasObj.transform, uiManager);

            // 创建科技面板
            CreateTechPanel(canvasObj.transform, uiManager);

            // 创建功能按钮
            CreateFunctionButtons(canvasObj.transform, uiManager);

            EditorUtility.SetDirty(canvasObj);
            Debug.Log("游戏UI设置完成！");
        }

        private static void CreateTopBar(Transform canvasTransform, GameUIManager uiManager)
        {
            // 创建顶部栏 - 半透明深色背景
            GameObject topBar = new GameObject("TopBar");
            topBar.transform.SetParent(canvasTransform, false);

            RectTransform topBarRT = topBar.AddComponent<RectTransform>();
            topBarRT.anchorMin = new Vector2(0, 1);
            topBarRT.anchorMax = new Vector2(1, 1);
            topBarRT.pivot = new Vector2(0.5f, 1);
            topBarRT.sizeDelta = new Vector2(0, 70);
            topBarRT.anchoredPosition = new Vector2(0, 0);

            Image topBarImage = topBar.AddComponent<Image>();
            topBarImage.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);

            uiManager.TopBarPanel = topBar;

            // 创建资源容器
            GameObject resourceContainer = new GameObject("ResourceContainer");
            resourceContainer.transform.SetParent(topBar.transform, false);

            RectTransform resourceRT = resourceContainer.AddComponent<RectTransform>();
            resourceRT.anchorMin = new Vector2(0, 0);
            resourceRT.anchorMax = new Vector2(0.5f, 1);
            resourceRT.pivot = new Vector2(0, 0.5f);
            resourceRT.sizeDelta = new Vector2(-520, -10);
            resourceRT.anchoredPosition = new Vector2(10, 0);

            HorizontalLayoutGroup resourceLayout = resourceContainer.AddComponent<HorizontalLayoutGroup>();
            resourceLayout.spacing = 5;
            resourceLayout.padding = new RectOffset(10, 10, 10, 10);
            resourceLayout.childAlignment = TextAnchor.MiddleLeft;
            resourceLayout.childControlWidth = false;
            resourceLayout.childControlHeight = false;
            resourceLayout.childForceExpandWidth = false;
            resourceLayout.childForceExpandHeight = false;

            uiManager.ResourceContainer = resourceContainer.transform;

            // 创建资源项预制体
            GameObject resourceItemPrefab = CreateResourceItemPrefab();
            uiManager.ResourceItemPrefab = resourceItemPrefab;

            // 创建时间和天数显示
            CreateTimeDisplay(topBar.transform, uiManager);

            // 创建电力显示
            CreatePowerDisplay(topBar.transform, uiManager);
        }

        private static GameObject CreateResourceItemPrefab()
        {
            GameObject prefab = new GameObject("ResourceItem");

            RectTransform rt = prefab.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(90, 45);

            // 背景
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(prefab.transform, false);
            RectTransform bgRT = bgObj.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.pivot = new Vector2(0.5f, 0.5f);
            bgRT.sizeDelta = Vector2.zero;

            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.3f, 0.3f, 0.4f, 0.9f);

            // 图标
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(prefab.transform, false);
            RectTransform iconRT = iconObj.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0, 0.5f);
            iconRT.anchorMax = new Vector2(0, 0.5f);
            iconRT.pivot = new Vector2(0, 0.5f);
            iconRT.sizeDelta = new Vector2(32, 32);
            iconRT.anchoredPosition = new Vector2(5, 0);

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = new Color(0.8f, 0.8f, 0.8f);

            // 数量文本
            GameObject amountObj = new GameObject("Amount");
            amountObj.transform.SetParent(prefab.transform, false);
            RectTransform amountRT = amountObj.AddComponent<RectTransform>();
            amountRT.anchorMin = new Vector2(0, 0);
            amountRT.anchorMax = new Vector2(1, 0.55f);
            amountRT.pivot = new Vector2(0.5f, 0);
            amountRT.sizeDelta = new Vector2(-42, 0);
            amountRT.anchoredPosition = new Vector2(21, 2);

            Text amountText = amountObj.AddComponent<Text>();
            amountText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            amountText.fontSize = 18;
            amountText.fontStyle = FontStyle.Bold;
            amountText.color = Color.white;
            amountText.alignment = TextAnchor.LowerLeft;

            // 名称文本
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(prefab.transform, false);
            RectTransform nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.5f);
            nameRT.anchorMax = new Vector2(1, 1);
            nameRT.pivot = new Vector2(0.5f, 1);
            nameRT.sizeDelta = new Vector2(-42, 0);
            nameRT.anchoredPosition = new Vector2(21, -2);

            Text nameText = nameObj.AddComponent<Text>();
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.fontSize = 12;
            nameText.color = new Color(0.7f, 0.7f, 0.7f);
            nameText.alignment = TextAnchor.UpperLeft;

            // 添加ResourceItemUI组件
            ResourceItemUI itemUI = prefab.AddComponent<ResourceItemUI>();
            itemUI.IconImage = iconImage;
            itemUI.AmountText = amountText;
            itemUI.NameText = nameText;

            prefab.SetActive(false);
            return prefab;
        }

        private static void CreateTimeDisplay(Transform parent, GameUIManager uiManager)
        {
            GameObject timeObj = new GameObject("TimeDisplay");
            timeObj.transform.SetParent(parent, false);

            RectTransform rt = timeObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.52f, 0);
            rt.anchorMax = new Vector2(0.65f, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = Vector2.zero;

            // 背景
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(timeObj.transform, false);
            RectTransform bgRT = bgObj.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.pivot = new Vector2(0.5f, 0.5f);
            bgRT.sizeDelta = new Vector2(-10, -10);

            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.15f, 0.15f, 0.2f, 0.8f);

            // 天数文本
            GameObject dayObj = new GameObject("DayText");
            dayObj.transform.SetParent(timeObj.transform, false);
            RectTransform dayRT = dayObj.AddComponent<RectTransform>();
            dayRT.anchorMin = new Vector2(0, 0.55f);
            dayRT.anchorMax = new Vector2(1, 1);
            dayRT.pivot = new Vector2(0.5f, 0.5f);
            dayRT.sizeDelta = Vector2.zero;

            Text dayText = dayObj.AddComponent<Text>();
            dayText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            dayText.fontSize = 14;
            dayText.color = new Color(0.8f, 0.8f, 0.8f);
            dayText.alignment = TextAnchor.MiddleCenter;
            dayText.text = "Day 1";

            uiManager.DayText = dayText;

            // 时间文本
            GameObject timeTextObj = new GameObject("TimeText");
            timeTextObj.transform.SetParent(timeObj.transform, false);
            RectTransform timeRT = timeTextObj.AddComponent<RectTransform>();
            timeRT.anchorMin = new Vector2(0, 0);
            timeRT.anchorMax = new Vector2(0.7f, 0.55f);
            timeRT.pivot = new Vector2(0.5f, 0.5f);
            timeRT.sizeDelta = Vector2.zero;

            Text timeText = timeTextObj.AddComponent<Text>();
            timeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            timeText.fontSize = 22;
            timeText.fontStyle = FontStyle.Bold;
            timeText.color = new Color(1f, 0.9f, 0.4f);
            timeText.alignment = TextAnchor.MiddleCenter;
            timeText.text = "08:00";

            uiManager.TimeText = timeText;

            // 昼夜图标
            GameObject iconObj = new GameObject("DayNightIcon");
            iconObj.transform.SetParent(timeObj.transform, false);
            RectTransform iconRT = iconObj.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.7f, 0.1f);
            iconRT.anchorMax = new Vector2(0.95f, 0.5f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.sizeDelta = Vector2.zero;

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = new Color(1f, 0.9f, 0.3f);

            uiManager.DayNightIcon = iconImage;
        }

        private static void CreatePowerDisplay(Transform parent, GameUIManager uiManager)
        {
            GameObject powerObj = new GameObject("PowerDisplay");
            powerObj.transform.SetParent(parent, false);

            RectTransform rt = powerObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.66f, 0);
            rt.anchorMax = new Vector2(0.78f, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = Vector2.zero;

            // 背景
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(powerObj.transform, false);
            RectTransform bgRT = bgObj.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.pivot = new Vector2(0.5f, 0.5f);
            bgRT.sizeDelta = new Vector2(-10, -10);

            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.15f, 0.15f, 0.2f, 0.8f);

            // 电力图标
            GameObject iconObj = new GameObject("PowerIcon");
            iconObj.transform.SetParent(powerObj.transform, false);
            RectTransform iconRT = iconObj.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.05f, 0.25f);
            iconRT.anchorMax = new Vector2(0.35f, 0.75f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.sizeDelta = Vector2.zero;

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = new Color(0.3f, 1f, 0.3f);

            uiManager.PowerIcon = iconImage;

            // 电力文本
            GameObject textObj = new GameObject("PowerText");
            textObj.transform.SetParent(powerObj.transform, false);
            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.35f, 0);
            textRT.anchorMax = new Vector2(1, 1);
            textRT.pivot = new Vector2(0.5f, 0.5f);
            textRT.sizeDelta = Vector2.zero;

            Text powerText = textObj.AddComponent<Text>();
            powerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            powerText.fontSize = 20;
            powerText.fontStyle = FontStyle.Bold;
            powerText.color = new Color(0.3f, 1f, 0.3f);
            powerText.alignment = TextAnchor.MiddleCenter;
            powerText.text = "+0";

            uiManager.PowerText = powerText;
        }

        private static void CreateCardBar(Transform canvasTransform, GameUIManager uiManager)
        {
            // 创建卡牌栏
            GameObject cardBar = new GameObject("CardBar");
            cardBar.transform.SetParent(canvasTransform, false);

            RectTransform rt = cardBar.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(850, 130);
            rt.anchoredPosition = new Vector2(0, 15);

            Image bg = cardBar.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);

            uiManager.CardBarPanel = cardBar;

            // 创建卡牌槽容器
            GameObject slotContainer = new GameObject("CardSlotContainer");
            slotContainer.transform.SetParent(cardBar.transform, false);

            RectTransform slotRT = slotContainer.AddComponent<RectTransform>();
            slotRT.anchorMin = Vector2.zero;
            slotRT.anchorMax = Vector2.one;
            slotRT.pivot = new Vector2(0.5f, 0.5f);
            slotRT.sizeDelta = new Vector2(-20, -20);
            slotRT.anchoredPosition = Vector2.zero;

            HorizontalLayoutGroup layout = slotContainer.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8;
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            uiManager.CardSlotContainer = slotContainer.transform;

            // 创建卡牌槽预制体
            GameObject cardSlotPrefab = CreateCardSlotPrefab();
            uiManager.CardSlotPrefab = cardSlotPrefab;
        }

        private static GameObject CreateCardSlotPrefab()
        {
            GameObject prefab = new GameObject("CardSlot");

            RectTransform rt = prefab.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(90, 110);

            // 背景
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(prefab.transform, false);
            RectTransform bgRT = bgObj.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.pivot = new Vector2(0.5f, 0.5f);
            bgRT.sizeDelta = Vector2.zero;

            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.25f, 0.95f);

            // 边框（稀有度）
            GameObject borderObj = new GameObject("RarityBorder");
            borderObj.transform.SetParent(prefab.transform, false);
            RectTransform borderRT = borderObj.AddComponent<RectTransform>();
            borderRT.anchorMin = Vector2.zero;
            borderRT.anchorMax = Vector2.one;
            borderRT.pivot = new Vector2(0.5f, 0.5f);
            borderRT.sizeDelta = new Vector2(-2, -2);

            Image borderImage = borderObj.AddComponent<Image>();
            borderImage.color = Color.gray;

            // 卡牌图标
            GameObject iconObj = new GameObject("CardIcon");
            iconObj.transform.SetParent(prefab.transform, false);
            RectTransform iconRT = iconObj.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.1f, 0.3f);
            iconRT.anchorMax = new Vector2(0.9f, 0.85f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.sizeDelta = Vector2.zero;

            Image iconImage = iconObj.AddComponent<Image>();

            // 数量文本
            GameObject amountObj = new GameObject("Amount");
            amountObj.transform.SetParent(prefab.transform, false);
            RectTransform amountRT = amountObj.AddComponent<RectTransform>();
            amountRT.anchorMin = new Vector2(0.6f, 0.75f);
            amountRT.anchorMax = new Vector2(0.95f, 0.95f);
            amountRT.pivot = new Vector2(1, 1);
            amountRT.sizeDelta = Vector2.zero;

            Text amountText = amountObj.AddComponent<Text>();
            amountText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            amountText.fontSize = 16;
            amountText.fontStyle = FontStyle.Bold;
            amountText.color = Color.white;
            amountText.alignment = TextAnchor.MiddleRight;

            // 名称文本
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(prefab.transform, false);
            RectTransform nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0.05f, 0.05f);
            nameRT.anchorMax = new Vector2(0.95f, 0.25f);
            nameRT.pivot = new Vector2(0.5f, 0.5f);
            nameRT.sizeDelta = Vector2.zero;

            Text nameText = nameObj.AddComponent<Text>();
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.fontSize = 11;
            nameText.color = Color.white;
            nameText.alignment = TextAnchor.MiddleCenter;

            // 选中指示器
            GameObject selectedObj = new GameObject("SelectedIndicator");
            selectedObj.transform.SetParent(prefab.transform, false);
            RectTransform selectedRT = selectedObj.AddComponent<RectTransform>();
            selectedRT.anchorMin = Vector2.zero;
            selectedRT.anchorMax = Vector2.one;
            selectedRT.pivot = new Vector2(0.5f, 0.5f);
            selectedRT.sizeDelta = new Vector2(4, 4);

            Image selectedImage = selectedObj.AddComponent<Image>();
            selectedImage.color = new Color(1f, 0.9f, 0.2f);
            selectedObj.SetActive(false);

            // 添加CardSlotUI组件
            CardSlotUI slotUI = prefab.AddComponent<CardSlotUI>();
            slotUI.CardImage = iconImage;
            slotUI.AmountText = amountText;
            slotUI.NameText = nameText;
            slotUI.RarityBorder = borderImage;
            slotUI.SelectedIndicator = selectedObj;

            prefab.SetActive(false);
            return prefab;
        }

        private static void CreateWavePanel(Transform canvasTransform, GameUIManager uiManager)
        {
            GameObject wavePanel = new GameObject("WavePanel");
            wavePanel.transform.SetParent(canvasTransform, false);

            RectTransform rt = wavePanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.sizeDelta = new Vector2(220, 90);
            rt.anchoredPosition = new Vector2(-15, -85);

            Image bg = wavePanel.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);

            uiManager.WavePanel = wavePanel;

            // 波次标题
            GameObject titleObj = new GameObject("WaveTitle");
            titleObj.transform.SetParent(wavePanel.transform, false);
            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.6f);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 0.5f);
            titleRT.sizeDelta = Vector2.zero;

            Text titleText = titleObj.AddComponent<Text>();
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 18;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = new Color(1f, 0.4f, 0.4f);
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.text = "Wave 1";

            uiManager.WaveText = titleText;

            // 敌人数量
            GameObject countObj = new GameObject("EnemyCount");
            countObj.transform.SetParent(wavePanel.transform, false);
            RectTransform countRT = countObj.AddComponent<RectTransform>();
            countRT.anchorMin = new Vector2(0, 0.3f);
            countRT.anchorMax = new Vector2(1, 0.6f);
            countRT.pivot = new Vector2(0.5f, 0.5f);
            countRT.sizeDelta = Vector2.zero;

            Text countText = countObj.AddComponent<Text>();
            countText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            countText.fontSize = 14;
            countText.color = Color.white;
            countText.alignment = TextAnchor.MiddleCenter;
            countText.text = "Enemies: 0";

            // 进度条
            GameObject sliderObj = new GameObject("WaveProgress");
            sliderObj.transform.SetParent(wavePanel.transform, false);
            RectTransform sliderRT = sliderObj.AddComponent<RectTransform>();
            sliderRT.anchorMin = new Vector2(0.1f, 0.1f);
            sliderRT.anchorMax = new Vector2(0.9f, 0.25f);
            sliderRT.pivot = new Vector2(0.5f, 0.5f);
            sliderRT.sizeDelta = Vector2.zero;

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.value = 0;

            // 进度条背景
            GameObject sliderBg = new GameObject("Background");
            sliderBg.transform.SetParent(sliderObj.transform, false);
            RectTransform bgRT = sliderBg.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.pivot = new Vector2(0.5f, 0.5f);
            bgRT.sizeDelta = Vector2.zero;

            Image sliderBgImage = sliderBg.AddComponent<Image>();
            sliderBgImage.color = new Color(0.2f, 0.2f, 0.2f);

            // 进度条填充
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(sliderObj.transform, false);
            RectTransform fillRT = fillObj.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.pivot = new Vector2(0, 0.5f);
            fillRT.sizeDelta = Vector2.zero;

            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.color = new Color(1f, 0.4f, 0.4f);

            slider.fillRect = fillRT;
            slider.targetGraphic = fillImage;

            uiManager.WaveProgressSlider = slider;

            wavePanel.SetActive(false);
        }

        private static void CreateTooltipPanel(Transform canvasTransform, GameUIManager uiManager)
        {
            GameObject tooltip = new GameObject("Tooltip");
            tooltip.transform.SetParent(canvasTransform, false);

            RectTransform rt = tooltip.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(250, 120);
            rt.pivot = new Vector2(0.5f, 0);

            Image bg = tooltip.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.15f, 0.98f);

            // 标题
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(tooltip.transform, false);
            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.6f);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 0.5f);
            titleRT.sizeDelta = new Vector2(-20, 0);

            Text titleText = titleObj.AddComponent<Text>();
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 16;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.MiddleLeft;

            uiManager.TooltipTitle = titleText;

            // 描述
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(tooltip.transform, false);
            RectTransform descRT = descObj.AddComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0, 0);
            descRT.anchorMax = new Vector2(1, 0.6f);
            descRT.pivot = new Vector2(0.5f, 0.5f);
            descRT.sizeDelta = new Vector2(-20, 0);

            Text descText = descObj.AddComponent<Text>();
            descText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            descText.fontSize = 12;
            descText.color = new Color(0.8f, 0.8f, 0.8f);
            descText.alignment = TextAnchor.UpperLeft;

            uiManager.TooltipDescription = descText;

            uiManager.TooltipPanel = tooltip;
            tooltip.SetActive(false);
        }

        private static void CreateBuildingMenu(Transform canvasTransform, GameUIManager uiManager)
        {
            GameObject menu = new GameObject("BuildingMenu");
            menu.transform.SetParent(canvasTransform, false);

            RectTransform rt = menu.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(250, 400);
            rt.anchoredPosition = new Vector2(-300, 0); // 放在屏幕左侧中间

            Image bg = menu.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.12f, 0.98f);
            bg.raycastTarget = true;

            // 添加Canvas Group确保子元素可以交互
            CanvasGroup canvasGroup = menu.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            // 标题
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(menu.transform, false);
            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.9f);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 0.5f);
            titleRT.sizeDelta = Vector2.zero;

            Text titleText = titleObj.AddComponent<Text>();
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 18;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.text = "建筑";

            // 按钮容器
            GameObject buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(menu.transform, false);

            RectTransform containerRT = buttonContainer.AddComponent<RectTransform>();
            containerRT.anchorMin = new Vector2(0, 0);
            containerRT.anchorMax = new Vector2(1, 0.9f);
            containerRT.pivot = new Vector2(0.5f, 0.5f);
            containerRT.sizeDelta = new Vector2(-20, -20);

            VerticalLayoutGroup layout = buttonContainer.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8;
            layout.padding = new RectOffset(5, 5, 5, 5);
            layout.childAlignment = TextAnchor.UpperCenter;

            uiManager.BuildingMenuPanel = menu;
            uiManager.BuildingButtonContainer = buttonContainer.transform;

            // 添加建筑按钮
            CreateBuildingButton(buttonContainer.transform, "采矿机", "采集矿石资源", Color.gray, uiManager);
            CreateBuildingButton(buttonContainer.transform, "发电厂", "产生电力", Color.yellow, uiManager);
            CreateBuildingButton(buttonContainer.transform, "防御塔", "攻击敌人", Color.red, uiManager);
            CreateBuildingButton(buttonContainer.transform, "仓库", "存储资源", Color.blue, uiManager);

            menu.SetActive(false);
        }

        private static void CreateBuildingButton(Transform parent, string name, string description, Color color, GameUIManager uiManager)
        {
            // 使用UnityEditor的API创建按钮
            GameObject buttonObj = new GameObject($"BuildingButton_{name}");
            buttonObj.transform.SetParent(parent, false);
            buttonObj.layer = LayerMask.NameToLayer("UI");

            RectTransform rt = buttonObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160, 50);
            rt.localScale = Vector3.one;

            // 背景图片
            Image bg = buttonObj.AddComponent<Image>();
            bg.color = new Color(0.25f, 0.25f, 0.3f, 1f);
            bg.raycastTarget = true;

            // 按钮组件 - 关键设置
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = bg;
            button.interactable = true;
            
            // 设置颜色过渡
            ColorBlock cb = button.colors;
            cb.normalColor = new Color(1f, 1f, 1f, 1f);
            cb.highlightedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
            cb.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            cb.colorMultiplier = 1f;
            cb.fadeDuration = 0.1f;
            button.colors = cb;

            // 名称文本
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(buttonObj.transform, false);
            RectTransform nameRT = nameObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.5f);
            nameRT.anchorMax = new Vector2(1, 1);
            nameRT.pivot = new Vector2(0.5f, 0.5f);
            nameRT.sizeDelta = new Vector2(-10, -5);
            nameRT.anchoredPosition = Vector2.zero;

            Text nameText = nameObj.AddComponent<Text>();
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.fontSize = 14;
            nameText.fontStyle = FontStyle.Bold;
            nameText.color = color;
            nameText.alignment = TextAnchor.MiddleCenter;
            nameText.text = name;
            nameText.raycastTarget = false;

            // 描述文本
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(buttonObj.transform, false);
            RectTransform descRT = descObj.AddComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0, 0);
            descRT.anchorMax = new Vector2(1, 0.5f);
            descRT.pivot = new Vector2(0.5f, 0.5f);
            descRT.sizeDelta = new Vector2(-10, -5);
            descRT.anchoredPosition = Vector2.zero;

            Text descText = descObj.AddComponent<Text>();
            descText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            descText.fontSize = 10;
            descText.color = new Color(0.7f, 0.7f, 0.7f);
            descText.alignment = TextAnchor.MiddleCenter;
            descText.text = description;
            descText.raycastTarget = false;

            // 点击事件 - 使用UnityEvent
            string buildingName = name;
            button.onClick.AddListener(() => {
                Debug.LogError($"[建筑按钮点击] >>> {buildingName} 被点击了! <<<");
                if (uiManager != null)
                {
                    uiManager.CloseBuildingMenu();
                }
            });
        }

        private static void CreateTechPanel(Transform canvasTransform, GameUIManager uiManager)
        {
            GameObject techPanel = new GameObject("TechPanel");
            techPanel.transform.SetParent(canvasTransform, false);

            RectTransform rt = techPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(600, 450);

            Image bg = techPanel.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.12f, 0.98f);

            // 标题
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(techPanel.transform, false);
            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0.9f);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.pivot = new Vector2(0.5f, 0.5f);
            titleRT.sizeDelta = Vector2.zero;

            Text titleText = titleObj.AddComponent<Text>();
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 24;
            titleText.fontStyle = FontStyle.Bold;
            titleText.color = new Color(0.4f, 0.8f, 1f);
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.text = "科技树";

            uiManager.TechPanel = techPanel;
            techPanel.SetActive(false);
        }

        private static void CreateFunctionButtons(Transform canvasTransform, GameUIManager uiManager)
        {
            // 创建按钮容器
            GameObject buttonContainer = new GameObject("FunctionButtons");
            buttonContainer.transform.SetParent(canvasTransform, false);

            RectTransform rt = buttonContainer.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(1, 0);
            rt.sizeDelta = new Vector2(300, 60);
            rt.anchoredPosition = new Vector2(-15, 15);

            HorizontalLayoutGroup layout = buttonContainer.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleRight;

            // 建筑按钮
            CreateFunctionButton(buttonContainer.transform, "建筑", Color.cyan, () => {
                Debug.Log("[功能按钮] 点击了建筑按钮！");
                uiManager.ToggleBuildingMenu();
            });

            // 科技按钮
            CreateFunctionButton(buttonContainer.transform, "科技", new Color(0.4f, 0.8f, 1f), () => {
                uiManager.ToggleTechPanel();
            });

            // 菜单按钮
            CreateFunctionButton(buttonContainer.transform, "菜单", Color.gray, () => {
                Debug.Log("打开菜单");
            });
        }

        private static void CreateFunctionButton(Transform parent, string text, Color color, System.Action onClick)
        {
            GameObject buttonObj = new GameObject(text + "Button");
            buttonObj.transform.SetParent(parent, false);
            buttonObj.layer = LayerMask.NameToLayer("UI");

            RectTransform rt = buttonObj.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(80, 50);

            Image bg = buttonObj.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);
            bg.raycastTarget = true;

            // 按钮组件
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = bg;
            button.interactable = true;
            
            // 设置颜色
            ColorBlock cb = button.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
            cb.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            button.colors = cb;
            
            // 点击事件
            string btnText = text;
            button.onClick.AddListener(() => {
                Debug.LogError($"[按钮点击] >>> {btnText} 被点击了! <<<");
                onClick?.Invoke();
            });

            // 文字
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.pivot = new Vector2(0.5f, 0.5f);
            textRT.sizeDelta = Vector2.zero;

            Text buttonText = textObj.AddComponent<Text>();
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = 14;
            buttonText.color = color;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.text = text;
            buttonText.raycastTarget = false;
            
            Debug.Log($"[UISetup] 创建功能按钮: {text}");
        }
    }
}
