using Game.Data;
using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
    [System.Serializable]
    public struct EnemyCreateData
    {
        public EnemyTypeID typeID;
        public EnemyUnitData unitData;
        //public AssetReference assetReference;
    }

    [System.Serializable]
    public struct LevelEnemyPos
    {
        public EnemyTypeID typeID;
        public Vector3Int pos;
        public bool randomOnBlock;
    }

    [System.Serializable]
    public struct LevelEnemyData
    {
        public List<LevelEnemyPos> enemies;
    }

    [CreateAssetMenu(fileName = "EnemyCreateConfig", menuName = "Scriptable Objects/EnemyCreateConfig")]
    public class EnemyCreateConfig : ScriptableObject
    {
        [SerializeField] private List<EnemyCreateData> EnemyConfig;
        public List<LevelEnemyData> levelEnemyData;

        private readonly Dictionary<EnemyTypeID, EnemyCreateData> mapEnemyTypeIDToData = new();
        public Dictionary<EnemyTypeID,EnemyCreateData> MapEnemyTypeIDToData
        {
            get
            {
                if(mapEnemyTypeIDToData.Count == 0)
                {
                    foreach(var item in EnemyConfig)
                    {
                        mapEnemyTypeIDToData.AddOrSet(item.typeID, item);
                    }
                }
                return mapEnemyTypeIDToData;
            }
        }
    }
}
