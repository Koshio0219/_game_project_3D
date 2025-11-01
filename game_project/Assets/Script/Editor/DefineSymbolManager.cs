#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build;

namespace Game.Editor
{
    public class DefineSymbolManager : EditorWindow
    {
        // ==== 配置 ====
        private static readonly string[] DefaultPresets = new[]
        {
            "DEBUG_MODE", "DEVELOPMENT_BUILD", "USE_BD_TOOLBAR_FIX", "ENABLE_LOGGING", "PROFILING",
            "FEATURE_X", "FEATURE_Y"
        };

        private const string PREF_SEARCH = "DefineSymbolManager_Search";
        private const string PREF_GROUP = "DefineSymbolManager_Group";

        // ==== 状态 ====
        private string search = "";
        private BuildTargetGroup selectedGroup;
        private Vector2 scroll;
        private string addField = "";
        private string bulkField = "";

        private readonly HashSet<string> toggled = new(StringComparer.Ordinal); // UI 勾选状态
        private string loadedDefinesRaw = ""; // 从 PlayerSettings 读取的原始串（显示&对比用）

        [MenuItem("Tools/Define Symbol Manager %#m")] // Ctrl/Cmd + Shift + M
        public static void ShowWindow()
        {
            var win = GetWindow<DefineSymbolManager>();
            win.titleContent = new GUIContent("Define Symbol Manager");
            win.minSize = new Vector2(560, 420);
            win.Show();
        }

        private void OnEnable()
        {
            // 记住上次选择
            search = EditorPrefs.GetString(PREF_SEARCH, "");
            selectedGroup = (BuildTargetGroup)EditorPrefs.GetInt(PREF_GROUP, (int)EditorUserBuildSettings.selectedBuildTargetGroup);
            if (!IsValidGroup(selectedGroup))
                selectedGroup = EditorUserBuildSettings.selectedBuildTargetGroup;

            RefreshFromPlayerSettings();
        }

        private void OnDisable()
        {
            EditorPrefs.SetString(PREF_SEARCH, search);
            EditorPrefs.SetInt(PREF_GROUP, (int)selectedGroup);
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawToolbar();
            GUILayout.Space(6);
            DrawListArea();
            GUILayout.Space(8);
            DrawBulkArea();
            GUILayout.Space(8);
            DrawBottomButtons();
        }

        // ===== UI: 顶部信息 =====
        private void DrawHeader()
        {
            EditorGUILayout.HelpBox(
                "可视化管理 Scripting Define Symbols：勾选/添加/批量导入导出，并可一键应用到当前或全部目标平台。",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                var newGroup = (BuildTargetGroup)EditorGUILayout.EnumPopup(new GUIContent("Build Target Group"), selectedGroup);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("从 PlayerSettings 重新读取", GUILayout.Width(220)))
                    RefreshFromPlayerSettings();

                if (newGroup != selectedGroup)
                {
                    selectedGroup = newGroup;
                    RefreshFromPlayerSettings();
                }
            }
        }

        // ===== UI: 工具条 =====
        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                // 搜索
                var newSearch = GUILayout.TextField(search, GUI.skin.FindStyle("ToolbarSeachTextField") ?? EditorStyles.toolbarTextField, GUILayout.MinWidth(160));
                if (newSearch != search)
                {
                    search = newSearch;
                }

                if (GUILayout.Button("×", EditorStyles.toolbarButton, GUILayout.Width(24)))
                {
                    search = "";
                    GUI.FocusControl(null);
                }

                GUILayout.FlexibleSpace();

