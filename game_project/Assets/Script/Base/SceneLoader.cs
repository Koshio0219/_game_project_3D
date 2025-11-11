using Cysharp.Threading.Tasks;
using Game.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Base
{
    public class SceneLoader : Singleton<SceneLoader>
    {
        private float progress = 0f;
        private bool isLoading = false;

        private async UniTaskVoid OnClickLoadScene(string toScene)
        {
            isLoading = true;

            EventQueueSystem.QueueEvent(new SceneLoadStartEvent());
            await Resources.UnloadUnusedAssets();
            System.GC.Collect();

            await SceneManager.LoadSceneAsync(toScene, LoadSceneMode.Single).ToUniTask(Progress.CreateOnlyValueChanged<float>(p =>
            {
                progress = p;
                EventQueueSystem.QueueEvent(new SceneLoadProgressChangeEvent(progress));

                Debug.Log($"current scene loding progress is {progress * 100:F2}%");
            }));


            EventQueueSystem.QueueEvent(new SceneLoadFinishedEvent());
            isLoading = false;

            progress = 0f;
        }

        public void BackToMenu()
        {
            if (isLoading) return;
            OnClickLoadScene("Start").Forget();
            GameManager.Instance.UnlockCursor();
        }

        public void GoToReady()
        {
            if (isLoading) return;
            OnClickLoadScene("Ready").Forget();
            GameManager.Instance.UnlockCursor();
        }

        public void GoToStage()
        {
            if (isLoading) return;
            OnClickLoadScene("Stage").Forget();
            GameManager.Instance.LockCursor();
        }
    }
}
