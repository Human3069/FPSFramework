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
            UpgradeArtilleryStrkieRadius,

            UpgradeRiflemanRange,
            UpgradeRiflemanFireInterval,
        }

        [SerializeField]
        protected UpgradeType upgradeType;

        protected override void OnPurchased()
        {
            int upgradeCount;
            switch (upgradeType)
            {
                case UpgradeType.UpgradeArtilleryStrikeDuration:
                    {
                        GameManager.Instance._ArtilleryStrike.UpgradeFireDuration(out upgradeCount);
                        break;
                    }

                case UpgradeType.UpgradeArtilleryStrikeInterval:
                    {
                        GameManager.Instance._ArtilleryStrike.UpgradeFireInterval(out upgradeCount);
                        break;
                    }

                case UpgradeType.UpgradeArtilleryStrkieRadius:
                    {
                        GameManager.Instance._ArtilleryStrike.UpgradeFireRadius(out upgradeCount);
                        break;
                    }

                case UpgradeType.UpgradeRiflemanRange:
                    {
                        GameManager.Instance._UnitStats.UpgradeRange(out upgradeCount);
                        break;
                    }

                case UpgradeType.UpgradeRiflemanFireInterval:
                    {
                        GameManager.Instance._UnitStats.UpgradeFireInterval(out upgradeCount);
                        break;
                    }

                default:
                    {
                        throw new System.NotImplementedException("upgradeType : " + upgradeType);
                    }
            }

            progress.Set(upgradeCount % progress.Max);
            _price += priceDelta;
        }
    }
}