using UnityEngine;

namespace Game.Unit
{
    [DisallowMultipleComponent]
    public class AutoTriggerGenerator : MonoBehaviour
    {
        [Tooltip("Trigger 比原始 Collider 大多少比例，例如 1.2 表示大 20%")]
        public float triggerScale = 1.2f;

        private void Start()
        {
            GenerateRootTriggers();
        }

        private void GenerateRootTriggers()
        {
            Collider[] colliders = GetComponentsInChildren<Collider>();

            foreach (var col in colliders)
            {
                if (col.isTrigger) continue;

                // 使用 transform.root
                var target = transform.root.gameObject;

                if (col is BoxCollider box)
                {
                    if(target.TryGetComponent<BoxCollider>(out var boxCollider))
                        if (boxCollider != null && boxCollider.isTrigger)
                            continue;
                    
                    BoxCollider trigger = target.AddComponent<BoxCollider>();
                    trigger.center = box.center;
                    trigger.size = box.size * triggerScale;
                    trigger.isTrigger = true;
                }
                else if (col is SphereCollider sphere)
                {
                    if (target.TryGetComponent<SphereCollider>(out var boxCollider))
                        if (boxCollider != null && boxCollider.isTrigger)
                            continue;
                    SphereCollider trigger = target.AddComponent<SphereCollider>();
                    trigger.center = sphere.center;
                    trigger.radius = sphere.radius * triggerScale;
                    trigger.isTrigger = true;
                }
                else if (col is CapsuleCollider capsule)
                {
                    if (target.TryGetComponent<CapsuleCollider>(out var boxCollider))
                        if (boxCollider != null && boxCollider.isTrigger)
                            continue;
                    CapsuleCollider trigger = target.AddComponent<CapsuleCollider>();
                    trigger.center = capsule.center;
                    trigger.radius = capsule.radius * triggerScale;
                    trigger.height = capsule.height * triggerScale;
                    trigger.direction = capsule.direction;
                    trigger.isTrigger = true;
                }
                else
                {
                    Debug.LogWarning($"未处理的Collider类型: {col.GetType().Name}");
                }
            }

            Debug.Log($"[{name}] 已直接在根物体上添加Trigger Collider。");
        }
    }
    }
