using _KMH_Framework;
using Cysharp.Threading.Tasks;
using FPS_Framework.ZuluWar;
using NPOI.Util;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FPS_Framework
{
    public class UI_Ingame : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[UI_Ingame]</b></color> {0}";

        [HideInInspector]
        public SingleFPSPlayer _SingleFPSPlayer;

        [Header("Components")]
        [SerializeField]
        protected UI_Minimap minimap;

        [Header("Interactable")]
        [SerializeField]
        protected Camera mainCamera;
        [SerializeField]
        protected TMP_Text interactableText;

        [Header("Toolbars")]
        [SerializeField]
        protected TMP_Text totalMoneyText;
        [SerializeField]
        protected TMP_Text remainedEnemiesCountText;

        [Header("Shop")]
        [SerializeField]
        protected RectTransform shopPanel;
        [SerializeField]
        protected TextMeshProUGUI shopTitleText;
        [SerializeField]
        protected GridLayoutGroup gridLayoutGroup;

        [Header("Predictor")]
        [SerializeField]
        protected RectTransform predictPanel;
        [SerializeField]
        protected TextMeshProUGUI predictDistanceText;

        [Header("Bottombars")]
        [SerializeField]
        protected TMP_Text ammoText;

        protected void Awake()
        {
            interactableText.enabled = false;
            shopPanel.localRotation = Quaternion.Euler(0f, -90f, 0f);

            Debug.Log(Display.main.renderingWidth);
            float remainedX = Display.main.renderingWidth + shopPanel.sizeDelta.x;
            gridLayoutGroup.cellSize = new Vector2(remainedX / gridLayoutGroup.constraintCount, 150f);

            AwakeAsync().Forget();
        }

        protected async UniTaskVoid AwakeAsync()
        {
            await KeyType.Toggle_Shop.RegisterEventAsync(OnValueChangedOpenCloseShopToggle);
            shopTitleText.text = "toggle \'" + KeyType.Toggle_Shop.GetKeyName() + "\' to open";

            await UniTask.WaitUntil(() => _SingleFPSPlayer != null);

            _SingleFPSPlayer.FPSController.OnEquipableValueChanged += OnEquipableValueChanged;
            _SingleFPSPlayer.FPSController.OnSeatableValueChanged += OnSeatableValueChanged;
            _SingleFPSPlayer.FPSController.OnEquipedWeaponChanged += OnEquipedWeaponChanged;

            PhaseCounter.OnTotalMoneyChanged += OnTotalMoneyChanged;
            OnTotalMoneyChanged(0); // forcelly call!

            PhaseCounter.OnRemainedEnemyCountChanged += OnRemainedEnemyCountChanged;
            OnRemainedEnemyCountChanged(0); // forcelly call!
        }

        protected void OnValueChangedOpenCloseShopToggle(bool isOn)
        {
            Cursor.visible = isOn;
            Cursor.lockState = isOn ? CursorLockMode.None : CursorLockMode.Locked;

            UniTaskEx.Cancel(this, 0);
            OnValueChangedOpenCloseShopToggleAsync(isOn).Forget();
        }

        protected async UniTaskVoid OnValueChangedOpenCloseShopToggleAsync(bool isOpened)
        {
            Quaternion targetRot = (isOpened == true) ? Quaternion.Euler(0f, 0f, 0f) : Quaternion.Euler(0f, -90f, 0f);
            Quaternion currentRot = shopPanel.localRotation;

            while (currentRot != targetRot)
            {
                currentRot = Quaternion.RotateTowards(currentRot, targetRot, 360f * Time.unscaledDeltaTime);
                shopPanel.localRotation = currentRot;

                await UniTaskEx.Yield(this, 0);
            }
        }

        protected void OnEquipableValueChanged(bool isEquipable)
        {
            // Debug.LogFormat(LOG_FORMAT, "OnEquipableValueChanged(), isEquipable : " + isEquipable);

            interactableText.text = "Press \'" + KeyType.Interact.GetKeyName() + "\' to Equip";
            interactableText.enabled = isEquipable;
        }

        protected void OnSeatableValueChanged(bool isSeatable)
        {
            // Debug.LogFormat(LOG_FORMAT, "OnSeatableValueChanged(), isSeatable : " + isSeatable);

            interactableText.text = "Press \'" + KeyType.Interact.GetKeyName() + "\' to Seat";
            interactableText.enabled = isSeatable;
        }

        protected void OnEquipedWeaponChanged(WeaponEx currentWeapon, WeaponEx equipedWeapon)
        {
            if (currentWeapon != null)
            {
                currentWeapon.OnAmmoValuesChanged -= OnAmmoValuesChanged;
                currentWeapon.OnPredictPointCalculated -= OnPredictPointCalculated;
            }
            equipedWeapon.OnAmmoValuesChanged += OnAmmoValuesChanged;
            equipedWeapon.OnPredictPointCalculated += OnPredictPointCalculated;
        }

        protected void OnTotalMoneyChanged(int totalMoney)
        {
            totalMoneyText.text = "Money : " + totalMoney.ToString();
        }

        protected void OnRemainedEnemyCountChanged(int remainedEnemyCount)
        {
            remainedEnemiesCountText.text = "Remained : " + remainedEnemyCount.ToString();
        }

        protected void OnAmmoValuesChanged(int maxAmmo, int currentAmmo)
        {
            ammoText.text = currentAmmo + " / " + maxAmmo;
        }

        protected void OnPredictPointCalculated(Vector3 predictedHitPos, float distance)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(predictedHitPos);
            predictPanel.position = screenPos;
            predictDistanceText.text = distance.ToString("F0") + "m";
        }
    }
}