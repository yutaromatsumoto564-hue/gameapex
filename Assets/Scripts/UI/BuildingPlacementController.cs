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
                previewRenderer.sortingOrder = 100;
            }

            previewRenderer.sprite = cardData.Icon;
            previewObject.SetActive(true);
        }

        public void UpdateDragPreview(Vector2 screenPosition)
        {
            if (draggingCard == null || previewObject == null) return;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -Camera.main.transform.position.z));

            currentGridPos = BuildingManager.Instance.GetGridPosition(worldPos);

            Vector2 snappedWorldPos = BuildingManager.Instance.GetWorldPosition(currentGridPos);
            previewObject.transform.position = snappedWorldPos;

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
    }
}