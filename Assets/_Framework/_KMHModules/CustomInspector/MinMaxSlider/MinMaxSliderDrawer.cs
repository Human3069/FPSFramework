using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
public class MinMaxSliderDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        MinMaxSliderAttribute _sliderAttribute = attribute as MinMaxSliderAttribute;
        EditorGUI.BeginProperty(position, label, property);

        if (property.propertyType == SerializedPropertyType.Vector2)
        {
            float minValue = property.vector2Value.x;
            float maxValue = property.vector2Value.y;

            Rect minValueRect = new Rect(position.x, position.y, 40f, position.height);
            minValue = EditorGUI.FloatField(minValueRect, minValue);

            Rect sliderRect = new Rect(position.x + 45f, position.y, position.width - 90f, position.height);
            EditorGUI.MinMaxSlider(sliderRect, ref minValue, ref maxValue, _sliderAttribute.FloatMin, _sliderAttribute.FloatMax);

            Rect maxValueRect = new Rect(position.xMax - 40f, position.y, 40f, position.height);
            maxValue = EditorGUI.FloatField(maxValueRect, maxValue);

            property.vector2Value = new Vector2(minValue, maxValue);
        }
        else if (property.propertyType == SerializedPropertyType.Vector2Int)
        {
            float minValue = property.vector2IntValue.x;
            float maxValue = property.vector2IntValue.y;

            Rect minValueRect = new Rect(position.x, position.y, 40f, position.height);
            minValue = EditorGUI.IntField(minValueRect, (int)minValue);

            Rect sliderRect = new Rect(position.x + 45f, position.y, position.width - 90f, position.height);
            EditorGUI.MinMaxSlider(sliderRect, ref minValue, ref maxValue, _sliderAttribute.IntMin, _sliderAttribute.IntMax);

            Rect maxValueRect = new Rect(position.xMax - 40f, position.y, 40f, position.height);
            maxValue = EditorGUI.IntField(maxValueRect, (int)maxValue);

            property.vector2IntValue = new Vector2Int((int)minValue, (int)maxValue);
        }
        else
        {
            Rect labelRect = new Rect(position.x, position.y, position.width, position.height);

            EditorGUI.LabelField(labelRect, "Use Vector2 or Vector2Int Type");
        }

        EditorGUI.EndProperty();
    }
}
#endif