using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ARIA.Card;

namespace ARIA.UI
{
    public class CardSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler,
                              IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("UI Components")]
        public Image CardImage;
        public Text AmountText;
        public Text NameText;
        public Image RarityBorder;
        public GameObject SelectedIndicator;
        
        [Header("Rarity Colors")]
        public Color CommonColor = Color.gray;
        public Color UncommonColor = Color.green;
        public Color RareColor = Color.blue;
        public Color EpicColor = Color.magenta;
        public Color LegendaryColor = Color.yellow;
        
        private CardData currentCard;
        private int currentAmount;
        private int slotIndex;
        private bool isSelected;
        
        // 公共属性供外部访问
        public CardData CardData => currentCard;
        public int Amount => currentAmount;
        public int SlotIndex => slotIndex;
        public bool IsSelected => isSelected;
        
        // 事件
        public System.Action<CardSlotUI> OnSlotClicked;
        public System.Action<CardSlotUI> OnSlotDragged;
        
        public void SetIndex(int index)
        {
            slotIndex = index;
        }
        
        public void SetCard(CardData card, int amount)
        {
            currentCard = card;
            currentAmount = amount;
            
            if (card == null)
            {
                Clear();
                return;
            }
            
            // 更新图标
            if (CardImage != null)
            {
                CardImage.sprite = card.Icon;
                CardImage.color = Color.white;
            }
            
            // 更新数量
            if (AmountText != null)
            {
                AmountText.text = amount > 1 ? amount.ToString() : "";
            }
            
            // 更新名称
            if (NameText != null)
            {
                NameText.text = card.CardName;
            }
            
            // 更新边框颜色（根据稀有度）
            if (RarityBorder != null)
            {
                RarityBorder.color = GetRarityColor(card.Rarity);
            }
            
            gameObject.SetActive(true);
        }
        
        public void Clear()
        {
            ClearSlot();
        }
        
        public void ClearSlot()
        {
            currentCard = null;
            currentAmount = 0;
            
            if (CardImage != null)
            {
                CardImage.sprite = null;
                CardImage.color = new Color(1, 1, 1, 0.2f);
            }
            
            if (AmountText != null)
            {
                AmountText.text = "";
            }
            
            if (NameText != null)
            {
                NameText.text = "";
            }
            
            SetSelected(false);
        }
        
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            if (SelectedIndicator != null)
            {
                SelectedIndicator.SetActive(selected);
            }
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (currentCard == null) return;
            
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                // 左键点击 - 选择/使用卡牌
                OnCardClicked();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                // 右键点击 - 显示详情
                OnCardRightClicked();
            }
        }
        
        private void OnCardClicked()
        {
            if (currentCard == null) return;
            
            // 触发事件
            OnSlotClicked?.Invoke(this);
            
            // 通知UI管理器选择此卡牌
            GameUIManager.Instance?.ShowMessage($"Selected: {currentCard.CardName}");
            
            // 如果是建筑卡牌，进入放置模式
            if (currentCard.Type == CardType.Building)
            {
                SetSelected(true);
                // TODO: 启动建筑放置系统
            }
            else
            {
                // 直接使用卡牌
                // TODO: 实现卡牌使用逻辑
            }
        }
        
        private void OnCardRightClicked()
        {
            if (currentCard == null) return;
            
            // 显示卡牌详情
            // TODO: 打开卡牌详情面板
            Debug.Log($"Card Details: {currentCard.CardName}\n{currentCard.Description}");
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentCard == null) return;
            
            // 显示提示
            Vector2 tooltipPos = Input.mousePosition;
            GameUIManager.Instance?.ShowTooltip(
                $"{currentCard.CardName} (x{currentAmount})", 
                currentCard.Description, 
                tooltipPos
            );
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            GameUIManager.Instance?.HideTooltip();
        }
        
        private Color GetRarityColor(CardRarity rarity)
        {
            switch (rarity)
            {
                case CardRarity.Common: return CommonColor;
                case CardRarity.Uncommon: return UncommonColor;
                case CardRarity.Rare: return RareColor;
                case CardRarity.Epic: return EpicColor;
                case CardRarity.Legendary: return LegendaryColor;
                default: return Color.white;
            }
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (currentCard == null || currentCard.Type != CardType.Building) return;

            BuildingPlacementController.Instance?.StartDragPreview(currentCard);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (currentCard == null || currentCard.Type != CardType.Building) return;

            BuildingPlacementController.Instance?.UpdateDragPreview(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (currentCard == null || currentCard.Type != CardType.Building) return;

            bool success = BuildingPlacementController.Instance?.EndDragPreview() ?? false;
            
            if (success)
            {
                CardManager.Instance.RemoveCard(currentCard.CardId, 1);
            }
        }
    }
}
