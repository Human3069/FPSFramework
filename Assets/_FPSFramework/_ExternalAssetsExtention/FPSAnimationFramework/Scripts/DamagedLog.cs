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
                return OffenderType + "(��)�� " + DamagedBy + "�� �̿��Ͽ� ���������ϴ�.";
            }
            else
            {
                return OffenderType + "(��)�� " + Range.ToString("F0") + "m �Ÿ��� �ִ� " + VictimType + "���� " + DamagedBy + " �� �̿��Ͽ� " + InjuriedType.ToLocalizedKorean() + " ������ " + DamageAmount + " ��ŭ�� ���ظ� �޾� " + _StateOnDamaged.ToLocalizedKoreanSuffix();
            }
        }
    }
}