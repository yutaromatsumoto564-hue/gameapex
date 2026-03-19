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

        private void Start()
        {
            // 核心修复：射线检测穿透所有 Trigger
            Physics2D.queriesHitTriggers = false;

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            originalColor = spriteRenderer.color;
            originalScale = transform.localScale;
            
            // 1. 获取当前物体上所有的碰撞体
            Collider2D[] allColliders = GetComponents<Collider2D>();
            
            // 2. 寻找或创建一个"标准实体碰撞体"
            // 这个碰撞体必须和建筑尺寸(SizeX, SizeY)严格一致，用于判定位置占用
            BoxCollider2D mainBodyCollider = null;

            foreach (var col in allColliders)
            {
                // 如果发现一个 BoxCollider 且尺寸正好等于建筑尺寸，我们认为它是"本体"
                if (col is BoxCollider2D box && Data != null &&
                    Mathf.Approximately(box.size.x, Data.SizeX) &&
                    Mathf.Approximately(box.size.y, Data.SizeY))
                {
                    mainBodyCollider = box;
                    mainBodyCollider.isTrigger = false; // 只有它不是 Trigger
                }
                else
                {
                    // 【关键修复】：所有其他的碰撞体（比如巨大的圆形、没调好的方框）
                    // 全部强制转为 Trigger，这样它们就挡不住鼠标点击了
                    col.isTrigger = true;
                }
            }

            // 3. 如果没找到合适的本体碰撞体，代码自动补一个完美的
            if (mainBodyCollider == null && Data != null)
            {
                mainBodyCollider = gameObject.AddComponent<BoxCollider2D>();
                mainBodyCollider.size = new Vector2(Data.SizeX, Data.SizeY);
                mainBodyCollider.offset = Vector2.zero;
                mainBodyCollider.isTrigger = false;
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