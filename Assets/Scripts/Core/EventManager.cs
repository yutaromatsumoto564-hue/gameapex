using UnityEngine;
using System;
using System.Collections.Generic;

namespace ARIA.Core
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        private Dictionary<string, Action<object[]>> eventDictionary;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            eventDictionary = new Dictionary<string, Action<object[]>>();
        }

        public void StartListening(string eventName, Action<object[]> listener)
        {
            if (eventDictionary.TryGetValue(eventName, out Action<object[]> thisEvent))
            {
                thisEvent += listener;
                eventDictionary[eventName] = thisEvent;
            }
            else
            {
                thisEvent += listener;
                eventDictionary.Add(eventName, thisEvent);
            }
        }

        public void StopListening(string eventName, Action<object[]> listener)
        {
            if (eventDictionary.TryGetValue(eventName, out Action<object[]> thisEvent))
            {
                thisEvent -= listener;
                eventDictionary[eventName] = thisEvent;
            }
        }

        public void TriggerEvent(string eventName, params object[] parameters)
        {
            if (eventDictionary.TryGetValue(eventName, out Action<object[]> thisEvent))
            {
                thisEvent?.Invoke(parameters);
            }
        }
    }

    public static class GameEvents
    {
        public const string GAME_START = "GameStart";
        public const string GAME_PAUSE = "GamePause";
        public const string GAME_RESUME = "GameResume";
        public const string GAME_SAVE = "GameSave";
        public const string GAME_LOAD = "GameLoad";

        public const string DAY_START = "DayStart";
        public const string DAY_END = "DayEnd";
        public const string NIGHT_START = "NightStart";
        public const string NIGHT_END = "NightEnd";
        public const string DUSK_START = "DuskStart";
        public const string DAWN_START = "DawnStart";

        public const string CARD_PICKUP = "CardPickup";
        public const string CARD_DROP = "CardDrop";
        public const string CARD_CRAFT = "CardCraft";
        public const string CARD_USE = "CardUse";

        public const string BUILDING_PLACED = "BuildingPlaced";
        public const string BUILDING_REMOVED = "BuildingRemoved";
        public const string BUILDING_UPGRADED = "BuildingUpgraded";
        public const string BUILDING_DAMAGED = "BuildingDamaged";
        public const string BUILDING_DESTROYED = "BuildingDestroyed";

        public const string RESOURCE_ADDED = "ResourceAdded";
        public const string RESOURCE_REMOVED = "ResourceRemoved";
        public const string RESOURCE_NETWORK_CHANGED = "ResourceNetworkChanged";

        public const string POWER_CHANGED = "PowerChanged";
        public const string POWER_SHORTAGE = "PowerShortage";

        public const string ENEMY_SPAWNED = "EnemySpawned";
        public const string ENEMY_KILLED = "EnemyKilled";
        public const string WAVE_START = "WaveStart";
        public const string WAVE_END = "WaveEnd";

        public const string TECH_UNLOCKED = "TechUnlocked";
        public const string TECH_RESEARCH_START = "TechResearchStart";
        public const string TECH_RESEARCH_COMPLETE = "TechResearchComplete";

        public const string UI_OPEN = "UIOpen";
        public const string UI_CLOSE = "UIClose";
    }
}
