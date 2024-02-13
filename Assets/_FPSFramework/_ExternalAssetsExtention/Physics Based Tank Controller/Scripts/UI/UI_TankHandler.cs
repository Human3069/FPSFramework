using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_TankHandler : MonoBehaviour
{
    [SerializeField]
    protected Camera tankCamera;

    [Space(10)]
    [SerializeField]
    protected Transform aimDotTransform;
    [SerializeField]
    protected Transform aimDotImageTransform;

    protected virtual void Update()
    {
        Vector3 uiPosition = tankCamera.WorldToScreenPoint(aimDotTransform.position);
        uiPosition.z = 0;

        aimDotImageTransform.position = uiPosition;
    }
}
