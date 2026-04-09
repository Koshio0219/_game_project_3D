using Cysharp.Threading.Tasks;
using Game.Base;
using Game.Framework;
using Game.Hud;
using System;
using System.Collections;
using UnityEngine;

namespace Game.Hud
{
    [RequireComponent(typeof(EnemyHpRingView))]
    public class EnemyHpRingCtrl : MonoBehaviour
    {
        public float hpHeight = 1f;
        private EnemyHpRingView view = null;
        public EnemyHpRingView View
        {
            get
            {
                if (view == null)
                {
                    view = GetComponent<EnemyHpRingView>();
                }
                return view;
            }
        }
        //public HudConfig Mode => GameData.Instance.HudConfig;

        private GameObject rootObj = null;
        private GameObject RootObj
        {
            get
            {
                if (rootObj == null)
                {
                    var up = transform.GetRootParent();
                    rootObj = up.gameObject;
                    //detach the hpui with enemy
                    Detach();
                }
                return rootObj;
            }
        }

        private void Awake()
        {
            EventQueueSystem.AddListener<InitEnemyHpEvent>(InitEnemyHpHandler);
            EventQueueSystem.AddListener<StageStatesEvent>(StageStatesHandler);
        }

        private void StageStatesHandler(StageStatesEvent e)
        {
            switch (e.to)
            {
                case StageStates.GameOver:
                case StageStates.BattleClear:
                    Destroy(gameObject);
                    break;
            }
        }

        private void InitEnemyHpHandler(InitEnemyHpEvent e)
        {
            var target = GameManager.stageManager.GetEnemy(e.enemyId).gameObject;
            if (RootObj != target) return;
            View.InitHpView(e.hp);
        }

        private void OnEnable()
        {
            EventQueueSystem.AddListener<EnemyHpChangeEvent>(EnemyHpChangeHandler);
        }

        private void EnemyHpChangeHandler(EnemyHpChangeEvent e)
        {
            if (RootObj != GameManager.stageManager.GetEnemy(e.enemyId).gameObject) return;
            View.UpdateHpView(e.lastHp, e.nowHp);
        }

        private void OnDisable()
        {
            EventQueueSystem.RemoveListener<EnemyHpChangeEvent>(EnemyHpChangeHandler);
            ResetData();
        }

        private void OnDestroy()
        {
            EventQueueSystem.RemoveListener<InitEnemyHpEvent>(InitEnemyHpHandler);
            EventQueueSystem.RemoveListener<StageStatesEvent>(StageStatesHandler);
            ResetData();
        }

        private void ResetData()
        {
            view = null;
            rootObj = null;
        }

        private void Detach()
        {
            var par = transform.parent;
            transform.SetParent(null);
            UniTask.Void(async () =>
            {
                while (par != null && this && isActiveAndEnabled)
                {
                    transform.position = par.position + new Vector3(0f, hpHeight, 0f);
                    await UniTask.DelayFrame(1, PlayerLoopTiming.PreLateUpdate);
                }
            });
        }
    }
}