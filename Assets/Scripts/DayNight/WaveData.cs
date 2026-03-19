using UnityEngine;
using System.Collections.Generic;
using ARIA.Core;

namespace ARIA.DayNight
{
    [CreateAssetMenu(fileName = "NewWaveData", menuName = "ARIA/Wave Data")]
    public class WaveData : ScriptableObject
    {
        [Header("Wave Info")]
        public int WaveNumber;
        public int DayNumber;

        [Header("Timing")]
        public float WaveDelay = 1f;
        public float SpawnInterval = 0.5f;

        [Header("Rewards")]
        public int RewardGold;
        public List<string> RewardCardIds;
    }
}
