using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Attributes))]
public class AttributeSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Attributes attributeSystem = (Attributes)target;

        if (attributeSystem.AttributesList == null)
        {
            attributeSystem.AttributesList = new List<Attribute>();
        }

        List<int> deleteIndexList = new List<int>();

        for (int i = 0; i < attributeSystem.AttributesList.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            if (attributeSystem.AttributesList[i].key == null)
            {
                attributeSystem.AttributesList[i].key = EditorGUILayout.TextField("");
            }
            else
            {
                attributeSystem.AttributesList[i].key = EditorGUILayout.TextField(attributeSystem.AttributesList[i].key).ToLower();
            }
            

            attributeSystem.AttributesList[i].value.type = (AttributeValue.ValueType)EditorGUILayout.EnumPopup(attributeSystem.AttributesList[i].value.type);

            switch (attributeSystem.AttributesList[i].value.type)
            {
                case AttributeValue.ValueType.Float:
                    attributeSystem.AttributesList[i].value.floatValue = EditorGUILayout.FloatField(attributeSystem.AttributesList[i].value.floatValue);
                    break;
                case AttributeValue.ValueType.Int:
                    attributeSystem.AttributesList[i].value.intValue = EditorGUILayout.IntField(attributeSystem.AttributesList[i].value.intValue);
                    break;
                case AttributeValue.ValueType.String:
                    attributeSystem.AttributesList[i].value.stringValue = EditorGUILayout.TextField(attributeSystem.AttributesList[i].value.stringValue);
                    break;
            }

            attributeSystem.AttributesList[i].value.networked = EditorGUILayout.Toggle("Networked", attributeSystem.AttributesList[i].value.networked);

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                deleteIndexList.Add(i);
            }

            EditorGUILayout.EndHorizontal();
        }

        foreach (int index in deleteIndexList)
        {
            attributeSystem.AttributesList.RemoveAt(index);
        }

        if (GUILayout.Button("Add Attribute"))
        {
            attributeSystem.AttributesList.Add(new Attribute() { value = new AttributeValue() });
        }

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}