using UnityEngine;
using System;
using System.Collections.Generic;
using ARIA.Building;

namespace ARIA.Card
{
    [CreateAssetMenu(fileName = "NewCardData", menuName = "ARIA/Card Data")]
    public class CardData : ScriptableObject
    {
        [Header("Basic Info")]
        public string CardId;
        public string CardName;
        public string Description;
        public Sprite Icon;
        
        [Header("Card Type")]
        public CardType Type;
        public CardRarity Rarity;
        
        [Header("Stacking")]
        public bool CanStack = true;
        public int MaxStack = 99;
        
        [Header("Building Info (if Type == Building)")]
        public BuildingData BuildingData;
        public int BuildingSizeX = 1;
        public int BuildingSizeY = 1;
        
        [Header("Crafting Recipe")]
        public List<CardRequirement> CraftRequirements = new List<CardRequirement>();
        public int CraftOutputCount = 1;
        
        [Header("Value")]
        public int SellPrice;
        public int BuyPrice;
    }

    [Serializable]
    public class CardRequirement
    {
        public CardData Card;
        public int Amount;
    }

    public enum CardType
    {
        Resource,
        Material,
        Building,
        Special
    }

    public enum CardRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}
