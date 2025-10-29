using UnityEngine;
using Game.Player;
using System.Collections.Generic;

namespace Game.Base
{
    [System.Serializable]
    public struct PlayerWeaponData
    {
        public int weaponId;
        public GameObject prefab;
        public PlayerData weaponAddProp;
    }

    [System.Serializable]
    public struct PlayerConfig
    {
        public int initWeaponId;
    }

    [CreateAssetMenu(fileName = "GameData", menuName = "Scriptable Objects/GameData")]
    public class GameData : ScriptableObject
    {
        public PlayerConfig playerConfig;
        public List<PlayerWeaponData> playerWeaponDatas;
    }
}
