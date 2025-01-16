using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;

namespace _KMH_Framework
{
    public class FPSPlayerWeaponHandler : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[FPSPlayerWeaponHandler]</b></color> {0}";

        [SerializeField]
        protected Transform headTransform;
        [SerializeField]
        protected UI_FPSPlayer uiHandler;

        [Header("Equipable")]
        [ReadOnly]
        [SerializeField]
        protected BaseFPSWeapon _equipedWeapon = null;
        public BaseFPSWeapon EquipedWeapon
        {
            get
            {
                return _equipedWeapon;
            }
        }

        [SerializeField]
        protected float equipableDistance;

        [Space(10)]
        [SerializeField]
        protected Transform equipedParent;

        protected virtual void Awake()
        {
            PostAwake().Forget();
        }

        protected async UniTaskVoid PostAwake()
        {
            await KeyType.Change_FireMode.RegisterEventAsync(OnSelectionValueChanged);
        }

        protected void OnSelectionValueChanged(bool _value)
        {
            if (_equipedWeapon != null)
            {
                if (_value == true)
                {
                    if (_equipedWeapon._SelectionMode == BaseFPSWeapon.SelectionMode.SemiAuto)
                    {
                        _equipedWeapon._SelectionMode = BaseFPSWeapon.SelectionMode._2Burst;
                    }
                    else if (_equipedWeapon._SelectionMode == BaseFPSWeapon.SelectionMode._2Burst)
                    {
                        _equipedWeapon._SelectionMode = BaseFPSWeapon.SelectionMode._3Burst;
                    }
                    else if (_equipedWeapon._SelectionMode == BaseFPSWeapon.SelectionMode._3Burst)
                    {
                        _equipedWeapon._SelectionMode = BaseFPSWeapon.SelectionMode.FullAuto;
                    }
                    else if (_equipedWeapon._SelectionMode == BaseFPSWeapon.SelectionMode.FullAuto)
                    {
                        _equipedWeapon._SelectionMode = BaseFPSWeapon.SelectionMode.SemiAuto;
                    }
                    else
                    {
                        Debug.Assert(false);
                    }
                }
            }
        }

        protected void Update()
        {
            if (Physics.Raycast(headTransform.position, headTransform.forward, out RaycastHit _raycastHit, equipableDistance) == true)
            {
                if (_raycastHit.collider.TryGetComponent<BaseFPSWeapon>(out BaseFPSWeapon rayHitWeapon) == true)
                {
                    uiHandler.IsEquipable = true;
                    if (KeyType.Interact.IsInput() == true)
                    {
                        _equipedWeapon = rayHitWeapon;

                        _equipedWeapon.transform.parent = equipedParent;
                        _equipedWeapon.transform.localPosition = Vector3.zero;
                        _equipedWeapon.transform.localEulerAngles = Vector3.zero;

                        _equipedWeapon.Initialize(uiHandler);
                        uiHandler.SetAmmoText(_equipedWeapon.ExceptMagCount, _equipedWeapon.CurrentMagCount);
                    }
                }
                else
                {
                    uiHandler.IsEquipable = false;
                }
            }

            if (_equipedWeapon != null)
            {
                _equipedWeapon.IsTriggering = Input.GetMouseButton(0);

                if (KeyType.Reload.IsInput() == true)
                {
                    if (_equipedWeapon.AmmoPerMag > _equipedWeapon.CurrentMagCount)
                    {
                        if (_equipedWeapon.IsReloading == false)
                        {
                            StartCoroutine(_equipedWeapon.ReloadAsync());
                        }
                    }
                }
            }
        }
    }
}