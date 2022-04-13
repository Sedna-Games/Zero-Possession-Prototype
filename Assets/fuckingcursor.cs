using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fuckingcursor : MonoBehaviour
{
    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }
}
