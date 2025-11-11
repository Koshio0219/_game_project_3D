using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Data;
using Game.Framework;
using Game.Navigation;
using Game.Player;
using KanKikuchi.AudioManager;
using System.Threading;
using UnityEngine;

namespace Game.Base
{
    public class GameManager : MonoSingleton<GameManager>
    {
        public GameData gameData;
        public EnemyCreateConfig enemyCreateConfig;

        private CancellationToken token = CancellationToken.None;
        public CancellationToken CancelTokenOnGameDestroy
        {
            get
            {
                if (token == CancellationToken.None)
                {
                    token = this.GetCancellationTokenOnDestroy();
                }
                return token;
            }
        }

        public static StageManager stageManager = null;
        public static PointManager pointManager = null;
        public static RuntimeNavMeshRebuildController runtimeNavMeshRebuildController = null;

        private int levelIdx;
        public int LevelIdx
        {
            get
            {
                return levelIdx;
            }
            set
            {
                levelIdx = value;
            }
        }

        private int curLevelTime;

        public int CurLevelTime
        {
            get
            {
                return curLevelTime;
            }
            set
            {
                curLevelTime = value;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            SetFrameRate(60);
            SetFixedDeltaTime(0.02f);
            SetStartAudioVolume(.5f);

            EventQueueSystem.AddListener<SceneLoadStartEvent>(SceneLoadStartHandler);
        }

        public void LockCursor()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void UnlockCursor()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void SceneLoadStartHandler(SceneLoadStartEvent e)
        {
            Debug.Log("change scene started!");
            DOTween.KillAll();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventQueueSystem.RemoveListener<SceneLoadStartEvent>(SceneLoadStartHandler);
        }

        internal void SetFrameRate(int frameRate)
        {
            Application.targetFrameRate = frameRate;
            QualitySettings.vSyncCount = 1;
        }

        internal void SetFixedDeltaTime(float deltaTime)
        {
            Time.fixedDeltaTime = deltaTime;
        }

        internal void SetStartAudioVolume(float volume)
        {
            BGMManager.Instance.ChangeBaseVolume(volume);
            SEManager.Instance.ChangeBaseVolume(volume);
        }

        public void OpenHomePage(string url = "https://gamepit.tokyo/#top")
        {
            Application.OpenURL(url);
        }
    }
}
