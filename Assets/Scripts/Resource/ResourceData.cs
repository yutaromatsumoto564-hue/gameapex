using UnityEngine;
using System.Collections.Generic;

namespace ARIA.Resource
{
    [CreateAssetMenu(fileName = "NewResourceData", menuName = "ARIA/Resource Data")]
    public class ResourceData : ScriptableObject
    {
        [Header("Basic Info")]
        public string ResourceId;
        public string ResourceName;
        public string Description;
        public Sprite Icon;

        [Header("Category")]
        public ResourceCategory Category;
        public int Tier;

        [Header("Value")]
        public int BaseValue;
    }

    public enum ResourceCategory
    {
        Raw,
        Basic,
        Intermediate,
        Advanced,
        Ultimate,
        Organic
    }
}
