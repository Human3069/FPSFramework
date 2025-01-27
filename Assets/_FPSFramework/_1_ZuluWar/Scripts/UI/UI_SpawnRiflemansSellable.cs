using _KMH_Framework;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FPS_Framework.ZuluWar
{
    public class UI_SpawnRiflemansSellable : UI_BaseSellable
    {
        [Header("=== UI_SpawnRiflemansSellable ===")]
        [SerializeField]
        protected Installable installablePrefab;

        [Space(10)]
        [SerializeField]
        protected float maxRayDistance = 10f;

        protected enum SpawnState
        {
            _0_None,
            _1_Spawned,
            _2_Rotating,
        }

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected SpawnState spawnState = SpawnState._0_None;
        [SerializeField]
        protected Camera mainCamera;

        protected override void OnPurchased()
        {
            spawnState = SpawnState._1_Spawned;
            OnPurchasedAsync().Forget();
        }

        protected async UniTaskVoid OnPurchasedAsync()
        {
            FPSControllerEx.Instance.UpdateMouseShowState(false);
            KeyType.Toggle_Shop.UpdateLock(false);
            KeyType.Toggle_Shop.SetToggleValue(false);

            InstallableDummy installableDummy = Instantiate(installablePrefab.DummyPrefab);

            while (spawnState == SpawnState._1_Spawned)
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance) == true)
                {
                    installableDummy.transform.position = hit.point;

                    if (Input.GetMouseButtonDown(0) == true)
                    {
                        spawnState = SpawnState._2_Rotating;

                        installableDummy.SetUnlifted(hit.point);
                        OnRotatingAsync(installableDummy.gameObject).Forget();
                    }
                }

                await UniTask.Yield();
            }
        }

        protected async UniTaskVoid OnRotatingAsync(GameObject dummyObj)
        {
            KeyType.Move_Forward.UpdateLock(true);
            KeyType.Move_Backward.UpdateLock(true);
            KeyType.Move_Right.UpdateLock(true);
            KeyType.Move_Left.UpdateLock(true);
            KeyType.Jump.UpdateLock(true);

            while (spawnState == SpawnState._2_Rotating)
            {
                await UniTask.Yield();

                float rotating = 0f;
                if (Input.GetKey(KeyCode.A) == true)
                {
                    rotating = 100f;
                }
                else if (Input.GetKey(KeyCode.D) == true)
                {
                    rotating = -100f;
                }

                dummyObj.transform.Rotate(Vector3.up, rotating * Time.deltaTime);

                if (Input.GetMouseButtonDown(0) == true)
                {
                    spawnState = SpawnState._0_None;
                    
                    Vector3 dummyPos = dummyObj.transform.position;
                    Instantiate(installablePrefab, dummyPos, dummyObj.transform.rotation);
                    
                    Destroy(dummyObj);
                }
            }

            KeyType.Move_Forward.UpdateLock(false);
            KeyType.Move_Backward.UpdateLock(false);
            KeyType.Move_Right.UpdateLock(false);
            KeyType.Move_Left.UpdateLock(false);
            KeyType.Jump.UpdateLock(false);
        }
    }
}