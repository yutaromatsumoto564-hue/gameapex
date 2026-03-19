using UnityEngine;
using System;
using ARIA.Core;
using ARIA.Resource;

namespace ARIA.Building
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Building : MonoBehaviour
    {
        [Header("Building Info")]
        public int InstanceId;
        public BuildingData Data;
        public Vector2Int GridPosition;

        [Header("State")]
        public bool IsActive = true;

        [Header("Progress State")]
        public float CurrentProgress = 0f;
        public bool IsFull = false;

        [Header("Effects")]
        public GameObject BuildEffectPrefab;
        public GameObject UpgradeEffectPrefab;
        public GameObject ProductionEffectPrefab;

        private SpriteRenderer spriteRenderer;
        private Vector3 originalScale;
        private Color originalColor;

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            originalColor = spriteRenderer.color;
            originalScale = transform.localScale;
            
            // 1. 安全地获取或动态添加碰撞体
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            if (col == null)
            {
                col = gameObject.AddComponent<BoxCollider2D>();
            }
            
            // 2. 自动适配 Collider 的大小，确保点击区域和建筑占地一样大
            if (Data != null)
            {
                col.size = new Vector2(Data.SizeX, Data.SizeY);
            }
        }

        private void Update()
        {
            if (!IsActive) return;

            // 只有生产和资源类建筑才走进度条逻辑
            if (Data.Category == BuildingCategory.Production || Data.Category == BuildingCategory.Resource)
            {
                UpdatePassiveProgress();
            }
        }

        private void UpdatePassiveProgress()
        {
            // 1. 检查是否爆仓 (如果有输出产物的话)
            if (Data.Outputs != null && Data.Outputs.Count > 0)
            {
                int totalOutput = 0;
                foreach (var output in Data.Outputs) totalOutput += output.Amount;
                
                if (!ResourceManager.Instance.HasAvailableCapacity(totalOutput))
                {
                    IsFull = true;
                    // 爆仓时变灰
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                    }
                    return; // 停止进度增长
                }
            }

            IsFull = false;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor; // 恢复正常颜色
            }

            // 2. 被动增加进度
            CurrentProgress += Data.PassiveProgressPerSecond * Time.deltaTime;

            // 3. 检查是否达到产出要求
            CheckAndProduce();
        }

        private void CheckAndProduce()
        {
            if (CurrentProgress >= Data.ProgressRequired)
            {
                // 扣除所需进度 (保留溢出的部分)
                CurrentProgress -= Data.ProgressRequired;

                // 自动上传到全局资源库！无需连线！
                foreach (var output in Data.Outputs)
                {
                    ResourceManager.Instance.AddResource(output.ResourceId, output.Amount);
                }
                
                // 触发生产完成特效
                PlayProductionEffect();
            }
        }

        // Unity 原生方法：当鼠标在当前物体的 Collider 范围内按下时触发
        private void OnMouseDown()
        {
            if (!IsActive || IsFull) return; // 如果爆仓或停机了，点击无效

            // 1. 增加进度
            CurrentProgress += Data.ClickProgress;

            // 2. 立即检查是否满足产出条件
            CheckAndProduce();

            // 3. 视觉反馈：瞬间缩小（配合后面的缩放恢复代码，产生 Q弹 的手感）
            transform.localScale = originalScale * 0.85f;
        }

        // 用于恢复点击导致的缩放变形
        private void LateUpdate()
        {
            if (transform.localScale != originalScale)
            {
                // 平滑弹回原来的大小
                transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * 15f);
            }
        }

        public void Initialize(int instanceId, BuildingData data, Vector2Int gridPosition)
        {
            InstanceId = instanceId;
            Data = data;
            GridPosition = gridPosition;

            // 计算并设置建筑的世界位置
            Vector2 worldPos = BuildingManager.Instance.GetWorldPosition(gridPosition, data.SizeX, data.SizeY);
            transform.position = new Vector3(worldPos.x, worldPos.y, 0f);

            // 设置建筑缩放
            transform.localScale = new Vector3(data.SizeX, data.SizeY, 1f);
            originalScale = transform.localScale;

            IsActive = true;
        }

        public void PlayBuildEffect()
        {
            if (BuildEffectPrefab != null)
            {
                GameObject effect = Instantiate(BuildEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }
        
        public void PlayUpgradeEffect()
        {
            if (UpgradeEffectPrefab != null)
            {
                GameObject effect = Instantiate(UpgradeEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }
        
        public void PlayProductionEffect()
        {
            if (ProductionEffectPrefab != null)
            {
                GameObject effect = Instantiate(ProductionEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }

        public void SetActive(bool active)
        {
            IsActive = active;
        }

        private void OnDrawGizmosSelected()
        {
            if (Data != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(transform.position, new Vector3(Data.SizeX, Data.SizeY, 0.1f));
            }
        }
    }
}