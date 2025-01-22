using _KMH_Framework;
using UnityEngine;

namespace FPS_Framework.ZuluWar
{
    public class UI_CallArtillerySellable : UI_BaseSellable
    {
        protected override void OnPurchased()
        {
            _minimap.OpenWithCallArtillery(OnCallArtillery);

            KeyType.Toggle_Shop.UpdateLock(true);
            KeyType.Toggle_Shop.SetToggleValue(false);
            FPSControllerEx.Instance.UpdateMouseShowState(true);
        }

        protected void OnCallArtillery(Vector3 worldPoint)
        {
            GameManager.Instance._ArtilleryStrike.StrikeAsync(worldPoint).Forget();

            FPSControllerEx.Instance.UpdateMouseShowState(false);
            KeyType.Toggle_Shop.UpdateLock(false);
        }
    }
}