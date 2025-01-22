using UnityEngine;

namespace FPS_Framework.ZuluWar
{
    public class UI_UpgradeSellable : UI_BaseSellable
    {
        [Header("=== UI_UpgradeSellable ===")]
        [SerializeField]
        protected UI_FixedProgress progress;

        public enum UpgradeType
        {
            UpgradeArtilleryStrikeDuration,
            UpgradeArtilleryStrikeInterval,
        }

        [SerializeField]
        protected UpgradeType upgradeType;

        protected override void OnPurchased()
        {
            switch (upgradeType)
            {
                case UpgradeType.UpgradeArtilleryStrikeDuration:
                    {
                        int upgradeCount = GameManager.Instance._ArtilleryStrike.UpgradeFireDuration();
                        progress.Set(upgradeCount % progress.Max);
                        _price += priceDelta;

                        break;
                    }

                case UpgradeType.UpgradeArtilleryStrikeInterval:
                    {
                        int upgradeCount = GameManager.Instance._ArtilleryStrike.UpgradeFireInterval();
                        progress.Set(upgradeCount % progress.Max);
                        _price += priceDelta;

                        break;
                    }

                default:
                    throw new System.NotImplementedException("upgradeType : " + upgradeType);
            }
        }
    }
}