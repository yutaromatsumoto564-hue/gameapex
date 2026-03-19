using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using ARIA.Core;
using ARIA.Building;

namespace ARIA.Card
{
    public class CardManager : MonoBehaviour
    {
        public static CardManager Instance { get; private set; }

        [Header("Card Database")]
        public List<CardData> AllCards = new List<CardData>();

        [Header("Starting Cards")]
        public List<CardStack> StartingCards = new List<CardStack>();

        private Dictionary<string, int> cardInventory = new Dictionary<string, int>();
        private Dictionary<string, CardData> cardDatabase = new Dictionary<string, CardData>();

        public event Action<string, int> OnCardCountChanged;
        public event Action OnInventoryChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeDatabase();
        }

        private void Start()
        {
            InitializeStartingCards();
        }

        private void InitializeDatabase()
        {
            cardDatabase.Clear();
            
            // 如果AllCards为空，尝试从Resources/Data/Cards加载
            if (AllCards.Count == 0)
            {
                LoadCardsFromResources();
            }
            
            foreach (var card in AllCards)
            {
                if (card != null && !string.IsNullOrEmpty(card.CardId))
                {
                    cardDatabase[card.CardId] = card;
                }
            }
            
            Debug.Log($"CardManager initialized with {cardDatabase.Count} cards");
        }
        
        private void LoadCardsFromResources()
        {
#if UNITY_EDITOR
            // 在编辑器中直接从Assets/Data/Cards加载
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:CardData", new[] { "Assets/Data/Cards" });
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                CardData card = UnityEditor.AssetDatabase.LoadAssetAtPath<CardData>(path);
                if (card != null && !AllCards.Contains(card))
                {
                    AllCards.Add(card);
                }
            }
#else
            // 从Resources/Data/Cards文件夹加载所有卡牌数据
            CardData[] cards = Resources.LoadAll<CardData>("Data/Cards");
            foreach (var card in cards)
            {
                if (card != null && !AllCards.Contains(card))
                {
                    AllCards.Add(card);
                }
            }
#endif
            
            Debug.Log($"Loaded {AllCards.Count} cards from resources");
        }

        private void InitializeStartingCards()
        {
            cardInventory.Clear();
            
            // 如果没有配置初始卡牌，创建默认卡牌
            if (StartingCards == null || StartingCards.Count == 0)
            {
                CreateDefaultStartingCards();
            }
            
            foreach (var stack in StartingCards)
            {
                if (stack.Card != null)
                {
                    AddCard(stack.Card.CardId, stack.Amount);
                }
            }
            
            Debug.Log($"Initialized with {cardInventory.Count} card types in inventory");
        }
        
        private void CreateDefaultStartingCards()
        {
            StartingCards = new List<CardStack>();
            
            var buildingCards = AllCards.Where(c => c.Type == CardType.Building).ToList();
            var resourceCards = AllCards.Where(c => c.Type == CardType.Resource).ToList();
            
            foreach (var card in buildingCards)
            {
                StartingCards.Add(new CardStack { Card = card, Amount = 5 });
            }
            
            foreach (var card in resourceCards)
            {
                StartingCards.Add(new CardStack { Card = card, Amount = 5 });
            }
            
            Debug.Log($"Created {StartingCards.Count} default starting cards for testing");
        }

        public CardData GetCardData(string cardId)
        {
            return cardDatabase.TryGetValue(cardId, out CardData data) ? data : null;
        }

        public int GetCardCount(string cardId)
        {
            return cardInventory.TryGetValue(cardId, out int count) ? count : 0;
        }

        public bool HasCard(string cardId, int amount = 1)
        {
            return GetCardCount(cardId) >= amount;
        }

        public bool HasCards(List<CardRequirement> requirements)
        {
            return requirements.All(req => HasCard(req.Card.CardId, req.Amount));
        }

        public void AddCard(string cardId, int amount = 1)
        {
            if (amount <= 0) return;

            if (!cardInventory.ContainsKey(cardId))
            {
                cardInventory[cardId] = 0;
            }

            CardData cardData = GetCardData(cardId);
            int maxStack = cardData != null ? cardData.MaxStack : 99;
            
            cardInventory[cardId] = Mathf.Min(cardInventory[cardId] + amount, maxStack);

            OnCardCountChanged?.Invoke(cardId, cardInventory[cardId]);
            OnInventoryChanged?.Invoke();

            EventManager.Instance?.TriggerEvent(GameEvents.CARD_PICKUP, cardId, amount);
        }

        public bool RemoveCard(string cardId, int amount = 1)
        {
            if (amount <= 0) return true;
            if (!HasCard(cardId, amount)) return false;

            cardInventory[cardId] -= amount;

            if (cardInventory[cardId] <= 0)
            {
                cardInventory.Remove(cardId);
            }

            OnCardCountChanged?.Invoke(cardId, GetCardCount(cardId));
            OnInventoryChanged?.Invoke();

            EventManager.Instance?.TriggerEvent(GameEvents.CARD_DROP, cardId, amount);
            return true;
        }

        public bool CraftCard(string cardId)
        {
            CardData cardData = GetCardData(cardId);
            if (cardData == null) return false;

            if (!HasCards(cardData.CraftRequirements)) return false;

            foreach (var req in cardData.CraftRequirements)
            {
                RemoveCard(req.Card.CardId, req.Amount);
            }

            AddCard(cardId, cardData.CraftOutputCount);

            EventManager.Instance?.TriggerEvent(GameEvents.CARD_CRAFT, cardId);
            return true;
        }

        public bool UseCard(string cardId, Vector2Int position)
        {
            CardData cardData = GetCardData(cardId);
            if (cardData == null) return false;
            if (!HasCard(cardId)) return false;

            if (cardData.Type == CardType.Building)
            {
                if (BuildingManager.Instance != null)
                {
                    bool placed = BuildingManager.Instance.PlaceBuilding(cardData, position);
                    if (placed)
                    {
                        RemoveCard(cardId, 1);
                        EventManager.Instance?.TriggerEvent(GameEvents.CARD_USE, cardId, position);
                        return true;
                    }
                }
            }

            return false;
        }

        public List<CardStack> GetAllCards()
        {
            List<CardStack> stacks = new List<CardStack>();
            foreach (var kvp in cardInventory)
            {
                CardData data = GetCardData(kvp.Key);
                if (data != null && kvp.Value > 0)
                {
                    stacks.Add(new CardStack { Card = data, Amount = kvp.Value });
                }
            }
            return stacks;
        }

        public List<CardStack> GetCardsByType(CardType type)
        {
            return GetAllCards().Where(s => s.Card.Type == type).ToList();
        }

        public List<CardData> GetCraftableCards()
        {
            return AllCards.Where(card => 
                card.CraftRequirements.Count > 0 && 
                HasCards(card.CraftRequirements)
            ).ToList();
        }

        public CardSaveData[] GetSaveData()
        {
            List<CardSaveData> saveData = new List<CardSaveData>();
            foreach (var kvp in cardInventory)
            {
                saveData.Add(new CardSaveData
                {
                    CardId = kvp.Key,
                    Amount = kvp.Value
                });
            }
            return saveData.ToArray();
        }

        public void ApplySaveData(CardSaveData[] saveData)
        {
            cardInventory.Clear();
            foreach (var data in saveData)
            {
                cardInventory[data.CardId] = data.Amount;
            }
            OnInventoryChanged?.Invoke();
        }
    }

    [Serializable]
    public class CardStack
    {
        public CardData Card;
        public int Amount;
    }
}
