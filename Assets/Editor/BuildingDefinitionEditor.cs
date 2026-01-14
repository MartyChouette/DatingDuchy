#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using CozyTown.Build;

[CustomEditor(typeof(BuildingDefinition))]
public class BuildingDefinitionEditor : Editor
{
    SerializedProperty idProp;
    SerializedProperty displayNameProp;

    SerializedProperty kindProp;
    SerializedProperty footprintProp;

    SerializedProperty isRoadProp;
    SerializedProperty isShopProp;
    SerializedProperty isSocialNodeProp;

    SerializedProperty jobSlotsProp;
    SerializedProperty housingBedsProp;
    SerializedProperty taxValueProp;
    SerializedProperty beautyProp;
    SerializedProperty utilityProp;

    SerializedProperty heroPrefabProp;
    SerializedProperty monsterPrefabProp;
    SerializedProperty spawnEverySecondsProp;

    SerializedProperty prefabProp;
    SerializedProperty visualOffsetProp;
    SerializedProperty visualScaleProp;

    // Optional (only if you added these fields)
    SerializedProperty tavernSpendMinProp;
    SerializedProperty tavernSpendMaxProp;

    void OnEnable()
    {
        idProp = serializedObject.FindProperty("id");
        displayNameProp = serializedObject.FindProperty("displayName");

        kindProp = serializedObject.FindProperty("kind");
        footprintProp = serializedObject.FindProperty("footprint");

        isRoadProp = serializedObject.FindProperty("isRoad");
        isShopProp = serializedObject.FindProperty("isShop");
        isSocialNodeProp = serializedObject.FindProperty("isSocialNode");

        jobSlotsProp = serializedObject.FindProperty("jobSlots");
        housingBedsProp = serializedObject.FindProperty("housingBeds");
        taxValueProp = serializedObject.FindProperty("taxValue");
        beautyProp = serializedObject.FindProperty("beauty");
        utilityProp = serializedObject.FindProperty("utility");

        heroPrefabProp = serializedObject.FindProperty("heroPrefab");
        monsterPrefabProp = serializedObject.FindProperty("monsterPrefab");
        spawnEverySecondsProp = serializedObject.FindProperty("spawnEverySeconds");

        prefabProp = serializedObject.FindProperty("prefab");
        visualOffsetProp = serializedObject.FindProperty("visualOffset");
        visualScaleProp = serializedObject.FindProperty("visualScale");

        // Optional: only exists if you added them
        tavernSpendMinProp = serializedObject.FindProperty("tavernSpendMin");
        tavernSpendMaxProp = serializedObject.FindProperty("tavernSpendMax");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // --- Core ---
        EditorGUILayout.PropertyField(idProp);
        EditorGUILayout.PropertyField(displayNameProp);
        EditorGUILayout.Space(6);

        EditorGUILayout.PropertyField(kindProp);
        EditorGUILayout.PropertyField(footprintProp, true);
        EditorGUILayout.Space(6);

        // --- Tags ---
        EditorGUILayout.LabelField("Tags / Hooks", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(isRoadProp);
        EditorGUILayout.PropertyField(isShopProp);
        EditorGUILayout.PropertyField(isSocialNodeProp);
        EditorGUILayout.Space(6);

        // Determine kind
        var kind = (BuildingKind)kindProp.enumValueIndex;

        bool wantsJobs =
            kind == BuildingKind.Market ||
            kind == BuildingKind.Temple ||
            kind == BuildingKind.Tavern ||
            kind == BuildingKind.KnightsGuild; // optional: guild employs people

        bool wantsHousing = kind == BuildingKind.House;

        bool wantsTavern = kind == BuildingKind.Tavern;

        bool isGuild = kind == BuildingKind.KnightsGuild;
        bool isHive = kind == BuildingKind.MonsterHive;
        bool wantsSpawner = isGuild || isHive;

        // --- Simulation ---
        EditorGUILayout.LabelField("Simulation Hooks", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledGroupScope(!wantsJobs))
        {
            EditorGUILayout.PropertyField(jobSlotsProp);
        }

        using (new EditorGUI.DisabledGroupScope(!wantsHousing))
        {
            EditorGUILayout.PropertyField(housingBedsProp);
        }

        // Tax/beauty/utility are “general” in your current design,
        // but you can disable these too if you want.
        EditorGUILayout.PropertyField(taxValueProp);
        EditorGUILayout.PropertyField(beautyProp);
        EditorGUILayout.PropertyField(utilityProp);

        // Optional tavern spend fields (only if you added them)
        if (tavernSpendMinProp != null && tavernSpendMaxProp != null)
        {
            EditorGUILayout.Space(4);
            using (new EditorGUI.DisabledGroupScope(!wantsTavern))
            {
                EditorGUILayout.LabelField("Tavern Hooks", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(tavernSpendMinProp);
                EditorGUILayout.PropertyField(tavernSpendMaxProp);
            }
        }

        EditorGUILayout.Space(8);

        // --- Spawner Hooks ---
        EditorGUILayout.LabelField("Spawner Hooks", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledGroupScope(!isGuild))
        {
            EditorGUILayout.PropertyField(heroPrefabProp);
        }

        using (new EditorGUI.DisabledGroupScope(!isHive))
        {
            EditorGUILayout.PropertyField(monsterPrefabProp);
        }

        using (new EditorGUI.DisabledGroupScope(!wantsSpawner))
        {
            EditorGUILayout.PropertyField(spawnEverySecondsProp);
        }

        if (!wantsSpawner)
        {
            EditorGUILayout.HelpBox("Spawner fields are only used by KnightsGuild and MonsterHive.", MessageType.Info);
        }

        EditorGUILayout.Space(8);

        // --- Visuals ---
        EditorGUILayout.LabelField("Prefab / Visuals", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(prefabProp);
        EditorGUILayout.PropertyField(visualOffsetProp);
        EditorGUILayout.PropertyField(visualScaleProp);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
