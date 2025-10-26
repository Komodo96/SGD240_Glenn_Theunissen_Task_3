using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used to hide objects in the scene when the play button is pressed
public class HideOnPlay : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(false);
    }
}
