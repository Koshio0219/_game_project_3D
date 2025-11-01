using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Navigation
{
    [DefaultExecutionOrder(-200)]
    public class NavMeshObject : MonoBehaviour
    {
        // Global containers for all active mesh/terrain tags
        public static List<MeshFilter> m_Meshes = new();
        public static List<Terrain> m_Terrains = new();

        void OnEnable()
        {
            foreach (var m in GetComponentsInChildren<MeshFilter>())
            {
                if (m != null && !m_Meshes.Contains(m))
                {
                    m_Meshes.Add(m);
                }
            }
            foreach (var t in GetComponentsInChildren<Terrain>())
            {
                if (t != null && !m_Terrains.Contains(t))
                {
                    m_Terrains.Add(t);
                }
            }
        }
        void OnDisable()
        {
            foreach (var m in GetComponentsInChildren<MeshFilter>())
            {
                if (m != null && m_Meshes.Contains(m))
                {
                    m_Meshes.Remove(m);
                }
            }
            foreach (var t in GetComponentsInChildren<Terrain>())
            {
                if (t != null && m_Terrains.Contains(t))
                {
                    m_Terrains.Remove(t);
                }
            }
        }

        // Collect all the navmesh build sources for enabled objects tagged by this component
        public static void Collect(ref List<NavMeshBuildSource> sources)
        {
            sources.Clear();

            for (var i = 0; i < m_Meshes.Count; ++i)
            {
                var mf = m_Meshes[i];
                if (mf == null) continue;

                var m = mf.sharedMesh;
                if (m == null) continue;

                var s = new NavMeshBuildSource
                {
                    shape = NavMeshBuildSourceShape.Mesh,
                    sourceObject = m,
                    transform = mf.transform.localToWorldMatrix,
                    area = 0
                };
                if (mf.gameObject.activeInHierarchy)
                {
                    sources.Add(s);
                }
            }

            for (var i = 0; i < m_Terrains.Count; ++i)
            {
                var t = m_Terrains[i];
                if (t == null) continue;

                var s = new NavMeshBuildSource
                {
                    shape = NavMeshBuildSourceShape.Terrain,
                    sourceObject = t.terrainData,
                    // Terrain system only supports translation - so we pass translation only to back-end
                    transform = Matrix4x4.TRS(t.transform.position, Quaternion.identity, Vector3.one),
                    area = 0
                };
                sources.Add(s);
            }
        }
    }
}