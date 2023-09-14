using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentDisabler : MonoBehaviour
{
    public Behaviour component;
    public KeyCode key;

    private void Update()
    {
        if(Input.GetKeyDown(key))
        {
            component.enabled = !component.enabled;
        }
    }
}
