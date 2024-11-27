using UnityEditor;
using com.ctn_originals.unity_drawif_attributes;

namespace Utilities
{
    public static class SerializedPropertyExtentions
    {
#if UNITY_EDITOR
        public static T GetValue<T>(this SerializedProperty property)
        {
            return ReflectionUtil.GetNestedObject<T>(property.serializedObject.targetObject, property.propertyPath);
        }
#endif
    }
}