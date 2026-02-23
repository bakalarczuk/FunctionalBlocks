using UnityEditor;
using UnityEngine;
using FunctionalBlocks;

namespace FunctionalBlocks.Editor
{
    [CustomPropertyDrawer(typeof(BlockDef))]
    public class BlockDefDrawer : PropertyDrawer
    {
        private const float PAD = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int lines = 1;

            if (!property.isExpanded)
                return Height(lines);

            var typeProp = property.FindPropertyRelative("type");
            var type = (BlockType)typeProp.enumValueIndex;

            lines += 2;

            switch (type)
            {
                case BlockType.Start:
                    lines += 1; // nextId
                    break;

                case BlockType.CreatePrimitive:
                    lines += 1; // nextId
                    lines += 5; // primitiveType, position, rotationEuler, scale, outObjectVar
                    break;

                case BlockType.Transform:
                    lines += 1; // nextId
                    lines += 4; // transformKind, transformMode, targetObjectVar, vector
                    break;

                case BlockType.SetNumber:
                    lines += 1; // nextId
                    lines += 2; // varName, number
                    break;

                case BlockType.CompareNumber:
                    lines += 1; // nextId
                    lines += 6; // aVar, compareOp, bIsConstant, bConst/bVar, outBoolVar
                    break;

                case BlockType.If:
                    lines += 3; 
                    break;
            }

            return Height(lines);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var idProp = property.FindPropertyRelative("id");
            var typeProp = property.FindPropertyRelative("type");

            var headerRect = Line(position, 0);
            string header = BuildHeader(idProp.stringValue, (BlockType)typeProp.enumValueIndex);
            property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, header, true);

            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            int line = 1;

            Draw(property, ref line, position, "id", "Id");
            Draw(property, ref line, position, "type", "Type");

            var type = (BlockType)typeProp.enumValueIndex;

            switch (type)
            {
                case BlockType.Start:
                    Draw(property, ref line, position, "nextId", "Next Id");
                    break;

                case BlockType.CreatePrimitive:
                    Draw(property, ref line, position, "nextId", "Next Id");
                    Draw(property, ref line, position, "primitiveType", "Primitive Type");
                    Draw(property, ref line, position, "position", "Position");
                    Draw(property, ref line, position, "rotationEuler", "Rotation Euler");
                    Draw(property, ref line, position, "scale", "Scale");
                    Draw(property, ref line, position, "outObjectVar", "Out Object Var");
                    break;

                case BlockType.Transform:
                    Draw(property, ref line, position, "nextId", "Next Id");
                    Draw(property, ref line, position, "transformKind", "Transform Kind");
                    Draw(property, ref line, position, "transformMode", "Transform Mode");
                    Draw(property, ref line, position, "targetObjectVar", "Target Object Var");
                    Draw(property, ref line, position, "vector", "Vector");
                    break;

                case BlockType.SetNumber:
                    Draw(property, ref line, position, "nextId", "Next Id");
                    Draw(property, ref line, position, "varName", "Var Name");
                    Draw(property, ref line, position, "number", "Number");
                    break;

                case BlockType.CompareNumber:
                    Draw(property, ref line, position, "nextId", "Next Id");
                    Draw(property, ref line, position, "aVar", "A Var");
                    Draw(property, ref line, position, "compareOp", "Compare Op");
                    Draw(property, ref line, position, "bIsConstant", "B Is Constant");

                    var bIsConstantProp = property.FindPropertyRelative("bIsConstant");
                    if (bIsConstantProp.boolValue)
                        Draw(property, ref line, position, "bConst", "B Const");
                    else
                        Draw(property, ref line, position, "bVar", "B Var");

                    Draw(property, ref line, position, "outBoolVar", "Out Bool Var");
                    break;

                case BlockType.If:
                    Draw(property, ref line, position, "conditionVar", "Condition Var");
                    Draw(property, ref line, position, "trueNextId", "True Next Id");
                    Draw(property, ref line, position, "falseNextId", "False Next Id");
                    break;
            }

            EditorGUI.EndProperty();
        }


        private static string BuildHeader(string id, BlockType type)
        {
            if (string.IsNullOrWhiteSpace(id)) id = "<no id>";
            return $"{id}  ({type})";
        }

        private static float Height(int lines)
        {
            float h = EditorGUIUtility.singleLineHeight;
            float s = EditorGUIUtility.standardVerticalSpacing;
            return lines * h + (lines - 1) * s + PAD * 2f;
        }

        private static Rect Line(Rect position, int index)
        {
            float h = EditorGUIUtility.singleLineHeight;
            float s = EditorGUIUtility.standardVerticalSpacing;
            return new Rect(
                position.x,
                position.y + PAD + index * (h + s),
                position.width,
                h
            );
        }

        private static void Draw(SerializedProperty root, ref int line, Rect position, string relName, string label)
        {
            var p = root.FindPropertyRelative(relName);
            var r = Line(position, line++);
            EditorGUI.PropertyField(r, p, new GUIContent(label), true);
        }
    }
}