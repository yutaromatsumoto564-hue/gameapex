using UnityEngine;
using UnityEngine.UI;
using ARIA.Resource;

namespace ARIA.UI
{
    public class ResourceItemUI : MonoBehaviour
    {
        public string ResourceId { get; private set; }
        
        [Header("UI Components")]
        public Image IconImage;
        public Text AmountText;
        public Text NameText;
        
        private int currentAmount;
        
        public void Setup(string resourceId, string name, Sprite icon)
        {
            ResourceId = resourceId;
            
            if (NameText != null)
            {
                NameText.text = name;
            }
            
            if (IconImage != null && icon != null)
            {
                IconImage.sprite = icon;
            }
            
            UpdateAmount(0);
        }
        
        public void UpdateAmount(int amount)
        {
            currentAmount = amount;
            
            if (AmountText != null)
            {
                AmountText.text = FormatAmount(amount);
            }
            
            // 如果数量为0，可以隐藏或变灰
            gameObject.SetActive(amount > 0);
        }
        
        private string FormatAmount(int amount)
        {
            if (amount >= 1000000)
                return $"{amount / 1000000f:0.0}M";
            if (amount >= 1000)
                return $"{amount / 1000f:0.0}K";
            return amount.ToString();
        }
        
        public void OnPointerEnter()
        {
            // 显示资源提示
            var resourceData = ResourceManager.Instance?.GetResourceData(ResourceId);
            if (resourceData != null)
            {
                Vector2 tooltipPos = Input.mousePosition;
                GameUIManager.Instance?.ShowTooltip(resourceData.ResourceName, resourceData.Description, tooltipPos);
            }
        }
        
        public void OnPointerExit()
        {
            GameUIManager.Instance?.HideTooltip();
        }
    }
}
