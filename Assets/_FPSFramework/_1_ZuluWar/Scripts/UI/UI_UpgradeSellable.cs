using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace FPS_Framework.ZuluWar
{
    public class UI_UpgradeSellable : UI_BaseSellable
    {
        [Header("=== UI_UpgradeSellable ===")]
        [SerializeField]
        protected UI_FixedProgress progress;

        [Space(10)]
        [SerializeField]
        protected TextMeshProUGUI affectText;

        public enum UpgradeType
        {
            UpgradeArtilleryStrikeDuration,
            UpgradeArtilleryStrikeInterval,
            UpgradeArtilleryStrkieRadius,

            UpgradeRiflemanRange,
            UpgradeRiflemanFireInterval,
            UpgradeRiflemanAccuracy,
        }

        [SerializeField]
        protected UpgradeType upgradeType;

        protected virtual void Awake()
        {
            AwakeAsync().Forget();
        }

        protected async UniTaskVoid AwakeAsync()
        {
            await UniTask.WaitUntil(() => GameManager.Instance != null);

            UpdateText(upgradeType.ToCurrentValue());
        }

        protected override void OnPurchased()
        {
            int upgradeCount;
            float affectResult;

            switch (upgradeType)
            {
                case UpgradeType.UpgradeArtilleryStrikeDuration:
                    {
                        affectResult = GameManager.Instance._ArtilleryStrike.UpgradeFireDuration(out upgradeCount);
                        break;
                    }

                case UpgradeType.UpgradeArtilleryStrikeInterval:
                    {
                        affectResult = GameManager.Instance._ArtilleryStrike.UpgradeFireInterval(out upgradeCount);
                        break;
                    }

                case UpgradeType.UpgradeArtilleryStrkieRadius:
                    {
                        affectResult = GameManager.Instance._ArtilleryStrike.UpgradeFireRadius(out upgradeCount);
                        break;
                    }

                case UpgradeType.UpgradeRiflemanRange:
                    {
                        affectResult = GameManager.Instance._UnitStats.UpgradeRange(out upgradeCount);
                        break;
                    }

                case UpgradeType.UpgradeRiflemanFireInterval:
                    {
                        affectResult = GameManager.Instance._UnitStats.UpgradeFireInterval(out upgradeCount);
                        break;
                    }

                case UpgradeType.UpgradeRiflemanAccuracy:
                    {
                        affectResult = GameManager.Instance._UnitStats.UpgradeAccuracy(out upgradeCount);
                        break;
                    }

                default:
                    {
                        throw new System.NotImplementedException("upgradeType : " + upgradeType);
                    }
            }

            progress.Set((upgradeCount % progress.Max) + 1);
            _price += priceDelta;

            UpdateText(affectResult);
        }

        protected virtual void UpdateText(float affectResult)
        {
            UpdatePrice();

            upgradeType.ToAffectText(out string prefixAffect, out string suffixAffect);
            affectText.text = prefixAffect + " :\n" + affectResult.ToString("F2") + suffixAffect;
        }
    }

    public static class UpgradeTypeExtension
    {
        public static void ToAffectText(this UI_UpgradeSellable.UpgradeType upgradeType, out string prefix, out string suffix)
        {
            switch (upgradeType)
            {
                case UI_UpgradeSellable.UpgradeType.UpgradeArtilleryStrikeDuration:
                    prefix = "Duration";
                    suffix = " Sec";
                    break;

                case UI_UpgradeSellable.UpgradeType.UpgradeArtilleryStrikeInterval:
                    prefix = "Interval";
                    suffix = " Sec";
                    break;

                case UI_UpgradeSellable.UpgradeType.UpgradeArtilleryStrkieRadius:
                    prefix = "Radius";
                    suffix = "m";
                    break;

                case UI_UpgradeSellable.UpgradeType.UpgradeRiflemanRange:
                    prefix = "Range";
                    suffix = "m";
                    break;

                case UI_UpgradeSellable.UpgradeType.UpgradeRiflemanFireInterval:
                    prefix = "Interval";
                    suffix = " Sec";
                    break;

                case UI_UpgradeSellable.UpgradeType.UpgradeRiflemanAccuracy:
                    prefix = "Accuracy";
                    suffix = "";
                    break;

                default:
                    {
                        throw new System.NotImplementedException("upgradeType : " + upgradeType);
                    }
            }
        }

        public static float ToCurrentValue(this UI_UpgradeSellable.UpgradeType upgradeType)
        {
            switch (upgradeType)
            {
                case UI_UpgradeSellable.UpgradeType.UpgradeArtilleryStrikeDuration:
                    return GameManager.Instance._ArtilleryStrike.GetFireDuration();

                case UI_UpgradeSellable.UpgradeType.UpgradeArtilleryStrikeInterval:
                    return GameManager.Instance._ArtilleryStrike.GetFireInterval();

                case UI_UpgradeSellable.UpgradeType.UpgradeArtilleryStrkieRadius:
                    return GameManager.Instance._ArtilleryStrike.GetFireRadius();

                case UI_UpgradeSellable.UpgradeType.UpgradeRiflemanRange:
                    return GameManager.Instance._UnitStats.GetRange();

                case UI_UpgradeSellable.UpgradeType.UpgradeRiflemanFireInterval:
                    return GameManager.Instance._UnitStats.GetFireInterval();

                case UI_UpgradeSellable.UpgradeType.UpgradeRiflemanAccuracy:
                    return GameManager.Instance._UnitStats.GetAccuracy();

                default:
                    throw new System.NotImplementedException("upgradeType : " + upgradeType);
            }
        }
    }
}