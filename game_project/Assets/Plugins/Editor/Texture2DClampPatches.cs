#if UNITY_EDITOR
using System;
using HarmonyLib;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class Texture2DClampPatches
{
    static Texture2DClampPatches()
    {
        try
        {
            var h = new Harmony("fix.unity6.texture2d.ctor.clamp-anywhere");

            // 1) Texture2D(int width, int height, TextureFormat textureFormat, bool mipChain)
            var ctor1 = typeof(Texture2D).GetConstructor(new Type[] {
                typeof(int), typeof(int), typeof(TextureFormat), typeof(bool)
            });
            if (ctor1 != null)
                h.Patch(ctor1, prefix: new HarmonyMethod(typeof(Texture2DClampPatches), nameof(PrefixCtor_TexFormat)));

#if UNITY_6000_0_OR_NEWER
            // 2) Texture2D(int width, int height, GraphicsFormat format, TextureCreationFlags flags)
            var asm = typeof(Texture2D).Assembly;
            var gfxFormat = asm.GetType("UnityEngine.Experimental.Rendering.GraphicsFormat");
            var tcf = asm.GetType("UnityEngine.Experimental.Rendering.TextureCreationFlags");
            if (gfxFormat != null && tcf != null)
            {
                var ctor2 = typeof(Texture2D).GetConstructor(new Type[] {
                    typeof(int), typeof(int), gfxFormat, tcf
                });
                if (ctor2 != null)
                    h.Patch(ctor2, prefix: new HarmonyMethod(typeof(Texture2DClampPatches), nameof(PrefixCtor_GfxFormat)));
            }
#endif

            Debug.Log("[Texture2DClamp] enabled: clamp 0/neg size to 2 for Texture2D ctors.");
        }
        catch (Exception e)
        {
            Debug.LogError("[Texture2DClamp] patch failed: " + e);
        }
    }
    static void PrefixCtor_TexFormat(ref int __0, ref int __1, ref TextureFormat __2, ref bool __3)
    {
        if (__0 < 1) __0 = 2;   // width
        if (__1 < 1) __1 = 2;   // height
    }
#if UNITY_6000_0_OR_NEWER
    static void PrefixCtor_GfxFormat(ref int __0, ref int __1, object __2, object __3)
    {
        if (__0 < 1) __0 = 2;
        if (__1 < 1) __1 = 2;
    }
#endif

}
#endif
