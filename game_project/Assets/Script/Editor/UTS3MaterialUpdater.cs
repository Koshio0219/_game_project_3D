using UnityEngine;
using UnityEditor;
using System.Xml;
using System.IO;

namespace Game.Editor
{
    public class UTS3MaterialUpdater : EditorWindow
    {
        public TextAsset xmlFile;
        public string textureFolder = "Assets/Models/Textures";
        public string materialFolder = "Assets/Models/Materials";

        [MenuItem("Tools/UTS3 Material Updater")]
        public static void ShowWindow()
        {
            GetWindow<UTS3MaterialUpdater>("UTS3 Material Updater");
        }

        void OnGUI()
        {
            GUILayout.Label("UTS3 材质自动更新工具", EditorStyles.boldLabel);
            xmlFile = (TextAsset)EditorGUILayout.ObjectField("模型 XML 文件", xmlFile, typeof(TextAsset), false);
            textureFolder = EditorGUILayout.TextField("贴图目录", textureFolder);
            materialFolder = EditorGUILayout.TextField("材质目录", materialFolder);

            if (GUILayout.Button("开始更新材质"))
            {
                if (xmlFile != null)
                    UpdateMaterials();
                else
                    Debug.LogError("请先选择 XML 文件！");
            }
        }

        void UpdateMaterials()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlFile.text);

            // 读取贴图列表
            XmlNodeList textureNodes = xmlDoc.SelectNodes("//textureList/Texture");
            string[] texturePaths = new string[textureNodes.Count];
            for (int i = 0; i < textureNodes.Count; i++)
                texturePaths[i] = Path.Combine(textureFolder, textureNodes[i].InnerText.Trim());

