using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 自动修复 Behavior Designer 缺失的图标纹理
/// </summary>
//[InitializeOnLoad]
public static class BehaviorDesignerTextureAutoFix
{
    static BehaviorDesignerTextureAutoFix()
    {
        // 延迟执行，确保编辑器加载完程序集
        //EditorApplication.delayCall += TryFixAllTextures;
    }
    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void TryFixAllTextures()
    {
        try
        {
            var utilityType = FindBehaviorDesignerUtilityType();
            if (utilityType == null)
            {
                Debug.LogWarning("[BDTextureFix] 未找到 BehaviorDesignerUtility 类型。");
                return;
            }

            var fields = utilityType.GetFields(BindingFlags.Static | BindingFlags.NonPublic);
            Debug.Log($"[BDFieldScanner] 找到 {fields.Length} 个静态私有字段：");

            var placeholderTex = CreatePlaceholderTexture();

            foreach (var f in fields)
            {
                if (f.FieldType == typeof(Texture2D))
                {
                    var tex = f.GetValue(null) as Texture2D;
                    if (tex == null || tex.width <= 0 || tex.height <= 0)
                    {
                        f.SetValue(null, placeholderTex);
                        Debug.Log($"[BDTextureFix] 已替换缺失贴图字段: {f.Name}");
                    }
                }
                // 修复 Dictionary<string, Texture2D>
                else if (f.FieldType == typeof(Dictionary<string, Texture2D>))
                {
                    var dict = f.GetValue(null) as Dictionary<string, Texture2D>;
                    if (dict != null)
                    {
                        var keys = dict.Keys.ToList(); // 先复制 keys 避免遍历时修改
                        foreach (var key in keys)
                        {
                            var value = dict[key];
                            if (value == null || value.width <= 0 || value.height <= 0)
                                dict[key] = placeholderTex;
                        }

                        Debug.Log($"[BDTextureFix] 已替换字典 {f.Name} 中的所有贴图值 ");
                    }
                }
            }

            Debug.Log("[BDTextureFix] 完成 Behavior Designer 缺失贴图修复。");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[BDTextureFix] 修复时出错: {e}");
        }
    }


    private static Texture2D CreatePlaceholderTexture()
    {
        var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        var colors = new Color[16 * 16];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = new Color(0.4f, 0.4f, 0.4f, 0.25f);
        tex.SetPixels(colors);
        tex.Apply();
        tex.hideFlags = HideFlags.HideAndDontSave;
        return tex;
    }

    private static Type FindBehaviorDesignerUtilityType()
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var t = asm.GetType("BehaviorDesigner.Editor.BehaviorDesignerUtility", false);
                if (t != null)
                {
                    Debug.Log($"[BDTextureFixer] 找到 BehaviorDesignerUtility 于程序集：{asm.GetName().Name}");
                    return t;
                }
            }
            catch { /* ignore */ }
        }
        return null;
    }

    //public static Texture2D LoadTexture(string imageName, bool useSkinColor = true, UnityEngine.Object obj = null)
    //{
    //    if (textureCache.ContainsKey(imageName))
    //    {
    //        return textureCache[imageName];
    //    }

    //    Texture2D texture2D = null;
    //    string name = string.Format("{0}{1}", (!useSkinColor) ? string.Empty : ((!EditorGUIUtility.isProSkin) ? "Light" : "Dark"), imageName);
    //    Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
    //    if (manifestResourceStream == null)
    //    {
    //        name = string.Format("BehaviorDesignerEditor.Resources.{0}{1}", (!useSkinColor) ? string.Empty : ((!EditorGUIUtility.isProSkin) ? "Light" : "Dark"), imageName);
    //        manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
    //    }

    //    if (manifestResourceStream != null)
    //    {
    //        texture2D = new Texture2D(0, 0, TextureFormat.RGBA32, mipChain: false);
    //        texture2D.LoadImage(ReadToEnd(manifestResourceStream));
    //        manifestResourceStream.Close();
    //    }

    //    texture2D.hideFlags = HideFlags.HideAndDontSave;
    //    textureCache.Add(imageName, texture2D);
    //    return texture2D;
    //}
}
