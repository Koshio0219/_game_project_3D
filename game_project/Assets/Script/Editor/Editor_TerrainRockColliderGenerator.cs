#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class Editor_TerrainRockColliderGenerator : EditorWindow
{
    private string[] rockKeywords = { "PT_Menhir_Rock_02" };
    private ColliderType colliderType = ColliderType.Mesh;
    private Transform rootParent;
    private bool confirmDeleteOld = true;

    private enum ColliderType { Box, Mesh, Capsule }

    [MenuItem("Tools/Environment/Generate Rock Colliders from Terrains")]
    private static void ShowWindow()
    {
        GetWindow<Editor_TerrainRockColliderGenerator>("Rock Collider Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("岩石碰撞体自动生成工具", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("扫描场景中所有 Terrain 的树实例，识别名称包含 rock/stone/岩/石 等的 prefab，并生成碰撞体。", MessageType.Info);

        colliderType = (ColliderType)EditorGUILayout.EnumPopup("碰撞体类型", colliderType);
        rootParent = (Transform)EditorGUILayout.ObjectField("父物体（可选）", rootParent, typeof(Transform), true);
        confirmDeleteOld = EditorGUILayout.Toggle("清除旧的自动生成内容", confirmDeleteOld);

        if (GUILayout.Button("⚙️ 生成岩石碰撞体", GUILayout.Height(40)))
        {
            GenerateRockColliders();
        }
    }

    private void GenerateRockColliders()
    {
        var terrains = Object.FindObjectsByType<Terrain>();
        if (terrains.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "场景中未找到任何 Terrain。", "好的");
            return;
        }

        // 生成根节点
        GameObject root;
        if (rootParent != null)
            root = rootParent.gameObject;
        else
        {
            root = GameObject.Find("RockColliders_AutoGen");
            if (root == null)
                root = new GameObject("RockColliders_AutoGen");
        }

        if (confirmDeleteOld)
        {
            for (int i = root.transform.childCount - 1; i >= 0; i--)
                Undo.DestroyObjectImmediate(root.transform.GetChild(i).gameObject);
        }

        int totalAdded = 0;
        foreach (var terrain in terrains)
        {
            var data = terrain.terrainData;
            if (data == null) continue;

            var prototypes = data.treePrototypes;
            var instances = data.treeInstances;

            for (int i = 0; i < instances.Length; i++)
            {
                var inst = instances[i];
                var proto = prototypes[inst.prototypeIndex];
                if (proto?.prefab == null) continue;

                string prefabName = proto.prefab.name.ToLower();
                if (!IsRockName(prefabName)) continue;

                Vector3 worldPos = Vector3.Scale(inst.position, data.size) + terrain.transform.position;

                GameObject go = new GameObject($"RockCollider_{prefabName}_{i:D4}");
                Undo.RegisterCreatedObjectUndo(go, "Create Rock Collider");
                go.transform.SetParent(root.transform);
                go.transform.position = worldPos;

                AddCollider(go, proto.prefab);
                totalAdded++;
            }
        }

        EditorUtility.DisplayDialog("完成",
            $"已为所有地形生成 {totalAdded} 个岩石碰撞体。\n所有对象保存在 RockColliders_AutoGen 下。",
            "好的");

        Debug.Log($"✅ [RockColliderGenerator] 已生成 {totalAdded} 个岩石碰撞体。");
    }

    private bool IsRockName(string name)
    {
        foreach (var key in rockKeywords)
        {
            if (name.Contains(key.ToLower())) return true;
        }
        return false;
    }

    private void AddCollider(GameObject target, GameObject prefab)
    {
        switch (colliderType)
        {
            case ColliderType.Mesh:
                Mesh mesh = GetMeshFromPrefab(prefab);
                if (mesh != null)
                {
                    var meshCol = Undo.AddComponent<MeshCollider>(target);
                    meshCol.sharedMesh = mesh;
                }
                else
                {
                    Undo.AddComponent<BoxCollider>(target);
                }
                break;

            case ColliderType.Box:
                Undo.AddComponent<BoxCollider>(target);
                break;

            case ColliderType.Capsule:
                Undo.AddComponent<CapsuleCollider>(target);
                break;
        }
    }

    private Mesh GetMeshFromPrefab(GameObject prefab)
    {
        // 优先 LODGroup LOD0
        var lodGroup = prefab.GetComponent<LODGroup>();
        if (lodGroup != null)
        {
            var lods = lodGroup.GetLODs();
            if (lods.Length > 0 && lods[0].renderers != null && lods[0].renderers.Length > 0)
            {
                var rend = lods[0].renderers[0];
                if (rend is MeshRenderer)
                {
                    var mf = rend.GetComponent<MeshFilter>();
                    if (mf != null) return mf.sharedMesh;
                }
                else if (rend is SkinnedMeshRenderer smr)
                {
                    return smr.sharedMesh;
                }
            }
        }

        // 没有 LODGroup 的情况
        var meshFilter = prefab.GetComponentInChildren<MeshFilter>();
        if (meshFilter != null) return meshFilter.sharedMesh;

        var smr2 = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
        return smr2?.sharedMesh;
    }
}
#endif
