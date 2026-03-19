using UnityEngine;
using System;
using System.Collections.Generic;
using ARIA.Core;
using ARIA.Building;

namespace ARIA.Resource
{
    public class ResourceNetwork : MonoBehaviour
    {
        public static ResourceNetwork Instance { get; private set; }

        [Header("Network Settings")]
        public int BaseNetworkRange = 10;
        public int CommandCenterRange = 15;

        private List<Building.Building> connectedBuildings = new List<Building.Building>();
        private List<Building.Building> networkHubs = new List<Building.Building>();
        private Dictionary<int, List<Building.Building>> networkGroups = new Dictionary<int, List<Building.Building>>();

        public event Action OnNetworkChanged;

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

        public void RegisterBuilding(Building.Building building)
        {
            if (building == null) return;
            if (connectedBuildings.Contains(building)) return;

            connectedBuildings.Add(building);

            if (building.Data.Category == BuildingCategory.Storage || 
                building.Data.BuildingId == "building_command_center")
            {
                networkHubs.Add(building);
            }

            RecalculateNetwork();
            OnNetworkChanged?.Invoke();
            EventManager.Instance?.TriggerEvent(GameEvents.RESOURCE_NETWORK_CHANGED);
        }

        public void UnregisterBuilding(Building.Building building)
        {
            if (building == null) return;

            connectedBuildings.Remove(building);
            networkHubs.Remove(building);

            RecalculateNetwork();
            OnNetworkChanged?.Invoke();
            EventManager.Instance?.TriggerEvent(GameEvents.RESOURCE_NETWORK_CHANGED);
        }

        private void RecalculateNetwork()
        {
            networkGroups.Clear();

            foreach (var hub in networkHubs)
            {
                int groupId = hub.InstanceId;
                networkGroups[groupId] = new List<Building.Building>();
                networkGroups[groupId].Add(hub);

                foreach (var building in connectedBuildings)
                {
                    if (building == hub) continue;

                    float distance = Vector2.Distance(
                        hub.GridPosition,
                        building.GridPosition
                    );

                    int range = GetEffectiveRange(hub);
                    if (distance <= range)
                    {
                        networkGroups[groupId].Add(building);
                    }
                }
            }

            MergeOverlappingGroups();
        }

        private void MergeOverlappingGroups()
        {
            bool merged = true;
            while (merged)
            {
                merged = false;
                var groupIds = new List<int>(networkGroups.Keys);

                for (int i = 0; i < groupIds.Count && !merged; i++)
                {
                    for (int j = i + 1; j < groupIds.Count && !merged; j++)
                    {
                        if (GroupsOverlap(networkGroups[groupIds[i]], networkGroups[groupIds[j]]))
                        {
                            networkGroups[groupIds[i]].AddRange(networkGroups[groupIds[j]]);
                            networkGroups.Remove(groupIds[j]);
                            merged = true;
                        }
                    }
                }
            }
        }

        private bool GroupsOverlap(List<Building.Building> group1, List<Building.Building> group2)
        {
            foreach (var b1 in group1)
            {
                foreach (var b2 in group2)
                {
                    if (b1 == b2) return true;
                }
            }
            return false;
        }

        private int GetEffectiveRange(Building.Building building)
        {
            if (building.Data.BuildingId == "building_command_center")
            {
                return CommandCenterRange;
            }
            return Mathf.Max(building.Data.NetworkRange, BaseNetworkRange);
        }

        public bool IsBuildingConnected(Building.Building building)
        {
            foreach (var group in networkGroups.Values)
            {
                if (group.Contains(building))
                {
                    return true;
                }
            }
            return false;
        }

        public List<Building.Building> GetConnectedBuildings(Building.Building building)
        {
            foreach (var group in networkGroups.Values)
            {
                if (group.Contains(building))
                {
                    return new List<Building.Building>(group);
                }
            }
            return new List<Building.Building>();
        }

        public int GetNetworkCount()
        {
            return networkGroups.Count;
        }

        public int GetConnectedBuildingCount()
        {
            return connectedBuildings.Count;
        }

        public List<Building.Building> GetAllConnectedBuildings()
        {
            return new List<Building.Building>(connectedBuildings);
        }

        public void DrawNetworkGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.3f);

            foreach (var hub in networkHubs)
            {
                int range = GetEffectiveRange(hub);
                Gizmos.DrawWireSphere(hub.transform.position, range);
            }

            Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.5f);

            foreach (var group in networkGroups.Values)
            {
                for (int i = 0; i < group.Count; i++)
                {
                    for (int j = i + 1; j < group.Count; j++)
                    {
                        Gizmos.DrawLine(group[i].transform.position, group[j].transform.position);
                    }
                }
            }
        }
    }
}
