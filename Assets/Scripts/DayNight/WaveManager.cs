using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ARIA.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ARIA.DayNight
{
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        [Header("Wave Configuration")]
        public List<WaveData> Waves = new List<WaveData>();

        [Header("Current State")]
        public int CurrentWaveIndex = 0;
        public int CurrentDay = 1;
        public bool IsWaveActive = false;

        private Coroutine waveCoroutine;

        public event System.Action<WaveData> OnWaveStart;
        public event System.Action<WaveData> OnWaveEnd;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Debug.Log("[WaveManager] Start method called");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += OnGameStateChanged;
                Debug.Log("[WaveManager] Subscribed to GameManager state changes");
            }

            LoadWavesFromResources();
        }

        private void LoadWavesFromResources()
        {
            WaveData[] waves = Resources.LoadAll<WaveData>("Data/Waves");
            if (waves != null && waves.Length > 0)
            {
                Waves.Clear();
                foreach (var wave in waves)
                {
                    if (wave != null)
                    {
                        Waves.Add(wave);
                    }
                }
                Debug.Log($"[WaveManager] 从Resources加载了 {Waves.Count} 个波次数据");
            }
            else
            {
                Debug.LogWarning("[WaveManager] 从Resources没有找到波次数据，尝试从Editor路径加载");
#if UNITY_EDITOR
                LoadWavesFromEditorPath();
#endif
            }
        }

#if UNITY_EDITOR
        private void LoadWavesFromEditorPath()
        {
            string editorPath = "Assets/Data/Waves";
            if (System.IO.Directory.Exists(editorPath))
            {
                string[] assetPaths = System.IO.Directory.GetFiles(editorPath, "*.asset");
                foreach (string path in assetPaths)
                {
                    WaveData wave = UnityEditor.AssetDatabase.LoadAssetAtPath<WaveData>(path);
                    if (wave != null)
                    {
                        Waves.Add(wave);
                    }
                }
                Debug.Log($"[WaveManager] 从Editor路径加载了 {Waves.Count} 个波次数据");
            }
            else
            {
                Debug.LogWarning($"[WaveManager] Editor路径不存在: {editorPath}");
            }
        }
#endif

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= OnGameStateChanged;
            }
        }

        private void OnGameStateChanged(GameState state)
        {
            Debug.Log($"[WaveManager] 游戏状态改变为: {state}");
            if (state == GameState.Night)
            {
                Debug.Log("[WaveManager] 开始夜晚波次");
                StartNightWaves();
            }
            else if (state == GameState.Dawn)
            {
                EndWaves();
            }
        }

        public void StartNightWaves()
        {
            Debug.Log("[WaveManager] 启动夜晚波次协程");
            if (waveCoroutine != null)
            {
                StopCoroutine(waveCoroutine);
                Debug.Log("[WaveManager] 停止之前的波次协程");
            }
            waveCoroutine = StartCoroutine(RunNightWaves());
            Debug.Log("[WaveManager] 波次协程已启动");
        }

        private IEnumerator RunNightWaves()
        {
            Debug.Log("[WaveManager] 开始运行夜晚波次");
            List<WaveData> nightWaves = GetWavesForDay(CurrentDay);
            Debug.Log($"[WaveManager] 找到 {nightWaves.Count} 个波次");

            foreach (var wave in nightWaves)
            {
                Debug.Log($"[WaveManager] 运行波次: {wave.WaveNumber}, 天数: {wave.DayNumber}");
                yield return StartCoroutine(RunWave(wave));
                Debug.Log($"[WaveManager] 波次 {wave.WaveNumber} 完成，等待 {wave.WaveDelay} 秒");
                yield return new WaitForSeconds(wave.WaveDelay);
            }

            Debug.Log("[WaveManager] 夜晚波次全部完成");
            IsWaveActive = false;
        }

        private IEnumerator RunWave(WaveData wave)
        {
            Debug.Log("[WaveManager] 开始运行波次");
            IsWaveActive = true;
            CurrentWaveIndex++;

            OnWaveStart?.Invoke(wave);
            EventManager.Instance?.TriggerEvent(GameEvents.WAVE_START, wave);

            // 模拟波次完成
            Debug.Log("[WaveManager] 波次处理中...");
            yield return new WaitForSeconds(5f);

            Debug.Log("[WaveManager] 波次结束");
            OnWaveEnd?.Invoke(wave);
            EventManager.Instance?.TriggerEvent(GameEvents.WAVE_END, wave);

            GrantWaveRewards(wave);
            Debug.Log("[WaveManager] 波次奖励已发放");
        }

        private void EndWaves()
        {
            if (waveCoroutine != null)
            {
                StopCoroutine(waveCoroutine);
                waveCoroutine = null;
            }

            IsWaveActive = false;
            CurrentDay++;
        }

        private List<WaveData> GetWavesForDay(int day)
        {
            List<WaveData> dayWaves = new List<WaveData>();

            foreach (var wave in Waves)
            {
                if (wave.DayNumber == day)
                {
                    dayWaves.Add(wave);
                }
            }

            if (dayWaves.Count == 0)
            {
                dayWaves.Add(GenerateDefaultWave(day));
            }

            return dayWaves;
        }

        private WaveData GenerateDefaultWave(int day)
        {
            WaveData wave = ScriptableObject.CreateInstance<WaveData>();
            wave.WaveNumber = CurrentWaveIndex + 1;
            wave.DayNumber = day;
            wave.SpawnInterval = 0.5f;
            wave.WaveDelay = 2f;
            wave.RewardGold = 100 + day * 50;

            return wave;
        }

        private void GrantWaveRewards(WaveData wave)
        {
            // Grant gold reward
            // CurrencyManager.Instance?.AddGold(wave.RewardGold);

            // Grant card rewards
            if (wave.RewardCardIds != null)
            {
                foreach (var cardId in wave.RewardCardIds)
                {
                    Card.CardManager.Instance?.AddCard(cardId, 1);
                }
            }
        }

        public WaveData GetCurrentWave()
        {
            if (CurrentWaveIndex < Waves.Count)
            {
                return Waves[CurrentWaveIndex];
            }
            return null;
        }

        public float GetWaveProgress()
        {
            if (!IsWaveActive) return 0f;
            return 1f;
        }
    }
}
