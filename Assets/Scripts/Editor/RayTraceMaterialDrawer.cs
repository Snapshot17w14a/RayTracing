using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using static UnityEditor.EditorGUI;
using static UnityEditor.EditorGUIUtility;

[CustomPropertyDrawer(typeof(RayTracedMaterial))]
public class RayTraceMaterialDrawer : PropertyDrawer
{
    private readonly Dictionary<string, int> instanceIndentPair = new Dictionary<string, int>();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var colorProperty = property.FindPropertyRelative("color");
        var smoothnessProperty = property.FindPropertyRelative("smoothness");

        Rect rectPosition = new Rect(position.x, position.y, position.width, singleLineHeight);

        BeginProperty(position, label, property);

        var propertyPath = property.propertyPath;
        instanceIndentPair[propertyPath] = 0;

        PropertyField(rectPosition, colorProperty, new GUIContent("Color"));
        NextLine(ref rectPosition, propertyPath);
        smoothnessProperty.floatValue = Slider(rectPosition, new GUIContent("Smoothness"), smoothnessProperty.floatValue, 0, 1);
        NextLine(ref rectPosition, propertyPath);

        EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int multiplier = instanceIndentPair.ContainsKey(property.propertyPath) ? instanceIndentPair[property.propertyPath] : 1;
        return singleLineHeight * multiplier + standardVerticalSpacing * (multiplier - 1);
    }

    private void NextLine(ref Rect rect, string path)
    {
        rect.y += singleLineHeight + standardVerticalSpacing;
        instanceIndentPair[path]++;
    }
}
