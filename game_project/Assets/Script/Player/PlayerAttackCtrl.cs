using System.Collections.Generic;
using UnityEngine;

namespace Game.Player
{
    [System.Serializable]
    public struct WeaponHandTypeToTransform
    {
        public WeaponHandType handType;
        public Transform transform;
    }

    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(PlayerStateHandler))]
    public class PlayerAttackCtrl:MonoBehaviour
    {
        public PlayerPropManager PropManager { get;private set; }

        [Header("Weapon")]
        public List<WeaponHandTypeToTransform> weaponTransform;

        private Weapon currentWeapon = null;

        private PlayerInputHandler input;
        private PlayerStateHandler stateHandler;

        private void Awake()
        { 
            input = GetComponent<PlayerInputHandler>();
            stateHandler = GetComponent<PlayerStateHandler>();
        }

        private void Start()
        {
            EnterLevel();
        }

        public void EnterLevel()
        {
            PropManager = new PlayerPropManager(new PlayerData());

            EquipWeapon(1);
        }

        public void EquipWeapon(int weaponId)
        {
            if (currentWeapon != null) return;
        }

        public void ExitLevel()
        {

        }
    }
}
