using _KMH_Framework;
using Cysharp.Threading.Tasks;
using FPS_Framework.ZuluWar;
using NPOI.Util;
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

        protected bool isShopOpened = false;

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
            await UniTask.WaitUntil(() => KeyInputManager.Instance != null);

            KeySetting shopSetting = KeyInputManager.Instance.KeyData["Open/Close Shop"];
            shopSetting.OnValueChanged += OnValueChangedOpenCloseShopToggle;
            string keyCode = shopSetting._KeyCode.ToString();
            shopTitleText.text = "toggle \'" + keyCode + "\' to open";

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
            if (isOn == true)
            {
                isShopOpened = !isShopOpened;

                Cursor.visible = isShopOpened;
                Cursor.lockState = isShopOpened ? CursorLockMode.None : CursorLockMode.Locked;

                UniTaskEx.Cancel(this, 0);
                OnValueChangedOpenCloseShopToggleAsync(isShopOpened).Forget();
            }
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

            interactableText.text = "Press \'" + KeyInputManager.Instance.KeyData["Interact"]._KeyCode + "\' to Equip";
            interactableText.enabled = isEquipable;
        }

        protected void OnSeatableValueChanged(bool isSeatable)
        {
            // Debug.LogFormat(LOG_FORMAT, "OnSeatableValueChanged(), isSeatable : " + isSeatable);

            interactableText.text = "Press \'" + KeyInputManager.Instance.KeyData["Interact"]._KeyCode + "\' to Seat";
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