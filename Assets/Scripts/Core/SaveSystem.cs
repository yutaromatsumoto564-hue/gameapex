using UnityEngine;
using System;
using System.IO;
using ARIA.Resource;
using ARIA.Building;
using ARIA.Card;

namespace ARIA.Core
{
    [Serializable]
    public class GameSaveData
    {
        public int SaveVersion;
        public string SaveTime;
        public int CurrentDay;
        public int GameState;
        public float CurrentTime;
        public ResourceSaveData Resources;
        public BuildingSaveData[] Buildings;
        public CardSaveData[] Cards;
        public TechSaveData Tech;
    }

    [Serializable]
    public class ResourceSaveData
    {
        public string[] ResourceNames;
        public int[] ResourceAmounts;
    }

    [Serializable]
    public class BuildingSaveData
    {
        public string BuildingId;
        public int PosX;
        public int PosY;
        public int Health;
        public bool IsActive;
    }

    [Serializable]
    public class CardSaveData
    {
        public string CardId;
        public int Amount;
    }

    [Serializable]
    public class TechSaveData
    {
        public string[] UnlockedTechIds;
        public string[] ResearchingTechIds;
        public float[] ResearchProgress;
    }

    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        private const int CURRENT_SAVE_VERSION = 1;
        private const string SAVE_FOLDER = "Saves";
        private const string SAVE_EXTENSION = ".json";

        private string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FOLDER);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (!Directory.Exists(SavePath))
            {
                Directory.CreateDirectory(SavePath);
            }
        }

        public void SaveGame(int slotIndex)
        {
            GameSaveData saveData = CreateSaveData();

            string fileName = $"save_{slotIndex}{SAVE_EXTENSION}";
            string filePath = Path.Combine(SavePath, fileName);

            string jsonString = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(filePath, jsonString);

            Debug.Log($"Game saved to: {filePath}");
        }

        public GameSaveData LoadGame(int slotIndex)
        {
            string fileName = $"save_{slotIndex}{SAVE_EXTENSION}";
            string filePath = Path.Combine(SavePath, fileName);

            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"Save file not found: {filePath}");
                return null;
            }

            string jsonString = File.ReadAllText(filePath);
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(jsonString);

            if (saveData.SaveVersion != CURRENT_SAVE_VERSION)
            {
                Debug.LogWarning($"Save version mismatch: {saveData.SaveVersion} vs {CURRENT_SAVE_VERSION}");
            }

            ApplySaveData(saveData);

            Debug.Log($"Game loaded from: {filePath}");
            return saveData;
        }

        public bool HasSaveFile(int slotIndex)
        {
            string fileName = $"save_{slotIndex}{SAVE_EXTENSION}";
            string filePath = Path.Combine(SavePath, fileName);
            return File.Exists(filePath);
        }

        public void DeleteSave(int slotIndex)
        {
            string fileName = $"save_{slotIndex}{SAVE_EXTENSION}";
            string filePath = Path.Combine(SavePath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"Save deleted: {filePath}");
            }
        }

        public void AutoSave()
        {
            SaveGame(0);
        }

        public GameSaveData CreateSaveData()
        {
            var gameManager = GameManager.Instance;
            var resourceManager = ResourceManager.Instance;
            var buildingManager = BuildingManager.Instance;
            var cardManager = CardManager.Instance;

            var saveData = new GameSaveData
            {
                SaveVersion = CURRENT_SAVE_VERSION,
                SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                CurrentDay = gameManager.CurrentDay,
                GameState = (int)gameManager.CurrentState,
                CurrentTime = gameManager.CurrentTime
            };

            if (resourceManager != null)
            {
                saveData.Resources = resourceManager.GetSaveData();
            }

            if (buildingManager != null)
            {
                saveData.Buildings = buildingManager.GetSaveData();
            }

            if (cardManager != null)
            {
                saveData.Cards = cardManager.GetSaveData();
            }

            return saveData;
        }

        private void ApplySaveData(GameSaveData saveData)
        {
            var gameManager = GameManager.Instance;
            
            gameManager.CurrentDay = saveData.CurrentDay;
            gameManager.CurrentState = (GameState)saveData.GameState;

            var resourceManager = ResourceManager.Instance;
            if (resourceManager != null && saveData.Resources != null)
            {
                resourceManager.ApplySaveData(saveData.Resources);
            }

            var buildingManager = BuildingManager.Instance;
            if (buildingManager != null && saveData.Buildings != null)
            {
                buildingManager.ApplySaveData(saveData.Buildings);
            }

            var cardManager = CardManager.Instance;
            if (cardManager != null && saveData.Cards != null)
            {
                cardManager.ApplySaveData(saveData.Cards);
            }
        }
    }
}
