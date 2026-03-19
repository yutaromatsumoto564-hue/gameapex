using UnityEngine;
using System;

namespace ARIA.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Settings")]
        public int CurrentDay = 1;
        public float DayDuration = 300f;
        public float NightDuration = 180f;
        public float DuskDuration = 30f;
        public float DawnDuration = 30f;

        [Header("Game State")]
        public GameState CurrentState = GameState.Day;
        public float CurrentTime { get; private set; }
        public bool IsPaused { get; private set; }

        public event Action<GameState> OnStateChanged;
        public event Action<int> OnDayChanged;
        public event Action<float> OnTimeUpdated;

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
            InitializeGame();
        }

        private void Update()
        {
            if (IsPaused) return;

            UpdateGameTime();
        }

        private void InitializeGame()
        {
            CurrentDay = 1;
            CurrentState = GameState.Day;
            CurrentTime = 0f;
            IsPaused = false;

            OnDayChanged?.Invoke(CurrentDay);
            OnStateChanged?.Invoke(CurrentState);
        }

        private void UpdateGameTime()
        {
            CurrentTime += Time.deltaTime;

            float stateDuration = GetCurrentStateDuration();

            if (CurrentTime >= stateDuration)
            {
                CurrentTime = 0f;
                AdvanceToNextState();
            }

            OnTimeUpdated?.Invoke(CurrentTime / stateDuration);
        }

        public float GetCurrentStateDuration()
        {
            return CurrentState switch
            {
                GameState.Day => DayDuration,
                GameState.Dusk => DuskDuration,
                GameState.Night => NightDuration,
                GameState.Dawn => DawnDuration,
                _ => DayDuration
            };
        }

        private void AdvanceToNextState()
        {
            GameState nextState = CurrentState switch
            {
                GameState.Day => GameState.Dusk,
                GameState.Dusk => GameState.Night,
                GameState.Night => GameState.Dawn,
                GameState.Dawn => GameState.Day,
                _ => GameState.Day
            };

            if (CurrentState == GameState.Dawn && nextState == GameState.Day)
            {
                CurrentDay++;
                OnDayChanged?.Invoke(CurrentDay);
            }

            CurrentState = nextState;
            OnStateChanged?.Invoke(CurrentState);
        }

        public void PauseGame()
        {
            IsPaused = true;
            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            IsPaused = false;
            Time.timeScale = 1f;
        }

        public void TogglePause()
        {
            if (IsPaused)
                ResumeGame();
            else
                PauseGame();
        }

        public void SetTimeScale(float scale)
        {
            Time.timeScale = IsPaused ? 0f : scale;
        }

        public string GetFormattedTime()
        {
            float remainingTime = GetCurrentStateDuration() - CurrentTime;
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            return $"{minutes:D2}:{seconds:D2}";
        }

        public void ForceNight()
        {
            Debug.Log("[GameManager] 开始强制切换到夜晚状态");
            CurrentState = GameState.Night;
            CurrentTime = 0f;
            Debug.Log($"[GameManager] 状态已设置为: {CurrentState}");
            Debug.Log($"[GameManager] OnStateChanged 事件订阅者数量: {OnStateChanged?.GetInvocationList().Length ?? 0}");
            OnStateChanged?.Invoke(CurrentState);
            Debug.Log("[GameManager] 强制切换到夜晚状态完成");
        }
    }

    public enum GameState
    {
        Day,
        Dusk,
        Night,
        Dawn
    }
}
