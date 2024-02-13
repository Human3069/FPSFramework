using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FPS_Framework
{
    public class UI_Ingame : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[UI_Ingame]</b></color> {0}";

        [HideInInspector]
        public SingleFPSPlayer _SingleFPSPlayer;

        [SerializeField]
        protected GameObject equipablePanelObj;

        [Space(10)]
        [SerializeField]
        protected TMP_Text ammoText;

        protected void Awake()
        {
            equipablePanelObj.SetActive(false);

            StartCoroutine(PostAwake());
        }

        protected IEnumerator PostAwake()
        {
            while (_SingleFPSPlayer == null)
            {
                Debug.LogFormat(LOG_FORMAT, "yield return null");

                yield return null;
            }

            _SingleFPSPlayer.FPSController.OnEquipableValueChanged += OnEquipableValueChanged;
            _SingleFPSPlayer.FPSController.OnEquipedWeaponChanged += OnEquipedWeaponChanged;
        }

        protected void OnDestroy()
        {
            if (_SingleFPSPlayer != null)
            {
                _SingleFPSPlayer.FPSController.OnEquipedWeaponChanged -= OnEquipedWeaponChanged;
                _SingleFPSPlayer.FPSController.OnEquipableValueChanged -= OnEquipableValueChanged;
            }
        }

        protected void OnEquipableValueChanged(bool isEquipable)
        {
            Debug.LogFormat(LOG_FORMAT, "OnEquipableValueChanged(), isEquipable : " + isEquipable);

            equipablePanelObj.SetActive(isEquipable);
        }

        protected void OnEquipedWeaponChanged(WeaponEx currentWeapon, WeaponEx equipedWeapon)
        {
            if (currentWeapon != null)
            {
                currentWeapon.OnAmmoValuesChanged -= OnAmmoValuesChanged;
            }
            equipedWeapon.OnAmmoValuesChanged += OnAmmoValuesChanged;
        }

        protected void OnAmmoValuesChanged(int maxAmmo, int currentAmmo)
        {
            ammoText.text = currentAmmo + " / " + maxAmmo;
        }
    }
}