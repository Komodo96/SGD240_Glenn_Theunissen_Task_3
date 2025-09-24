using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// Custom editor for the MapGenerator script, allows adding extra functionality in the Inspector
[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    // Overrides the default Inspector GUI for MapGenerator
    public override void OnInspectorGUI()
    {
        // Get a reference to the MapGenerator component this editor is inspecting
        MapGenerator mapGen = (MapGenerator)target;

        if (DrawDefaultInspector())
        {
            // If any value was changed in the inspector and autoUpdate is true, regenerate the map
            if (mapGen.autoUpdate)
            {
                mapGen.DrawMapInEditor();
            }
        }

        // Adds a button to the inspector labeled Generate
        if (GUILayout.Button("Generate"))
        {
            // When clicked, call GenerateMap to update the map
            mapGen.DrawMapInEditor();
        }
    }
}
