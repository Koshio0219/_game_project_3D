using Cysharp.Threading.Tasks;
using DG.Tweening.Core.Easing;
using Game.Framework;
using Game.Unit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using KanKikuchi.AudioManager;
using System.Linq;
using Game.Player;
using UnityEngine.InputSystem.LowLevel;
using Game.Soul;

namespace Game.Base
{
    public enum StageStates
    {
        GetReady,
        EnemyBuildStart,
        EnemyBuildEnd,
        BattleStarted,
        BattleClear,//win
        GameOver//lose
    }

    public class StageManager : MonoBehaviour
    {
        private StageStates stageState = StageStates.GetReady;
        public StageStates StageState { get => stageState; private set => stageState = value; }

        private readonly Dictionary<StageStates, UnityAction> mapStateToEvent = new();

        private Dictionary<int, GameObject> MapPlayerIdToInstance { get; set; } = new();
        private Dictionary<int, Enemy> MapEnemyIdToInstance { get; set; } = new();

        private void Awake()
        {
            GameManager.stageManager = this;
            GameManager.pointManager = new PointManager();
            EventQueueSystem.AddListener<StageStatesEvent>(StageStatesHandler);
            InitTable();
        }

        private void OnDestroy()
        {
            GameManager.stageManager = null;
            GameManager.pointManager = null;
            EventQueueSystem.RemoveListener<StageStatesEvent>(StageStatesHandler);
        }

        private void InitTable()
        {
            if (mapStateToEvent.Count > 0) return;
            mapStateToEvent.Add(StageStates.EnemyBuildEnd, EnemyBuildEndHandler);
            mapStateToEvent.Add(StageStates.BattleClear, BattleClearEndHandler);
            mapStateToEvent.Add(StageStates.GameOver, GameOverHandler);
        }

        private async void GameOverHandler()
        {
            //lose
            //BGMSwitcher.FadeOutAndFadeIn(BGMPath.BGMGAME_OVER);
            //...something else...
            Debug.Log($"Game Over!");
            ClearAllEnemies();
            ClearAllPlayers();

            await UniTask.Delay(1000);
            PlayerPropManager.Instance.ResetProp();
            SwordSoulManager.Instance.ResetUsed();
            GameManager.Instance.LevelIdx = 0;
            this.WaitInput(PlayerInputHandler.Instance.InputActions.Player.Attack,()=> { SceneLoader.Instance.BackToMenu(); SEManager.Instance.Stop();});
        }

        private void BattleClearEndHandler()
        {
            Debug.Log($"battle clear !");
            ClearAllEnemies();
            SwordSoulManager.Instance.ResetUsed();

            if (IsLastStage())
            {
                Win();
            }
            else
            {
                NextStage();
            }
        }

        private void EnemyBuildEndHandler()
        {
            GameManager.runtimeNavMeshRebuildController.mapEnemyIdToInstance= MapEnemyIdToInstance;
            SendStateEvent(StageStates.BattleStarted);
        }

        private void StageStatesHandler(StageStatesEvent e)
        {
            if (mapStateToEvent.Count == 0) return;
            if (!mapStateToEvent.ContainsKey(e.to)) return;
            if (StageState == e.to) return;
            mapStateToEvent[e.to].Invoke();
            StageState = e.to;
        }

        private void Start()
        {
            StageState = StageStates.GetReady;
            SendStateEvent(StageStates.EnemyBuildStart);
        }

        private void SendStateEvent(StageStates state)
        {
            EventQueueSystem.QueueEvent(new StageStatesEvent(state));
            StageState = state;
            Debug.Log($"current stage state is :{state}");
        }

        public bool IsLastStage()
        {
            return GameManager.Instance.LevelIdx >= GameManager.Instance.enemyCreateConfig.levelEnemyData.Count - 1;
        }

        private async void NextStage()
        {
            //...wait ui show...
            BGMSwitcher.FadeOutAndFadeIn(BGMPath.BGMNEXT_STAGE);
            SEManager.Instance.Play(SEPath.SENEXT_STAGE);
            await UniTask.Delay(1000);
            GameManager.Instance.LevelIdx++;
            Debug.Log($"next stage! current level idx is {GameManager.Instance.LevelIdx}");
            this.WaitInput(PlayerInputHandler.Instance.InputActions.Player.Attack, () => { SceneLoader.Instance.GoToReady(); SEManager.Instance.Stop(); });
        }