                // 预设标签快速添加
                foreach (var p in DefaultPresets)
                {
                    if (GUILayout.Button(new GUIContent(p, "添加预设宏"), EditorStyles.toolbarButton))
                    {
                        ToggleOn(p);
                    }
                }
            }
        }

        // ===== UI: 宏列表（勾选） =====
        private void DrawListArea()
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                // 添加单个
                using (new EditorGUILayout.HorizontalScope())
                {
                    addField = EditorGUILayout.TextField(new GUIContent("添加新宏 (单个)"), addField);
                    using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(addField)))
                    {
                        if (GUILayout.Button("添加", GUILayout.Width(80)))
                        {
                            ToggleOn(addField);
                            addField = "";
                        }
                    }
                }

                // 列表
                GUILayout.Space(4);
                EditorGUILayout.LabelField("宏列表（勾选=启用）", EditorStyles.boldLabel);

                var all = toggled.ToList();
                all.Sort(StringComparer.Ordinal);
                var filtered = string.IsNullOrEmpty(search)
                    ? all
                    : all.Where(s => s.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

                scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MinHeight(180));
                foreach (var s in filtered)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        bool on = true; // 在集合中即为启用
                        bool newOn = EditorGUILayout.ToggleLeft(s, on);
                        if (!newOn) Remove(s);

                        if (GUILayout.Button("复制", GUILayout.Width(56)))
                            EditorGUIUtility.systemCopyBuffer = s;

                        if (GUILayout.Button("移除", GUILayout.Width(56)))
                            Remove(s);
                    }
                }
                EditorGUILayout.EndScrollView();

                if (filtered.Count == 0)
                {
                    EditorGUILayout.HelpBox("没有匹配的宏。", MessageType.None);
                }
            }
        }

        // ===== UI: 批量导入/导出 =====
        private void DrawBulkArea()
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("批量宏（; 或 换行 分隔）", EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("从当前勾选导出到文本框"))
                        bulkField = string.Join(";", NormalizeList(toggled));

                    if (GUILayout.Button("复制到剪贴板"))
                        EditorGUIUtility.systemCopyBuffer = bulkField ?? "";

                    if (GUILayout.Button("从剪贴板粘贴"))
                        bulkField = EditorGUIUtility.systemCopyBuffer ?? "";
                }

                bulkField = EditorGUILayout.TextArea(bulkField, GUILayout.MinHeight(80));

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("合并到列表（不清空）"))
                    {
                        foreach (var s in ParseBulk(bulkField))
                            ToggleOn(s);
                    }

                    if (GUILayout.Button("用这些替换列表（清空后导入）"))
                    {
                        toggled.Clear();
                        foreach (var s in ParseBulk(bulkField))
                            ToggleOn(s);
                    }

                    if (GUILayout.Button("清空文本框"))
                        bulkField = "";
                }
            }
        }

        // ===== UI: 底部操作区 =====
        private void DrawBottomButtons()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("清理空项/去重/排序", GUILayout.Height(28)))
                    Cleanup();

                if (GUILayout.Button("还原为 PlayerSettings 中的值", GUILayout.Height(28)))
                    RefreshFromPlayerSettings();

                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledScope(toggled.Count == 0 && string.IsNullOrEmpty(loadedDefinesRaw)))
                {
                    if (GUILayout.Button("应用到当前 Group", GUILayout.Width(180), GUILayout.Height(28)))
                        ApplyToGroup(selectedGroup);

                    if (GUILayout.Button("应用到所有有效 Group", GUILayout.Width(220), GUILayout.Height(28)))
                        ApplyToAllGroups();
                }
            }

            // 底部信息
            GUILayout.Space(6);
            EditorGUILayout.LabelField("当前 PlayerSettings 值（只读）", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.SelectableLabel(loadedDefinesRaw, GUILayout.Height(32));
            }
        }

        // ===== 逻辑 =====

        private void RefreshFromPlayerSettings()
        {
            var from = GetDefinesFor(selectedGroup);
            loadedDefinesRaw = from;
            toggled.Clear();
            foreach (var s in ParseBulk(from))
                ToggleOn(s);

            Repaint();
        }

        private void ApplyToGroup(BuildTargetGroup group)
        {
            var str = string.Join(";", NormalizeList(toggled));
            SetDefinesFor(group, str);
            loadedDefinesRaw = str;

            Debug.Log($"[DefineSymbolManager] 已应用到 {group}: {str}");
            AssetDatabase.Refresh();
        }

        private void ApplyToAllGroups()
        {
            var groups = Enum.GetValues(typeof(BuildTargetGroup))
                .Cast<BuildTargetGroup>()
                .Where(IsValidGroup)
                .ToList();

            var str = string.Join(";", NormalizeList(toggled));
            foreach (var g in groups)
                SetDefinesFor(g, str);

            loadedDefinesRaw = str;
            Debug.Log($"[DefineSymbolManager] 已应用到所有有效 BuildTargetGroup，共 {groups.Count} 个。");
            AssetDatabase.Refresh();
        }

        private void Cleanup()
        {
            var cleaned = NormalizeList(toggled);
            toggled.Clear();
            foreach (var s in cleaned) toggled.Add(s);
        }

        private void ToggleOn(string define)
        {
            define = (define ?? "").Trim();
            if (string.IsNullOrEmpty(define)) return;
            if (define.Contains(" ")) define = define.Replace(" ", "");
            if (define.Contains(",")) define = define.Replace(",", ";");

            // 拆分用户误粘贴的分隔
            var parts = define.Split(new[] { ';', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
            {
                var t = p.Trim();
                if (!string.IsNullOrEmpty(t)) toggled.Add(t);
            }
        }

        private void Remove(string define)
        {
            if (string.IsNullOrEmpty(define)) return;
            toggled.Remove(define);
        }

        private static IEnumerable<string> ParseBulk(string bulk)
        {
            if (string.IsNullOrEmpty(bulk)) yield break;
            var parts = bulk.Split(new[] { ';', '\n', '\r', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
            {
                var t = p.Trim();
                if (!string.IsNullOrEmpty(t))
                    yield return t;
            }
        }

        private static List<string> NormalizeList(IEnumerable<string> src)
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            foreach (var s in src)
            {
                var t = (s ?? "").Trim();
                if (string.IsNullOrEmpty(t)) continue;
                // 去掉空白与多余分隔
                t = t.Replace(" ", "");
                set.Add(t);
            }
            var list = set.ToList();
            list.Sort(StringComparer.Ordinal);
            return list;
        }

        private static bool IsValidGroup(BuildTargetGroup g)
        {
            // 过滤掉 Unknown/Deprecated
            if (g == BuildTargetGroup.Unknown) return false;

            // 某些组在当前 Unity 版本可能不可用，尝试读取判断
            try
            {
                PlayerSettings.GetScriptingDefineSymbolsForGroup(g);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string GetDefinesFor(BuildTargetGroup g)
        {
            try
            {
                return PlayerSettings.GetScriptingDefineSymbolsForGroup(g) ?? "";
            }
            catch
            {
                return "";
            }
        }

        private static void SetDefinesFor(BuildTargetGroup g, string defines)
        {
            // 写入当前 BuildTargetGroup
            PlayerSettings.SetScriptingDefineSymbolsForGroup(g, defines ?? "");

            // 再尝试给 Editor 目标也写入（IDE 才会立刻识别宏，避免灰色）
#if UNITY_EDITOR
            TrySetEditorNamedBuildTarget(defines ?? "");
#endif
        }

#if UNITY_EDITOR
        private static void TrySetEditorNamedBuildTarget(string defines)
        {
            try
            {
                // 反射兜底：即使没有 NamedBuildTarget.Editor 也调用同名方法
                var unityEditorAsm = typeof(PlayerSettings).Assembly;
                var nbtType = unityEditorAsm.GetType("UnityEditor.Build.NamedBuildTarget");
                if (nbtType == null) return;

                var editorField = nbtType.GetField("Editor",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (editorField == null) return;

                var editorValue = editorField.GetValue(null);
                var method = typeof(PlayerSettings).GetMethods()
                    .FirstOrDefault(m =>
                        m.Name == "SetScriptingDefineSymbols" &&
                        m.GetParameters().Length == 2 &&
                        m.GetParameters()[0].ParameterType.FullName == nbtType.FullName);

                method?.Invoke(null, new object[] { editorValue, defines });
            }
            catch { /* 忽略：老版本没有该 API 时安全失败 */ }
        }
#endif


    }
#endif
}
