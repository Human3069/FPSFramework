using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FPS_Framework.ZuluWar
{
    public class UI_Minimap : MonoBehaviour
    {
        [SerializeField]
        protected Camera minimapCamera;

        [Space(10)]
        [SerializeField]
        protected RectTransform onRect;
        [SerializeField]
        protected RectTransform offRect;

        protected bool _isOn = false;
        public bool IsOn
        {
            get
            {
                return _isOn;
            }
            set
            {
                if (_isOn != value)
                {
                    _isOn = value;

                    onRect.gameObject.SetActive(value);
                    offRect.gameObject.SetActive(!value);
                }
            }
        }

        public delegate void CallArtilleryCallback(Vector3 worldPoint);

        public void OpenWithCallArtillery(CallArtilleryCallback onCallArtillery)
        {
            IsOn = true;
            OpenWithCallArtilleryAsync(onCallArtillery).Forget();
        }

        protected async UniTaskVoid OpenWithCallArtilleryAsync(CallArtilleryCallback onCallArtillery)
        {
            while (true)
            {
                if (Input.GetMouseButtonDown(0) == true && ConvertMousePositionToViewportPoint(onRect, Input.mousePosition, out Vector2 viewportPoint) == true)
                {
                    Ray viewPointRay = minimapCamera.ViewportPointToRay(viewportPoint);

                    if (Physics.Raycast(viewPointRay, out RaycastHit hit, Mathf.Infinity) == true)
                    {
                        IsOn = false;
                        Cursor.visible = false;
                        Cursor.lockState = CursorLockMode.Locked;

                        onCallArtillery?.Invoke(hit.point);
                        return;
                    }
                }

                await UniTask.Yield();
            }
        }

        protected bool ConvertMousePositionToViewportPoint(RectTransform rect, Vector2 mousePosition, out Vector2 viewportPoint)
        {
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, mousePosition, null, out localPoint) == true)
            {
                viewportPoint = new Vector2((localPoint.x - rect.rect.x) / rect.rect.width, (localPoint.y - rect.rect.y) / rect.rect.height);
  
                return viewportPoint.x >= 0f &&
                       viewportPoint.x <= 1f &&
                       viewportPoint.y >= 0f &&
                       viewportPoint.y <= 1f;
            }
            else
            {
                viewportPoint = Vector2.zero;
                return false;
            }
        }

        protected void Awake()
        {
            onRect.gameObject.SetActive(false);
            offRect.gameObject.SetActive(true);
        }
    }
}