            // 读取材质信息
            XmlNodeList matNodes = xmlDoc.SelectNodes("//materialList/Material");
            foreach (XmlNode matNode in matNodes)
            {
                string materialName = matNode.SelectSingleNode("materialName")?.InnerText ?? "Unknown";
                string matName = materialName.Replace(" ", "").Replace("/", "_");
                string matPath = Path.Combine(materialFolder, $"{matName}.mat");

                Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (mat == null)
                {
                    Debug.LogWarning($"⚠️ 找不到材质: {matName}");
                    continue;
                }

                Undo.RecordObject(mat, "UTS3 Material Update");

                // 基础颜色
                XmlNode diffNode = matNode.SelectSingleNode("diffuse");
                if (diffNode != null && mat.HasProperty("_BaseColor"))
                {
                    float r = float.Parse(diffNode["r"].InnerText);
                    float g = float.Parse(diffNode["g"].InnerText);
                    float b = float.Parse(diffNode["b"].InnerText);
                    float a = float.Parse(diffNode["a"].InnerText);
                    mat.SetColor("_BaseColor", new Color(r, g, b, a));
                }
                else
                {
                    Debug.LogWarning($"⚠️ 材质 {matName} 没有 _BaseColor 属性");
                }

                // 主贴图
                int texID = int.Parse(matNode.SelectSingleNode("textureID").InnerText);
                if (texID >= 0 && texID < texturePaths.Length)
                {
                    string texPath = texturePaths[texID];
                    Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                    // UTS3 URP 使用 _BaseMap
                    if (tex && mat.HasProperty("_MainTex") && mat.HasProperty("_BaseMap"))
                    {
                        mat.SetTexture("_MainTex", tex);
                        mat.SetTexture("_BaseMap", tex);
                    }
                    else
                        Debug.LogWarning($"⚠️ 材质 {matName} 没有 _MainTex 属性 或 贴图文件不存在: {texPath}");
                }

                // 追加贴图（AdditionalTextureID → 环境贴图）
                XmlNode addTexNode = matNode.SelectSingleNode("additionalTextureID");
                if (addTexNode != null)
                {
                    int addTexID = int.Parse(addTexNode.InnerText);
                    if (addTexID >= 0 && addTexID < texturePaths.Length)
                    {
                        string addTexPath = texturePaths[addTexID];
                        Texture2D addTex = AssetDatabase.LoadAssetAtPath<Texture2D>(addTexPath);

                        if (addTex != null)
                        {
                            // 根据你的项目选择合适的属性
                            // 一般是 Sph/Spa → _SphereAddTex
                            if (mat.HasProperty("_SphereAddTex"))
                            {
                                mat.SetTexture("_SphereAddTex", addTex);
                                Debug.Log($"🌀 [{mat.name}] 已设置环境贴图: {Path.GetFileName(addTexPath)}");
                            }
                            else if (mat.HasProperty("_2nd_ShadeMap"))
                            {
                                mat.SetTexture("_2nd_ShadeMap", addTex);
                                Debug.Log($"🎨 [{mat.name}] 已设置第二阴影贴图: {Path.GetFileName(addTexPath)}");
                            }
                            else
                            {
                                Debug.LogWarning($"[{mat.name}] 找不到可用的 additionalTexture 槽位");
                            }
                        }
                    }
                }

                // Toon阴影贴图
                int toon_ID = int.Parse(matNode.SelectSingleNode("toonTextureID").InnerText);
                if (toon_ID >= 0 && toon_ID < texturePaths.Length)
                {
                    string toonPath = texturePaths[toon_ID];
                    Texture2D toonTex = AssetDatabase.LoadAssetAtPath<Texture2D>(toonPath);
                    if (toonTex && mat.HasProperty("_1st_ShadeMap"))
                        mat.SetTexture("_1st_ShadeMap", toonTex);
                    else
                        Debug.LogWarning($"⚠️ 材质 {matName} 没有 _1st_ShadeMap 属性 或 贴图文件不存在: {toonPath}");
                }

                // Outline 颜色与宽度
                XmlNode edgeNode = matNode.SelectSingleNode("edgeColor");
                if (edgeNode != null && mat.HasProperty("_Outline_Color"))
                {
                    float r = float.Parse(edgeNode["r"].InnerText);
                    float g = float.Parse(edgeNode["g"].InnerText);
                    float b = float.Parse(edgeNode["b"].InnerText);
                    float a = float.Parse(edgeNode["a"].InnerText);
                    mat.SetColor("_Outline_Color", new Color(r, g, b, a));
                }
                else
                {
                    Debug.LogWarning($"⚠️ 材质 {matName} 没有 _Outline_Color 属性");
                }

                XmlNode edgeSize = matNode.SelectSingleNode("edgeSize");
                if (edgeSize != null && mat.HasProperty("_Outline_Width"))
                    mat.SetFloat("_Outline_Width", float.Parse(edgeSize.InnerText));
                else
                    Debug.LogWarning($"⚠️ 材质 {matName} 没有 _Outline_Width 属性");

                // -------------------- 追加字段设置 --------------------

                // 1. specular（高光颜色/强度）
                XmlNode specular = matNode.SelectSingleNode("specular");
                if (specular != null && mat.HasProperty("_SpecularColor"))
                {
                    float r = float.Parse(specular["r"].InnerText);
                    float g = float.Parse(specular["g"].InnerText);
                    float b = float.Parse(specular["b"].InnerText);
                    mat.SetColor("_SpecularColor", new Color(r, g, b));
                }
                else
                {
                    Debug.LogWarning($"⚠️ 材质 {matName} 的 specular 字段格式错误");
                }


                // 2. shininess（光泽度）
                if (matNode["shininess"] != null || matNode["shiness"] != null)
                {
                    string shinyStr = matNode["shininess"]?.InnerText ?? matNode["shiness"]?.InnerText;
                    if (float.TryParse(shinyStr, out float shiny))
                    {
                        mat.SetFloat("_Smoothness", Mathf.Clamp01(shiny / 100f)); // 根据MMD范围(0~100)映射到0~1
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ 材质 {matName} 的 shininess 字段格式错误");
                    }
                }

                // 4. toonID（Toon阴影层编号）
                //if (matNode["toonID"] != null)
                //{
                //    if (int.TryParse(matNode["toonID"].InnerText, out int toonID))
                //    {
                //        //// Toon ID 0~10 映射到不同的阴影强度（可根据美术调整）
                //        //float shadowStrength = Mathf.Clamp01(1f - toonID * 0.08f);
                //        //mat.SetFloat("_1st_ShadeColor_Step", shadowStrength);
                //        //mat.SetFloat("_2nd_ShadeColor_Step", shadowStrength * 0.5f);

                //        string texPath = texturePaths[toonID];
                //        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                //        if (tex && mat.HasProperty("_ShadingGradeMap"))
                //        {
                //            mat.SetTexture("_ShadingGradeMap", tex);
                //        }
                //        else
                //        {
                //            Debug.LogWarning($"⚠️ 材质 {matName} 没有 _ShadingGradeMap 属性 或 贴图文件不存在: {texPath}");
                //        }
                //    }
                //}

                // 5. isDrawBothFaces（是否双面绘制）
                if (matNode["isDrawBothFaces"] != null)
                {
                    if (int.TryParse(matNode["isDrawBothFaces"].InnerText, out int both))
                    {
                        mat.SetInt("_CullMode", both == 1 ? (int)UnityEngine.Rendering.CullMode.Off : (int)UnityEngine.Rendering.CullMode.Back);
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ 材质 {matName} 的 isDrawBothFaces 字段格式错误");
                    }
                }

                // 6. isDrawSelfShadowMap（是否生成自阴影贴图）
                if (matNode["isDrawSelfShadowMap"] != null)
                {
                    if (int.TryParse(matNode["isDrawSelfShadowMap"].InnerText, out int useShadowMap))
                    {
                        mat.SetFloat("_UseShadow", useShadowMap == 1 ? 1f : 0f);
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ 材质 {matName} 的 isDrawSelfShadowMap 字段格式错误");
                    }
                }

                // 7. isDrawSelfShadow（是否启用自阴影）
                if (matNode["isDrawSelfShadow"] != null)
                {
                    if (int.TryParse(matNode["isDrawSelfShadow"].InnerText, out int selfShadow))
                    {
                        mat.SetFloat("_Is_LightColor_Base", selfShadow == 1 ? 1f : 0f);
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ 材质 {matName} 的 isDrawSelfShadow 字段格式错误");
                    }
                }


                EditorUtility.SetDirty(mat);
                Debug.Log($"✅ 已更新材质: {matName}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("🎉 所有材质更新完成！");
        }
    }
}
