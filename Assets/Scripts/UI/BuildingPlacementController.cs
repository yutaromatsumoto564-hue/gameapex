using UnityEngine;
using ARIA.Card;
using ARIA.Building;

namespace ARIA.UI
{
    public class BuildingPlacementController : MonoBehaviour
    {
        public static BuildingPlacementController Instance { get; private set; }

        [Header("Preview Settings")]
        public Color ValidColor = new Color(0f, 1f, 0f, 0.5f);
        public Color InvalidColor = new Color(1f, 0f, 0f, 0.5f);

        private GameObject previewObject;
        private SpriteRenderer previewRenderer;
        private CardData draggingCard;
        private Vector2Int currentGridPos;
        private bool canPlaceCurrent;

        private void Awake()
        {
            Instance = this;
        }

        public void StartDragPreview(CardData cardData)
        {
            draggingCard = cardData;

            if (previewObject == null)
            {
                previewObject = new GameObject("BuildingPreview");
                previewRenderer = previewObject.AddComponent<SpriteRenderer>();
                previewRenderer.sortingOrder = 9999;
                Debug.Log("成功创建了虚影 GameObject！");
            }

            if (cardData.Icon == null)
            {
                Debug.LogWarning($"【注意】卡牌 {cardData.CardName} 没有配置 Icon，使用默认方块作为预览！");
                previewRenderer.sprite = CreateDefaultSprite();
            }
            else
            {
                previewRenderer.sprite = cardData.Icon; 
            }
            
            float scale = Mathf.Max(cardData.BuildingSizeX, cardData.BuildingSizeY) * 1.0f;
            previewObject.transform.localScale = new Vector3(scale, scale, 1f);
            Debug.Log($"设置预览大小: 建筑尺寸 {cardData.BuildingSizeX}x{cardData.BuildingSizeY}, 缩放 {scale}x");
            
            previewObject.SetActive(true);
            
            Debug.Log($"开始拖拽建筑: {cardData.CardName}");
        }

        public void UpdateDragPreview(Vector2 screenPosition)
        {
            if (draggingCard == null || previewObject == null) return;

            Vector3 mousePos = new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(Camera.main.transform.position.z));
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            
            currentGridPos = BuildingManager.Instance.GetGridPosition(worldPos);
            Vector2 snappedWorldPos = BuildingManager.Instance.GetWorldPosition(currentGridPos);
            
            previewObject.transform.position = new Vector3(snappedWorldPos.x, snappedWorldPos.y, 0f);

            canPlaceCurrent = BuildingManager.Instance.CanPlaceBuilding(draggingCard, currentGridPos);
            
            previewRenderer.color = canPlaceCurrent ? new Color(0f, 1f, 0f, 0.8f) : new Color(1f, 0f, 0f, 0.8f);
            
            Debug.Log($"拖拽位置 - 屏幕: {screenPosition}, 世界: {worldPos}, 网格: {currentGridPos}, 可放置: {canPlaceCurrent}");
        }

        public bool EndDragPreview()
        {
            if (draggingCard == null || previewObject == null) return false;

            previewObject.SetActive(false);

            if (canPlaceCurrent)
            {
                return BuildingManager.Instance.PlaceBuilding(draggingCard, currentGridPos);
            }

            return false;
        }

        private Sprite CreateDefaultSprite()
        {
            Texture2D texture = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color(0.5f, 0.8f, 1.0f); // 蓝色方块
            }
            texture.SetPixels(pixels);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        }
    }
}