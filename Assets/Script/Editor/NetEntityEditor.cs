using Game;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

[CustomEditor(typeof(NetEntity))]
public class NetEntityEditor : Editor
{
    SerializedProperty addressProperty;
    private void OnEnable()
    {
        addressProperty = serializedObject.FindProperty("Address");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update(); // Update the serialized object

        NetEntity entity = (NetEntity)target;

        EditorGUILayout.LabelField(addressProperty.stringValue);

        if( Application.isPlaying)
        {
            EditorGUILayout.LabelField(entity.Owned ? "LOCAL" : "REMOTE");
        }


        PrefabInstanceStatus prefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus(entity.gameObject);
        bool isPrefabInstance = prefabInstanceStatus != PrefabInstanceStatus.NotAPrefab;

        if(!isPrefabInstance)
        {
            string assetId = GetAddressableId(entity.gameObject);
            if (entity.UpdateAssetId(assetId))
            {
                EditorUtility.SetDirty(entity);  // Mark the object as dirty to ensure changes are saved
            }
        }

        serializedObject.ApplyModifiedProperties(); // Apply the modified properties to the actual target object
    }

    private string GetAddressableId(GameObject go)
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

        if (settings == null)
        {
            Debug.LogWarning("Addressable Asset Settings not found!");
            return string.Empty;
        }

        string assetPath = AssetDatabase.GetAssetPath(go);
        string assetGUID = AssetDatabase.AssetPathToGUID(assetPath);

        AddressableAssetEntry entry = settings.FindAssetEntry(assetGUID);
        if (entry != null)
        {
            return entry.address;
        }
        else
        {
            return string.Empty;
        }
    }
}