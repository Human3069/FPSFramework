using _KMH_Framework;
using UnityEngine;

namespace FPS_Framework.ZuluWar
{
    public class UI_CallArtillerySellable : UI_BaseSellable
    {
        protected override void OnClickButton()
        {
            _minimap.OpenWithCallArtillery(OnCallArtillery);
            KeyType.Toggle_Shop.SetToggleValue(false);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        protected void OnCallArtillery(Vector3 worldPoint)
        {
            Debug.Log("Call Artillery : " + worldPoint);

            GameManager.Instance._ArtilleryStrike.StrikeAsync(worldPoint, 5f).Forget();
        }
    }
}