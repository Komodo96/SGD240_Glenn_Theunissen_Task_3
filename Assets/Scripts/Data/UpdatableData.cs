using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class for ScriptableObjects that can notify other systems when their values change
public class UpdatableData : ScriptableObject
{
    // Event triggered when values are updated
    public event System.Action OnValuesUpdated;

    // Enables automatic updates when values are changed in the inspector
    public bool autoUpdate;

#if UNITY_EDITOR

    // Called automatically when a value in the inspector is modified
    protected virtual void OnValidate()
    {
        if (autoUpdate)
        {
            // Registers an update callback so we can trigger OnValuesUpdated safely
            UnityEditor.EditorApplication.update += NotifyOfUpdatedValues;
        }
    }

    // Invokes the OnValuesUpdated event and unregisters the callback
    public void NotifyOfUpdatedValues()
    {
        UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues;

        if (OnValuesUpdated != null)
        {
            OnValuesUpdated();
        }
    }

#endif

}

