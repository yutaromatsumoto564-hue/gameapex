using UnityEngine;
using System;
using ARIA.Core;
using ARIA.Resource;
using ARIA.Power;

namespace ARIA.Building
{
    public class Building : MonoBehaviour
    {
        [Header("Building Info")]
        public int InstanceId;
        public BuildingData Data;
        public Vector2Int GridPosition;

        [Header("State")]
        public int CurrentHealth;
        public bool IsActive = true;
        public bool IsPowered = true;
        public bool IsProducing = false;

        [Header("Production")]
        public float ProductionProgress;
        public float ProductionTimer;

        [Header("Defense")]
        public float AttackTimer;
        public GameObject Target;
        
        [Header("Effects")]
        public GameObject BuildEffectPrefab;
        public GameObject UpgradeEffectPrefab;
        public GameObject ProductionEffectPrefab;
        
        [Header("Repair")]
        public bool AutoRepair = false;
        public float RepairRate = 1f;
        public float RepairTimer = 0f;

        public event Action<int> OnHealthChanged;
        public event Action<bool> OnActiveStateChanged;
        public event Action<float> OnProductionProgress;

        private SpriteRenderer spriteRenderer;
        private Color originalColor;

        public void Initialize(int instanceId, BuildingData data, Vector2Int gridPosition)
        {
            InstanceId = instanceId;
            Data = data;
            GridPosition = gridPosition;
            CurrentHealth = data.MaxHealth;
            IsActive = true;
            ProductionProgress = 0f;
            ProductionTimer = 0f;

            transform.position = BuildingManager.Instance.GetWorldPosition(gridPosition);

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            originalColor = spriteRenderer.color;

            if (data.SizeX > 1 || data.SizeY > 1)
            {
                Vector3 scale = new Vector3(data.SizeX, data.SizeY, 1f);
                transform.localScale = scale;
            }
            
            // 检查资源产出配置
            if (data.Outputs != null && data.Outputs.Count > 0)
            {
                Debug.Log($"[建筑初始化] {data.BuildingName} 配置了 {data.Outputs.Count} 个产出，生产时间: {data.ProductionTime}秒");
            }
            else
            {
                Debug.Log($"[建筑初始化] {data.BuildingName} 没有配置资源产出");
            }
        }

        private void Update()
        {
            if (!IsActive) return;

            UpdatePowerStatus();

            if (!IsPowered && Data.PowerConsumption > 0) return;

            // 自动维修
            if (AutoRepair && CurrentHealth < Data.MaxHealth)
            {
                UpdateAutoRepair();
            }

            switch (Data.Category)
            {
                case BuildingCategory.Production:
                    UpdateProduction();
                    break;
                case BuildingCategory.Resource:
                    UpdateResourceGathering();
                    break;
                case BuildingCategory.Defense:
                    UpdateDefense();
                    break;
                case BuildingCategory.Power:
                    UpdatePowerGeneration();
                    break;
            }
        }
        
        private void UpdateAutoRepair()
        {
            RepairTimer += Time.deltaTime;
            if (RepairTimer >= 1f / RepairRate)
            {
                Repair(1);
                RepairTimer = 0f;
            }
        }

        private void UpdatePowerStatus()
        {
            if (Data.PowerConsumption <= 0)
            {
                IsPowered = true;
                return;
            }

            IsPowered = PowerManager.Instance?.HasEnoughPower() ?? false;
            UpdateVisualState();
        }

        private void UpdateProduction()
        {
            if (Data.Inputs.Count == 0 && Data.Outputs.Count == 0) return;

            if (!CanProduce())
            {
                IsProducing = false;
                return;
            }

            IsProducing = true;
            ProductionTimer += Time.deltaTime;
            ProductionProgress = ProductionTimer / Data.ProductionTime;

            OnProductionProgress?.Invoke(ProductionProgress);

            if (ProductionTimer >= Data.ProductionTime)
            {
                Produce();
                ProductionTimer = 0f;
                ProductionProgress = 0f;
            }
        }

        private bool CanProduce()
        {
            foreach (var input in Data.Inputs)
            {
                if (!ResourceManager.Instance.HasResource(input.ResourceId, input.Amount))
                {
                    return false;
                }
            }
            return true;
        }

        private void Produce()
        {
            foreach (var input in Data.Inputs)
            {
                ResourceManager.Instance.RemoveResource(input.ResourceId, input.Amount);
            }

            foreach (var output in Data.Outputs)
            {
                ResourceManager.Instance.AddResource(output.ResourceId, output.Amount);
            }
            
            // 触发生产完成特效
            PlayProductionEffect();
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

        private void UpdateResourceGathering()
        {
            if (Data.Outputs == null || Data.Outputs.Count == 0) return;
            
            ProductionTimer += Time.deltaTime;

            if (ProductionTimer >= Data.ProductionTime)
            {
                foreach (var output in Data.Outputs)
                {
                    ResourceManager.Instance.AddResource(output.ResourceId, output.Amount);
                    Debug.Log($"[资源生产] {Data.BuildingName} 生产了 {output.Amount} {output.ResourceId}");
                }
                ProductionTimer = 0f;
            }
        }

        private void UpdateDefense()
        {
            // Defense functionality will be implemented when enemy system is added
        }

        private void UpdatePowerGeneration()
        {
            // Power is handled by PowerManager
        }

        public void TakeDamage(int damage)
        {
            CurrentHealth -= damage;
            OnHealthChanged?.Invoke(CurrentHealth);

            EventManager.Instance?.TriggerEvent(GameEvents.BUILDING_DAMAGED, this, damage);

            UpdateVisualState();

            if (CurrentHealth <= 0)
            {
                Destroy();
            }
        }

        public void Repair(int amount)
        {
            CurrentHealth = Mathf.Min(CurrentHealth + amount, Data.MaxHealth);
            OnHealthChanged?.Invoke(CurrentHealth);
            UpdateVisualState();
        }

        private void Destroy()
        {
            EventManager.Instance?.TriggerEvent(GameEvents.BUILDING_DESTROYED, this);
            BuildingManager.Instance?.RemoveBuilding(this);
        }

        private void UpdateVisualState()
        {
            if (spriteRenderer == null) return;

            float healthPercent = (float)CurrentHealth / Data.MaxHealth;

            if (healthPercent <= 0.25f)
            {
                spriteRenderer.color = new Color(1f, 0.3f, 0.3f, 1f);
            }
            else if (healthPercent <= 0.5f)
            {
                spriteRenderer.color = new Color(1f, 0.7f, 0.3f, 1f);
            }
            else if (healthPercent <= 0.75f)
            {
                spriteRenderer.color = new Color(1f, 1f, 0.7f, 1f);
            }
            else if (!IsPowered)
            {
                spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }
            else
            {
                spriteRenderer.color = originalColor;
            }
        }

        public void SetActive(bool active)
        {
            IsActive = active;
            OnActiveStateChanged?.Invoke(IsActive);
        }

        private void OnDrawGizmosSelected()
        {
            if (Data != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(transform.position, new Vector3(Data.SizeX, Data.SizeY, 0.1f));

                if (Data.AttackRange > 0)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(transform.position, Data.AttackRange);
                }

                if (Data.NetworkRange > 0)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(transform.position, Data.NetworkRange);
                }
            }
        }
    }
}
