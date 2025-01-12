using UnityEngine;

namespace FPS_Framework
{
    [System.Serializable]
    public struct DamagedLog
    {
        public DamagedLog(UnitType offenderType, UnitType victimType, float range, string damagedBy, float damageAmount, InjuriedType injuriedType, StateOnDamaged stateOnDamaged)
        {
            this.OffenderType = offenderType;
            this.VictimType = victimType;

            this.Range = range;
            this.DamagedBy = damagedBy;
            this.DamageAmount = damageAmount;
            this.InjuriedType = injuriedType;
            this._StateOnDamaged = stateOnDamaged;
        }

        public UnitType OffenderType;
        public UnitType VictimType;

        [Space(10)]
        public float Range;
        public string DamagedBy;
        public float DamageAmount;
        public InjuriedType InjuriedType;
        public StateOnDamaged _StateOnDamaged;

        public override string ToString()
        {
            if (_StateOnDamaged == StateOnDamaged.None)
            {
                return OffenderType + "(이)가 " + DamagedBy + "를 이용하여 빗맞혔습니다.";
            }
            else
            {
                return OffenderType + "(이)가 " + Range.ToString("F0") + "m 거리에 있는 " + VictimType + "에게 " + DamagedBy + " 를 이용하여 " + InjuriedType.ToLocalizedKorean() + " 부위에 " + DamageAmount + " 만큼의 피해를 받아 " + _StateOnDamaged.ToLocalizedKoreanSuffix();
            }
        }
    }
}