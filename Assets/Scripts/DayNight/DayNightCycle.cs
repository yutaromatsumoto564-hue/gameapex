using UnityEngine;
using System.Collections.Generic;
using ARIA.Core;

namespace ARIA.DayNight
{
    public class DayNightCycle : MonoBehaviour
    {
        public static DayNightCycle Instance { get; private set; }

        [Header("Lighting")]
        public Light DirectionalLight;
        public Gradient AmbientColor;
        public Gradient DirectionalColor;
        public AnimationCurve LightIntensity;

        [Header("Colors")]
        public Color DayColor = new Color(1f, 1f, 0.9f);
        public Color DuskColor = new Color(1f, 0.6f, 0.3f);
        public Color NightColor = new Color(0.2f, 0.2f, 0.4f);
        public Color DawnColor = new Color(1f, 0.8f, 0.6f);

        [Header("Background")]
        public SpriteRenderer BackgroundRenderer;
        public Gradient BackgroundGradient;

        private GameState previousState;

        public event System.Action<GameState> OnVisualStateChanged;

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
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += OnGameStateChanged;
                GameManager.Instance.OnTimeUpdated += OnTimeUpdated;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= OnGameStateChanged;
                GameManager.Instance.OnTimeUpdated -= OnTimeUpdated;
            }
        }

        private void OnGameStateChanged(GameState newState)
        {
            if (newState != previousState)
            {
                previousState = newState;
                UpdateLighting(newState, 0f);
                OnVisualStateChanged?.Invoke(newState);
            }
        }

        private void OnTimeUpdated(float progress)
        {
            UpdateLighting(GameManager.Instance.CurrentState, progress);
        }

        private void UpdateLighting(GameState state, float progress)
        {
            if (DirectionalLight != null)
            {
                Color targetColor = GetStateColor(state);
                float intensity = GetStateIntensity(state, progress);

                DirectionalLight.color = targetColor;
                DirectionalLight.intensity = intensity;

                float angle = GetStateAngle(state, progress);
                DirectionalLight.transform.rotation = Quaternion.Euler(angle, 170f, 0f);
            }

            if (BackgroundRenderer != null && BackgroundGradient != null)
            {
                float timeOfDay = GetTimeOfDay(state, progress);
                BackgroundRenderer.color = BackgroundGradient.Evaluate(timeOfDay);
            }

            RenderSettings.ambientLight = GetStateColor(state);
        }

        private Color GetStateColor(GameState state)
        {
            return state switch
            {
                GameState.Day => DayColor,
                GameState.Dusk => DuskColor,
                GameState.Night => NightColor,
                GameState.Dawn => DawnColor,
                _ => DayColor
            };
        }

        private float GetStateIntensity(GameState state, float progress)
        {
            return state switch
            {
                GameState.Day => 1f,
                GameState.Dusk => Mathf.Lerp(1f, 0.3f, progress),
                GameState.Night => 0.3f,
                GameState.Dawn => Mathf.Lerp(0.3f, 1f, progress),
                _ => 1f
            };
        }

        private float GetStateAngle(GameState state, float progress)
        {
            return state switch
            {
                GameState.Day => Mathf.Lerp(90f, 180f, progress),
                GameState.Dusk => Mathf.Lerp(180f, 200f, progress),
                GameState.Night => Mathf.Lerp(200f, 270f, progress),
                GameState.Dawn => Mathf.Lerp(270f, 360f, progress),
                _ => 90f
            };
        }

        private float GetTimeOfDay(GameState state, float progress)
        {
            float baseTime = state switch
            {
                GameState.Day => 0.25f,
                GameState.Dusk => 0.45f,
                GameState.Night => 0.5f,
                GameState.Dawn => 0.9f,
                _ => 0.25f
            };

            float range = 0.1f;
            return Mathf.Clamp01(baseTime + progress * range);
        }

        public bool IsNight()
        {
            return GameManager.Instance?.CurrentState == GameState.Night;
        }

        public bool IsDay()
        {
            return GameManager.Instance?.CurrentState == GameState.Day;
        }

        public float GetDayProgress()
        {
            if (GameManager.Instance == null) return 0f;

            float totalDayTime = GameManager.Instance.DayDuration + 
                                 GameManager.Instance.DuskDuration +
                                 GameManager.Instance.NightDuration +
                                 GameManager.Instance.DawnDuration;

            float currentTime = 0f;
            var state = GameManager.Instance.CurrentState;

            currentTime += state switch
            {
                GameState.Day => GameManager.Instance.CurrentTime,
                GameState.Dusk => GameManager.Instance.DayDuration + GameManager.Instance.CurrentTime,
                GameState.Night => GameManager.Instance.DayDuration + GameManager.Instance.DuskDuration + GameManager.Instance.CurrentTime,
                GameState.Dawn => GameManager.Instance.DayDuration + GameManager.Instance.DuskDuration + GameManager.Instance.NightDuration + GameManager.Instance.CurrentTime,
                _ => 0f
            };

            return currentTime / totalDayTime;
        }
    }
}
