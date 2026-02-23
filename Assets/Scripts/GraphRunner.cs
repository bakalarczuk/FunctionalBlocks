using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FunctionalBlocks
{
    public class GraphRunner : MonoBehaviour
    {
        [Header("Graph")]
        public GraphAsset graph;

        [Header("Execution")]
        public bool runOnStart = true;
        public int maxSteps = 10000;

        [Header("Scene")]
        public Transform parentForSpawned;
        public bool clearBeforeRun = true;

        private readonly List<GameObject> _spawnedThisRun = new List<GameObject>();

        private void Start()
        {
            if (runOnStart)
                RunFromAsset();
        }

        [ContextMenu("Run From Asset")]
        public void RunFromAsset()
        {
            if (graph == null)
            {
                Debug.LogError("[GraphRunner] GraphAsset is null.");
                return;
            }

            RunGraph(graph.data);
        }

        public void RunGraph(GraphData data)
        {
            if (data == null)
            {
                Debug.LogError("[GraphRunner] GraphData is null.");
                return;
            }

            if (clearBeforeRun)
                ClearSpawned();

            var blocksById = new Dictionary<string, BlockDef>(StringComparer.Ordinal);
            for (int i = 0; i < data.blocks.Count; i++)
            {
                var b = data.blocks[i];
                if (string.IsNullOrWhiteSpace(b.id))
                {
                    Debug.LogError($"[GraphRunner] Block at index {i} has empty id.");
                    continue;
                }

                if (blocksById.ContainsKey(b.id))
                {
                    Debug.LogError($"[GraphRunner] Duplicate block id: '{b.id}'.");
                    continue;
                }

                blocksById[b.id] = b;
            }

            if (!blocksById.ContainsKey(data.startBlockId))
            {
                Debug.LogError($"[GraphRunner] startBlockId '{data.startBlockId}' not found in blocks.");
                return;
            }

            var ctx = new ExecutionContext(parentForSpawned, _spawnedThisRun);

            for (int i = 0; i < data.variables.Count; i++)
            {
                var v = data.variables[i];
                if (string.IsNullOrWhiteSpace(v.name))
                {
                    Debug.LogWarning($"[GraphRunner] Variable at index {i} has empty name (skipped).");
                    continue;
                }
                ctx.SetVar(v.name, v.value);
            }

            string currentId = data.startBlockId;
            int steps = 0;

            while (!string.IsNullOrWhiteSpace(currentId))
            {
                steps++;
                if (steps > maxSteps)
                {
                    Debug.LogError($"[GraphRunner] Step limit exceeded ({maxSteps}). Possible loop in graph.");
                    break;
                }

                if (!blocksById.TryGetValue(currentId, out var block))
                {
                    Debug.LogError($"[GraphRunner] Block id '{currentId}' not found. Stopping.");
                    break;
                }

                currentId = ExecuteBlock(block, ctx);
            }

            // Done
            Debug.Log("[GraphRunner] Finished.");
        }

        private string ExecuteBlock(BlockDef b, ExecutionContext ctx)
        {
            switch (b.type)
            {
                case BlockType.Start:
                    return b.nextId;

                case BlockType.CreatePrimitive:
                    {
                        var go = ctx.CreatePrimitive(b.primitiveType);
                        go.transform.position = b.position;
                        go.transform.rotation = Quaternion.Euler(b.rotationEuler);
                        go.transform.localScale = b.scale;

                        string handle = Guid.NewGuid().ToString("N");
                        ctx.RegisterObject(handle, go);

                        if (!string.IsNullOrWhiteSpace(b.outObjectVar))
                            ctx.SetVar(b.outObjectVar, Value.Text(handle));
                        else
                            Debug.LogWarning($"[CreatePrimitive:{b.id}] outObjectVar is empty (object handle will be lost).");

                        return b.nextId;
                    }

                case BlockType.Transform:
                    {
                        if (string.IsNullOrWhiteSpace(b.targetObjectVar))
                        {
                            Debug.LogError($"[Transform:{b.id}] targetObjectVar is empty.");
                            return b.nextId;
                        }

                        if (!ctx.TryGetText(b.targetObjectVar, out var handle))
                        {
                            Debug.LogError($"[Transform:{b.id}] Variable '{b.targetObjectVar}' not found or not Text.");
                            return b.nextId;
                        }

                        if (!ctx.TryGetObject(handle, out var go))
                        {
                            Debug.LogError($"[Transform:{b.id}] No object registered under handle '{handle}'.");
                            return b.nextId;
                        }

                        ApplyTransform(go.transform, b.transformKind, b.transformMode, b.vector);
                        return b.nextId;
                    }

                case BlockType.SetNumber:
                    {
                        if (string.IsNullOrWhiteSpace(b.varName))
                        {
                            Debug.LogError($"[SetNumber:{b.id}] varName is empty.");
                            return b.nextId;
                        }
                        ctx.SetVar(b.varName, Value.Number(b.number));
                        return b.nextId;
                    }

                case BlockType.CompareNumber:
                    {
                        if (!ctx.TryGetNumber(b.aVar, out var a))
                        {
                            Debug.LogError($"[CompareNumber:{b.id}] Missing/invalid number variable '{b.aVar}'.");
                            return b.nextId;
                        }

                        float bValue;
                        if (b.bIsConstant)
                        {
                            bValue = b.bConst;
                        }
                        else
                        {
                            if (!ctx.TryGetNumber(b.bVar, out bValue))
                            {
                                Debug.LogError($"[CompareNumber:{b.id}] Missing/invalid number variable '{b.bVar}'.");
                                return b.nextId;
                            }
                        }

                        bool result = Compare(a, bValue, b.compareOp);

                        if (string.IsNullOrWhiteSpace(b.outBoolVar))
                        {
                            Debug.LogError($"[CompareNumber:{b.id}] outBoolVar is empty.");
                            return b.nextId;
                        }

                        ctx.SetVar(b.outBoolVar, Value.Bool(result));
                        return b.nextId;
                    }

                case BlockType.If:
                    {
                        if (!ctx.TryGetBool(b.conditionVar, out var cond))
                        {
                            Debug.LogError($"[If:{b.id}] Missing/invalid bool variable '{b.conditionVar}'.");
                            return b.falseNextId; 
                        }
                        return cond ? b.trueNextId : b.falseNextId;
                    }

                default:
                    Debug.LogError($"[GraphRunner] Unsupported block type: {b.type}");
                    return null;
            }
        }

        private static void ApplyTransform(Transform t, TransformKind kind, TransformMode mode, Vector3 v)
        {
            switch (kind)
            {
                case TransformKind.Move:
                    if (mode == TransformMode.Set) t.position = v;
                    else t.position += v;
                    break;

                case TransformKind.Rotate:
                    if (mode == TransformMode.Set) t.rotation = Quaternion.Euler(v);
                    else t.rotation = t.rotation * Quaternion.Euler(v);
                    break;

                case TransformKind.Scale:
                    if (mode == TransformMode.Set) t.localScale = v;
                    else t.localScale += v; 
                    break;
            }
        }

        private static bool Compare(float a, float b, CompareOp op)
        {
            switch (op)
            {
                case CompareOp.Equal: return Mathf.Approximately(a, b);
                case CompareOp.NotEqual: return !Mathf.Approximately(a, b);
                case CompareOp.Greater: return a > b;
                case CompareOp.Less: return a < b;
                case CompareOp.GreaterOrEqual: return a >= b || Mathf.Approximately(a, b);
                case CompareOp.LessOrEqual: return a <= b || Mathf.Approximately(a, b);
                default: return false;
            }
        }

        [ContextMenu("Clear Spawned")]
        public void ClearSpawned()
        {
            for (int i = 0; i < _spawnedThisRun.Count; i++)
            {
                var go = _spawnedThisRun[i];
                if (go != null) DestroyImmediate(go);
            }
            _spawnedThisRun.Clear();
        }
    }

    public class ExecutionContext
    {
        private readonly Dictionary<string, Value> _vars = new Dictionary<string, Value>(StringComparer.Ordinal);
        private readonly Dictionary<string, GameObject> _objects = new Dictionary<string, GameObject>(StringComparer.Ordinal);
        private readonly Transform _parentForSpawned;
        private readonly List<GameObject> _spawnedThisRun;

        public ExecutionContext(Transform parentForSpawned, List<GameObject> spawnedThisRun)
        {
            _parentForSpawned = parentForSpawned;
            _spawnedThisRun = spawnedThisRun;
        }

        public void SetVar(string name, Value value)
        {
            _vars[name] = value;
        }

        public bool TryGetNumber(string name, out float value)
        {
            value = 0f;
            if (!_vars.TryGetValue(name, out var v)) return false;
            if (v.type != ValueType.Number) return false;
            value = v.number;
            return true;
        }

        public bool TryGetBool(string name, out bool value)
        {
            value = false;
            if (!_vars.TryGetValue(name, out var v)) return false;
            if (v.type != ValueType.Bool) return false;
            value = v.boolean;
            return true;
        }

        public bool TryGetText(string name, out string value)
        {
            value = null;
            if (!_vars.TryGetValue(name, out var v)) return false;
            if (v.type != ValueType.Text) return false;
            value = v.text;
            return true;
        }

        public void RegisterObject(string handle, GameObject go)
        {
            _objects[handle] = go;
        }

        public bool TryGetObject(string handle, out GameObject go)
        {
            return _objects.TryGetValue(handle, out go);
        }

        public GameObject CreatePrimitive(PrimitiveTypeFB type)
        {
            PrimitiveType unityType = PrimitiveType.Cube;
            switch (type)
            {
                case PrimitiveTypeFB.Cube: unityType = PrimitiveType.Cube; break;
                case PrimitiveTypeFB.Sphere: unityType = PrimitiveType.Sphere; break;
                case PrimitiveTypeFB.Cylinder: unityType = PrimitiveType.Cylinder; break;
            }

            var go = GameObject.CreatePrimitive(unityType);
            if (_parentForSpawned != null)
                go.transform.SetParent(_parentForSpawned, true);

            _spawnedThisRun?.Add(go);
            return go;
        }
    }
}