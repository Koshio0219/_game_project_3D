using AYellowpaper.SerializedCollections;
using Game.Data;
using Game.Player;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Base
{
    public enum LanguageType
    {
        Chinese,
        English,
        Japanese
    }

    [System.Serializable]
    public struct PlayerWeaponData
    {
        public GameObject prefab;
        public WeaponHandType handType;
        public PlayerData weaponAddProp;
    }

    [System.Serializable]
    public struct PlayerConfig
    {
        public int initWeaponId;
        public int maxSwordPoint;
    }

    [System.Serializable]
    public struct PlayerStateConfig
    {
        public int priority;
        public float duration;
    }

    [System.Serializable]
    public struct HudConfig
    {
        public GameObject popupTextPrefab;
    }

    [Serializable]
    public struct LevelData
    {
        public Color backgroundColor;
    }

    [CreateAssetMenu(fileName = "GameData", menuName = "Scriptable Objects/GameData")]
    public class GameData : ScriptableObject
    {
        public List<LevelData> levelDatas;

        public PlayerConfig playerConfig;
        public SerializedDictionary<int, PlayerWeaponData> playerWeaponDatas;
        public SerializedDictionary<PlayerAnimatorState, PlayerStateConfig> playerStateConfigs;
        public HudConfig hudConfig;
        
    }
}
