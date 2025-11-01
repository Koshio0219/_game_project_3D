using System.IO;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public class ScreenCaptureEditor : EditorWindow
    {
        private static string directory = "Screenshots/Capture/";
        private static string latestScreenshotPath = "";
        private bool initDone = false;

        private GUIStyle BigText;

        void InitStyles()
        {
            initDone = true;
            BigText = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold
            };
        }

        private void OnGUI()
        {
            if (!initDone)
            {
                InitStyles();
            }

            GUILayout.Label("Screen Capture Tools", BigText);
            if (GUILayout.Button("Single Capture"))
            {
                TakeScreenshot();
            }
            GUILayout.Label("Current Resolution： " + GetResolution());

            if (GUILayout.Button("Open Folder"))
            {
                ShowFolder();
            }
            GUILayout.Label("Save Path: " + directory);
        }

        [MenuItem("Tools/Screenshots/Open Window &`", false, 0)]
        public static void ShowWindow()
        {
            GetWindow(typeof(ScreenCaptureEditor));
        }

        [MenuItem("Tools/Screenshots/Save Path &2", false, 2)]
        private static void ShowFolder()
        {
            if (File.Exists(latestScreenshotPath))
            {
                EditorUtility.RevealInFinder(latestScreenshotPath);
                return;
            }
            Directory.CreateDirectory(directory);
            EditorUtility.RevealInFinder(directory);
        }

        [MenuItem("Tools/Screenshots/Single Capture &1", false, 1)]
        private static void TakeScreenshot()
        {
            Directory.CreateDirectory(directory);
            var currentTime = System.DateTime.Now;
            var filename = currentTime.ToString().Replace('/', '-').Replace(':', '_') + ".png";
            var path = directory + filename;
            ScreenCapture.CaptureScreenshot(path);
            latestScreenshotPath = path;
            Debug.Log($"Screen Capture Path: <b>{path}</b> Resolution： <b>{GetResolution()}</b>");
        }

        private static string GetResolution()
        {
            Vector2 size = Handles.GetMainGameViewSize();
            Vector2Int sizeInt = new Vector2Int((int)size.x, (int)size.y);
            return $"{sizeInt.x}x{sizeInt.y}";
        }

    }
}
