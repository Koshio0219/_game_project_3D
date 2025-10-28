using System.Collections.Generic;
using UnityEngine;

namespace Game.Player
{
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(PlayerStateHandler))]
    public class PlayerAttackCtrl:MonoBehaviour
    {
        public PlayerPropManager PropManager { get;private set; }

        [Header("Weapon")]
        public List<(WeaponHandType, Transform)> weaponTransform;

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

        }

        public void EquippedWeapon(Weapon weapon)
        {

        }

        public void ExitLevel()
        {

        }
    }
}
