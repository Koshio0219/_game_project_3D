#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using HarmonyLib;
using System;
using System.Reflection;

[InitializeOnLoad]
public static class BehaviorDesignerToolbarStyleFix
{
    static BehaviorDesignerToolbarStyleFix()
    {
        try
        {
            var harmony = new Harmony("fix.behaviordesigner.toolbarstyle");
            var type = AccessTools.TypeByName("BehaviorDesigner.Editor.TaskList");
            if (type == null)
            {
                Debug.LogWarning("[BD Fix] TaskList type not found, skip patch.");
                return;
            }

            foreach (var m in type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if (m.Name is "DrawTaskList" or "DrawQuickTaskList")
                    harmony.Patch(m,
                        prefix: new HarmonyMethod(typeof(BehaviorDesignerToolbarStyleFix), nameof(Prefix)));
            }

            Debug.Log("[BD Fix] Toolbar style safety patch installed.");
        }
        catch (Exception e)
        {
            Debug.LogError("[BD Fix] Patch failed: " + e);
        }
    }

    static void Prefix()
    {
        // 兼容旧拼写的样式名称
        AddFallbackStyle("ToolbarSeachTextField", "ToolbarSearchTextField");
        AddFallbackStyle("ToolbarSeachCancelButton", "ToolbarSearchCancelButton");
        AddFallbackStyle("ToolbarSeachCancelButtonEmpty", "ToolbarSearchCancelButtonEmpty");
    }

    static void AddFallbackStyle(string wrongName, string correctName)
    {
        if (GUI.skin.FindStyle(wrongName) != null) return;

        // 尝试从当前 skin 获取正确样式
        var style = GUI.skin.FindStyle(correctName);
        if (style == null)
        {
            // 如果当前皮肤没有，则从 Inspector 默认皮肤加载
            var builtin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
            style = builtin?.FindStyle(correctName);
        }

        if (style == null)
        {
            // 最后兜底一个基础样式
            style = new GUIStyle(EditorStyles.textField);
        }

        // 克隆并添加为旧拼写名
        var copy = new GUIStyle(style) { name = wrongName };
        var styles = new System.Collections.Generic.List<GUIStyle>(GUI.skin.customStyles ?? Array.Empty<GUIStyle>());
        styles.Add(copy);
        GUI.skin.customStyles = styles.ToArray();
    }
}
#endif