        private async void Win()
        {
            Debug.Log($"game win !");
            BGMSwitcher.FadeOutAndFadeIn(BGMPath.BGMWIN);
            SEManager.Instance.Play(SEPath.SEWIN);
            // wait ui show
            await UniTask.Delay(1000);
            GameManager.Instance.LevelIdx = 0;
            PlayerPropManager.Instance.ResetProp();
            this.WaitInput(PlayerInputHandler.Instance.InputActions.Player.Attack, () => { SceneLoader.Instance.BackToMenu(); SEManager.Instance.Stop(); });
        }

        public void AddOnePlayer(int playerId, GameObject playerIns)
        {
            if (MapPlayerIdToInstance.ContainsKey(playerId)) return;
            MapPlayerIdToInstance.Add(playerId, playerIns);
        }

        public void RemoveOnePlayer(int playerId, bool bDestroy = true)
        {
            if (!MapPlayerIdToInstance.ContainsKey(playerId)) return;
            if (bDestroy) Destroy(MapPlayerIdToInstance[playerId]);
            MapPlayerIdToInstance.Remove(playerId);
        }

        public void ClearAllPlayers()
        {
            foreach (var item in MapPlayerIdToInstance)
            {
                Destroy(item.Value);
            }
            MapPlayerIdToInstance.Clear();
        }

        public void AddOneEnemy(int enemyId, Enemy enemy)
        {
            if (MapEnemyIdToInstance.ContainsKey(enemyId)) return;
            MapEnemyIdToInstance.Add(enemyId, enemy);
        }

        public void RemoveOneEnemy(int enemyId, bool bDestroy = true)
        {
            if (!MapEnemyIdToInstance.ContainsKey(enemyId)) return;
            if (bDestroy) Destroy(MapEnemyIdToInstance[enemyId].gameObject);
            MapEnemyIdToInstance.Remove(enemyId);
            //game clear 
            if (MapEnemyIdToInstance.Count == 0)
                EventQueueSystem.QueueEvent(new StageStatesEvent(StageStates.BattleClear));
        }

        public void ClearAllEnemies()
        {
            foreach (var item in MapEnemyIdToInstance)
            {
                Destroy(item.Value.gameObject);
            }
            MapEnemyIdToInstance.Clear();
        }

        public GameObject GetPlayer(int playerId)
        {
            if (!MapPlayerIdToInstance.ContainsKey(playerId)) return null;
            return MapPlayerIdToInstance[playerId];
        }

        public Enemy GetEnemy(int enemyId)
        {
            if (!MapEnemyIdToInstance.ContainsKey(enemyId)) return null;
            return MapEnemyIdToInstance[enemyId];
        }

        public Enemy FindCloseEnemy(Vector3 pos)
        {
            if (MapEnemyIdToInstance.Count == 0) return null;
            var result = MapEnemyIdToInstance[MapEnemyIdToInstance.Index(0)];
            var offse = pos - result.transform.position;
            var dis = Vector3.SqrMagnitude(offse);
            for (int i = 1; i < MapEnemyIdToInstance.Count; i++)
            {
                var one = MapEnemyIdToInstance[MapEnemyIdToInstance.Index(i)];
                var temOffse = pos - one.transform.position;
                var temDis = Vector3.SqrMagnitude(temOffse);
                if (temDis < dis)
                {
                    dis = temDis;
                    result = one;
                }
            }
            return result;
        }

        public int MatchPlayerId(GameObject target)
        {
            foreach (var item in MapPlayerIdToInstance)
            {
                if (target.GetInstanceID() == item.Value.GetInstanceID())
                {
                    return item.Key;
                }
            }

            Debug.Log($"match player failure! target name :{target.name}");
            return -1;
        }

        public bool IsFriend(int id1, int id2)
        {
            return (MapPlayerIdToInstance.ContainsKey(id1) && MapPlayerIdToInstance.ContainsKey(id2)) || (MapEnemyIdToInstance.ContainsKey(id1) && MapEnemyIdToInstance.ContainsKey(id2));
        }

        public List<GameObject> GetAllPlayer() => MapPlayerIdToInstance.Values.ToList();
    }
}