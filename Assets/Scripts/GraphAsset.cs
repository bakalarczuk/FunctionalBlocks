using System;
using System.Collections.Generic;
using UnityEngine;

namespace FunctionalBlocks
{
    /// <summary>
    /// ScriptableObject asset that holds a functional block graph definition.
    /// </summary>
    [CreateAssetMenu(menuName = "Functional Blocks/Graph Asset", fileName = "GraphAsset")]
    public class GraphAsset : ScriptableObject
    {
        /// <summary>
        /// The serialized graph data.
        /// </summary>
        [Tooltip("The serialized graph data.")]
        public GraphData data = new GraphData();
    }

    /// <summary>
    /// Serializable data structure for a functional block graph.
    /// </summary>
    [Serializable]
    public class GraphData
    {
        /// <summary>
        /// Version of the graph data format.
        /// </summary>
        [Tooltip("Version of the graph data format.")]
        public int formatVersion = 1;

        /// <summary>
        /// The ID of the block where execution starts.
        /// </summary>
        [Tooltip("The ID of the block where execution starts.")]
        public string startBlockId = "start";

        /// <summary>
        /// List of variables used in the graph.
        /// </summary>
        [Tooltip("List of variables used in the graph.")]
        public List<VariableDef> variables = new List<VariableDef>();

        /// <summary>
        /// List of all blocks (nodes) in the graph.
        /// </summary>
        [Tooltip("List of all blocks (nodes) in the graph.")]
        public List<BlockDef> blocks = new List<BlockDef>();
    }

    /// <summary>
    /// Supported value types for variables.
    /// </summary>
    public enum ValueType
    {
        Number,
        Bool,
        Vector3,
        Text
    }

    [Serializable]
    public class Value
    {
        public ValueType type = ValueType.Number;

        public float number;
        public bool boolean;
        public Vector3 vector3;
        public string text;

        public static Value Number(float v) => new Value { type = ValueType.Number, number = v };
        public static Value Bool(bool v) => new Value { type = ValueType.Bool, boolean = v };
        public static Value Vector(Vector3 v) => new Value { type = ValueType.Vector3, vector3 = v };
        public static Value Text(string v) => new Value { type = ValueType.Text, text = v };
    }

    [Serializable]
    public class VariableDef
    {
        public string name = "x";
        public Value value = Value.Number(0);
    }

    public enum BlockType
    {
        Start,
        CreatePrimitive,
        Transform,
        SetNumber,
        CompareNumber,
        If
    }

    public enum PrimitiveTypeFB
    {
        Cube,
        Sphere,
        Cylinder
    }

    public enum TransformKind
    {
        Move,
        Rotate,
        Scale
    }

    public enum TransformMode
    {
        Set,
        Add
    }

    public enum CompareOp
    {
        Equal,
        NotEqual,
        Greater,
        Less,
        GreaterOrEqual,
        LessOrEqual
    }

    [Serializable]
    public class BlockDef
    {
        public string id = "block1";
        public BlockType type = BlockType.Start;

        public string nextId;
        public string trueNextId;
        public string falseNextId;

        // CreatePrimitive
        public PrimitiveTypeFB primitiveType = PrimitiveTypeFB.Cube;
        public Vector3 position = Vector3.zero;
        public Vector3 rotationEuler = Vector3.zero;
        public Vector3 scale = Vector3.one;
        public string outObjectVar = "obj"; 

        // Transform (Move/Rotate/Scale)
        public TransformKind transformKind = TransformKind.Move;
        public TransformMode transformMode = TransformMode.Add;
        public string targetObjectVar = "obj"; 
        public Vector3 vector = Vector3.zero;

        // SetNumber
        public string varName = "x";
        public float number = 0f;

        // CompareNumber
        public string aVar = "x";
        public CompareOp compareOp = CompareOp.Greater;
        public bool bIsConstant = true;
        public float bConst = 0f;
        public string bVar = "y";
        public string outBoolVar = "cond";

        // If
        public string conditionVar = "cond";
    }
}