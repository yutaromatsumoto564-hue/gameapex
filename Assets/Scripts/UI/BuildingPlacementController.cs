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
            }

            if (cardData.Icon == null)
            {
                previewRenderer.sprite = CreateDefaultSprite();
            }
            else
            {
                previewRenderer.sprite = cardData.Icon;
            }
            
            // 精确缩放：按照建筑实际的 X 和 Y 占地面积进行拉伸，确保虚影大小绝对正确
            previewObject.transform.localScale = new Vector3(cardData.BuildingSizeX, cardData.BuildingSizeY, 1f);
            
            // 为了让预览图有一点半透明效果，重置默认颜色
            previewRenderer.color = new Color(1f, 1f, 1f, 0.5f);
            
            previewObject.SetActive(true);
        }

        public void UpdateDragPreview(Vector2 screenPosition)
        {
            if (draggingCard == null || previewObject == null) return;

            // 1. 获取鼠标真实的 2D 世界坐标
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(Camera.main.transform.position.z)));
            Vector2 mouseWorldPos2D = new Vector2(mousePos.x, mousePos.y);
            
            int sizeX = draggingCard.BuildingSizeX;
            int sizeY = draggingCard.BuildingSizeY;

            // 2. 核心：鼠标代表建筑中心。计算出如果以此为中心，建筑左下角所在的网格坐标 (GridPos)
            currentGridPos = BuildingManager.Instance.GetGridPositionFromCenter(mouseWorldPos2D, sizeX, sizeY);
            
            // 3. 将计算出的网格坐标反推回中心世界坐标，实现严格吸附 (Snapping)
            Vector2 snappedCenterPos = BuildingManager.Instance.GetWorldPosition(currentGridPos, sizeX, sizeY);
            
            // 4. 更新虚影位置
            previewObject.transform.position = new Vector3(snappedCenterPos.x, snappedCenterPos.y, 0f);

            // 5. 判断该位置是否合法并改变颜色
            canPlaceCurrent = BuildingManager.Instance.CanPlaceBuilding(draggingCard, currentGridPos);
            previewRenderer.color = canPlaceCurrent ? ValidColor : InvalidColor;
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