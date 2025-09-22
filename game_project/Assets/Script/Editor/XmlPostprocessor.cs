using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class XmlPostprocessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
                                       string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string assetPath in importedAssets)
        {
            if (assetPath.EndsWith(".xml"))
            {
                string fullPath = Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length));
                if (File.Exists(fullPath))
                {
                    string text = File.ReadAllText(fullPath, Encoding.UTF8);
                    string cleaned = Regex.Replace(text, @"[\x00-\x08\x0B\x0C\x0E-\x1F]", "");
                    if (text != cleaned)
                    {
                        File.WriteAllText(fullPath, cleaned, Encoding.UTF8);
                        Debug.Log($"[XmlPostprocessor] 清理非法字符: {assetPath}");
                    }
                }
            }
        }
    }
}
