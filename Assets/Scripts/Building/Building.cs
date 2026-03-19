using UnityEngine;
using System;
using ARIA.Core;
using ARIA.Resource;
using ARIA.Power;

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
        public bool IsPowered = true;

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

        private void Awake()
        {
            // 1. 确保全局设置正确
            Physics2D.queriesHitTriggers = false;

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }

        private void Update()
        {
            if (!IsActive) return;

            // 1. 电力系统：如果建筑需要耗电，检测是否有电
            if (Data.PowerConsumption > 0 && PowerManager.Instance != null)
            {
                IsPowered = PowerManager.Instance.HasEnoughPower();
                if (!IsPowered) return; // 停电罢工
            }

            // 2. 根据建筑类型，分别执行对应的工作逻辑
            switch (Data.Category)
            {
                case BuildingCategory.Production:
                case BuildingCategory.Resource:
                    UpdatePassiveProgress(); // 生产和采矿：走全新的进度条与点击逻辑
                    break;
                case BuildingCategory.Power:
                    UpdatePowerGeneration(); // 发电建筑(指挥中心)：走原有的发电逻辑
                    break;
                case BuildingCategory.Defense:
                    UpdateDefense();         // 防御塔：走原有的索敌开火逻辑
                    break;
            }
        }

        private void UpdatePassiveProgress()
        {
            // 1. 爆仓检测：确保全局容量足以存放产出
            if (Data.Outputs != null && Data.Outputs.Count > 0)
            {
                int totalOutput = 0;
                foreach (var output in Data.Outputs) totalOutput += output.Amount;
                
                // 检查容量。如果容量已满，进入爆仓停机状态
                if (!ResourceManager.Instance.HasAvailableCapacity(totalOutput))
                {
                    IsFull = true;
                    if (spriteRenderer != null) spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 1f); // 爆仓变灰
                    return; // 停止进度增长
                }
            }

            // 容量正常，恢复状态
            IsFull = false;
            if (spriteRenderer != null) spriteRenderer.color = originalColor;

            // 2. 被动增加进度
            CurrentProgress += Data.PassiveProgressPerSecond * Time.deltaTime;

            // 3. 检查是否达到产出要求
            CheckAndProduce();
        }

        private void CheckAndProduce()
        {
            if (CurrentProgress >= Data.ProgressRequired)
            {
                CurrentProgress -= Data.ProgressRequired; 

                // 产出资源，自动上传
                if (Data.Outputs != null)
                {
                    foreach (var output in Data.Outputs)
                    {
                        ResourceManager.Instance.AddResource(output.ResourceId, output.Amount);
                    }
                }
            }
        }

        // Unity 原生方法：当鼠标在当前物体的 Collider 范围内按下时触发
        private void OnMouseDown()
        {
            // 只有当建筑在工作，且没有爆仓时，点击才有效！
            if (!IsActive || IsFull) return; 

            // 只有生产类和资源类增加进度
            if (Data.Category == BuildingCategory.Production || Data.Category == BuildingCategory.Resource)
            {
                CurrentProgress += Data.ClickProgress;
                CheckAndProduce();
            }

            // 所有建筑都可以有 Q弹 视觉反馈
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

        private void UpdatePowerGeneration()
        {
            // 发电建筑的逻辑
            if (PowerManager.Instance != null && Data.PowerGeneration > 0)
            {
                PowerManager.Instance.AddPowerGeneration(InstanceId, Data.PowerGeneration);
            }
        }

        private void UpdateDefense()
        {
            // 防御塔的逻辑
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
            originalColor = spriteRenderer.color;

            // 2. 处理所有碰撞体
            Collider2D[] allColliders = GetComponents<Collider2D>();
            bool foundMainCollider = false;

            foreach (var col in allColliders)
            {
                if (col is BoxCollider2D box)
                {
                    // 强制设置所有BoxCollider2D为1x1大小
                    box.size = new Vector2(1, 1);
                    box.isTrigger = false;
                    foundMainCollider = true;
                    Debug.Log($"[Building] {gameObject.name} 的BoxCollider2D已设置为1x1大小。");
                }
                else
                {
                    // 移除所有非BoxCollider2D的碰撞体
                    Destroy(col);
                    Debug.Log($"[Building] {gameObject.name} 移除了超出范围的碰撞体 {col.GetType().Name}。");
                }
            }

            // 3. 防呆：如果完全没找到BoxCollider，就动态补一个1x1大小的
            if (!foundMainCollider)
            {
                BoxCollider2D newBox = gameObject.AddComponent<BoxCollider2D>();
                newBox.size = new Vector2(1, 1); // 固定为1x1大小
                newBox.isTrigger = false;
                Debug.LogWarning($"[Building] {gameObject.name} 未找到BoxCollider，已自动创建1x1大小的碰撞体。");
            }

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