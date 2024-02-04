using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPS_Framework
{
    public class UI_Ingame : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[UI_Ingame]</b></color> {0}";

        [SerializeField]
        protected GameObject equipablePanelObj;

        protected void Awake()
        {
            equipablePanelObj.SetActive(false);

            FPSControllerEx.OnEquipableValueChanged += OnEquipableValueChanged;
        }

        protected void OnDestroy()
        {
            FPSControllerEx.OnEquipableValueChanged -= OnEquipableValueChanged;
        }

        protected void OnEquipableValueChanged(bool isEquipable)
        {
            Debug.LogFormat(LOG_FORMAT, "OnEquipableValueChanged(), isEquipable : " + isEquipable);

            equipablePanelObj.SetActive(isEquipable);
        }
    }
}