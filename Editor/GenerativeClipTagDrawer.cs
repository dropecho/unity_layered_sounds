using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dropecho {
  [CustomPropertyDrawer(typeof(GenerativeClipTag))]
  public class GenerativeClipTagDrawer : PropertyDrawer {
    // IMGUI drawer override
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      var tagProp = property.FindPropertyRelative("tag");
      var minValueProp = property.FindPropertyRelative("minValue");
      var maxValueProp = property.FindPropertyRelative("maxValue");
      var valueMapProp = property.FindPropertyRelative("valueMap");
      var tag = tagProp.stringValue;
      label.text = string.IsNullOrWhiteSpace(tag) || !label.text.Contains("Element") ? label.text : tag;

      position.height -= 2;

      // Using BeginProperty / EndProperty on the parent property means that
      // prefab override logic works on the entire property.
      EditorGUI.BeginProperty(position, label, property);

      // Draw label
      position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

      // Don't make child fields be indented
      var indent = EditorGUI.indentLevel;
      EditorGUI.indentLevel = 0;

      // Calculate rects
      var tagRect = new Rect(position.x, position.y, 50, position.height);
      var minValueRect = new Rect(position.x + 55, position.y, 30, position.height);
      var sliderValueRect = new Rect(position.x + 90, position.y, 200, position.height);
      var maxValueRect = new Rect(position.x + 295, position.y, 30, position.height);
      // var valueMapRect = new Rect(position.x + 260, position.y, 100, position.height);

      // Draw fields - pass GUIContent.none to each so they are drawn without labels
      EditorGUI.PropertyField(tagRect, tagProp, GUIContent.none);
      // valueProp.floatValue = EditorGUI.Slider(valueRect, valueProp.floatValue, 0, 1);
      
      EditorGUI.PropertyField(minValueRect, minValueProp, GUIContent.none);
      EditorGUI.PropertyField(maxValueRect, maxValueProp, GUIContent.none);

      float min = minValueProp.floatValue;
      float max = maxValueProp.floatValue;
      EditorGUI.MinMaxSlider(sliderValueRect, "", ref min, ref max, 0, 1);
      minValueProp.floatValue = min;
      maxValueProp.floatValue = max;
      // EditorGUI.PropertyField(valueMapRect, valueMapProp, GUIContent.none);

      // Set indent back to what it was
      EditorGUI.indentLevel = indent;

      EditorGUI.EndProperty();
    }

    // UI Elements drawer override.
    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
      // Create property container element.
      var container = new VisualElement();

      // Create property fields.
      var tagField = new PropertyField(property.FindPropertyRelative("tag"));
      var valueField = new PropertyField(property.FindPropertyRelative("value"));
      var valueMapField = new PropertyField(property.FindPropertyRelative("valueMap"));

      // Add fields to the container.
      container.Add(tagField);
      container.Add(valueField);
      container.Add(valueMapField);

      return container;
    }
  }
}