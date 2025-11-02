using AYellowpaper.SerializedCollections;
using Game.Base;
using Game.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Player
{
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(PlayerStateHandler))]
    public class PlayerAttackCtrl:MonoBehaviour
    {
        private int? _insId = null;
        public int InsId
        {
            get{
                _insId ??= gameObject.GetInstanceID();
                return _insId.Value;
            }
        }

        public PlayerPropManager PropManager { get;private set; }

        [Header("Weapon")]
        public SerializedDictionary<WeaponHandType, Transform> weaponTransform;

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
            GameManager.stageManager.AddOnePlayer(InsId, gameObject);
            EnterLevel();
        }

        public void EnterLevel()
        {
            PropManager = new PlayerPropManager(new PlayerData(GameManager.Instance.gameData.playerConfig.maxSwordPoint));

            EquipWeapon(GameManager.Instance.gameData.playerConfig.initWeaponId);
            EventQueueSystem.QueueEvent(new PlayerEnterLevelEvent(PropManager.Prop));
        }

        public void EquipWeapon(int weaponId)
        {
            if (currentWeapon != null) return;
            var data = GameManager.Instance.gameData.playerWeaponDatas[weaponId];

            currentWeapon = GameObjectPool.Instance.GetObj(data.prefab, weaponTransform[data.handType]).GetComponent<Weapon>();
            currentWeapon.transform.ResetLocal();
            currentWeapon.InitWeapon(weaponId);
            PropManager.AddProp(currentWeapon.AddProp);
        }

        private void Update()
        {
            if (currentWeapon == null) return;

            if (input.AttackPressed)
            {
                currentWeapon.NormalAttack();
                stateHandler.State = PlayerAnimatorState.Attack;
            }
        }

        public void ExitLevel()
        {

        }
    }
}
