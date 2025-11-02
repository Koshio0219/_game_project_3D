using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
#if UNITY_EDITOR
    using System.IO;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;


    
        [InitializeOnLoad]
        public static class PlayFromStartScene
        {
            // 你希望的起始场景名
            private const string StartSceneName = "Start";

            // 用 SessionState 来跨域重载/进出PlayMode保存临时状态
            private const string KeyPrevScene = "PFS__PrevScenePath";
            private const string KeyDidSwitch = "PFS__DidSwitchToStart";
            private const string KeyShouldReturn = "PFS__ShouldReturnAfterPlay";

            static PlayFromStartScene()
            {
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            }

            [MenuItem("Tools/Play From Start Scene %#&P", priority = 10)]
            public static void PlayFromStart()
            {
                // 如果已经在播放，等同于停止
                if (EditorApplication.isPlaying)
                {
                    EditorApplication.isPlaying = false;
                    return;
                }

                // 记录当前场景并提示保存
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    return;

                Scene active = SceneManager.GetActiveScene();
                if (!active.IsValid() || string.IsNullOrEmpty(active.path))
                {
                    EditorUtility.DisplayDialog("Play From Start Scene", "当前场景无效或未保存，请先保存场景。", "OK");
                    return;
                }

                string prevScenePath = active.path;
                SessionState.SetString(KeyPrevScene, prevScenePath);

                // 找到 Start 场景路径
                string startPath = GetStartScenePath();
                if (string.IsNullOrEmpty(startPath))
                {
                    EditorUtility.DisplayDialog(
                        "Play From Start Scene",
                        $"未找到名为 \"{StartSceneName}\" 的场景。\n" +
                        $"请在 Build Settings 中添加，或在工程中创建一个名为 {StartSceneName}.unity 的场景。",
                        "OK"
                    );
                    // 清理状态，防止误恢复
                    SessionState.EraseString(KeyPrevScene);
                    return;
                }

                // 打开 Start 场景并标记“需要恢复”
                var scene = EditorSceneManager.OpenScene(startPath, OpenSceneMode.Single);
                if (!scene.IsValid())
                {
                    EditorUtility.DisplayDialog("Play From Start Scene", $"无法打开场景：\n{startPath}", "OK");
                    SessionState.EraseString(KeyPrevScene);
                    return;
                }

                SessionState.SetBool(KeyDidSwitch, true);
                SessionState.SetBool(KeyShouldReturn, true);

                // 进入播放
                EditorApplication.isPlaying = true;
            }

            private static void OnPlayModeStateChanged(PlayModeStateChange state)
            {
                switch (state)
                {
                    case PlayModeStateChange.ExitingPlayMode:
                        // 将要退出播放：标记应该恢复
                        if (SessionState.GetBool(KeyDidSwitch, false))
                            SessionState.SetBool(KeyShouldReturn, true);
                        break;

                    case PlayModeStateChange.EnteredEditMode:
                        // 已回到编辑模式：如果是我们切的场景，就恢复
                        if (!SessionState.GetBool(KeyShouldReturn, false) ||
                            !SessionState.GetBool(KeyDidSwitch, false))
                            return;

                        SessionState.SetBool(KeyShouldReturn, false);

                        string prev = SessionState.GetString(KeyPrevScene, string.Empty);
                        if (!string.IsNullOrEmpty(prev) && File.Exists(prev))
                        {
                            // 打开之前的场景（不需要再次提示保存，因为我们刚从 Play 退出）
                            EditorSceneManager.OpenScene(prev, OpenSceneMode.Single);
                        }

                        // 清理状态
                        SessionState.EraseString(KeyPrevScene);
                        SessionState.SetBool(KeyDidSwitch, false);
                        break;
                }
            }

            /// <summary>
            /// 优先在 Build Settings 中找名为 Start 的已启用场景；找不到则全工程搜索。
            /// </summary>
            private static string GetStartScenePath()
            {
                // 1) 先在 Build Settings 中找
                foreach (var s in EditorBuildSettings.scenes)
                {
                    if (!s.enabled) continue;
                    var name = Path.GetFileNameWithoutExtension(s.path);
                    if (name == StartSceneName && File.Exists(s.path))
                        return s.path;
                }

                // 2) 全工程搜索 .unity 资源并匹配精确名称
                string[] guids = AssetDatabase.FindAssets("t:Scene " + StartSceneName);
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (Path.GetFileNameWithoutExtension(path) == StartSceneName && File.Exists(path))
                        return path;
                }

                // 3) 兜底：返回 Build Settings 的第一个已启用场景
                foreach (var s in EditorBuildSettings.scenes)
                {
                    if (s.enabled && File.Exists(s.path))
                        return s.path;
                }

                return null;
            }
        
    }
#endif

}