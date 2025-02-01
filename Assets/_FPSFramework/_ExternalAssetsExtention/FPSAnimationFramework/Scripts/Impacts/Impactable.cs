using FPS_Framework.Pool;
using FPS_Framework.ZuluWar;
using UnityEngine;

namespace FPS_Framework
{
    [RequireComponent(typeof(Collider))]
    public class Impactable : MonoBehaviour
    {
        public ImpactType ImpactType;
        public float Thickness = 1f;

        [Space(10)]
        public MeleeWarriorController Warrior;
        public InjuriedType _InjuriedType = InjuriedType.None;
        public float DamageMultiplier = 1f;
    }

    public enum InjuriedType
    {
        None,

        Head,
        Body,

        RightUpperArm,
        RightLowerArm,
        RightHand,

        LeftUpperArm,
        LeftLowerArm,
        LeftHand,

        RightUpperLeg,
        RightLowerLeg,
        RightFoot,

        LeftUpperLeg,
        LeftLowerLeg,
        LeftFoot,

        Splash,
    }

    public enum UnitType
    {
        Player,
        Ally,
        Enemy,
    }

    public enum StateOnDamaged
    {
        None,
        Wounded,
        Dead,
    }

    public static class EnumExtentionMethods
    {
        public static StateOnDamaged ToStateOnDamaged(this float currentHealth)
        {
            if (currentHealth > 0)
            {
                return StateOnDamaged.Wounded;
            }
            else
            {
                return StateOnDamaged.Dead;
            }
        }

        public static string ToLocalizedKoreanSuffix(this StateOnDamaged stateOnDamaged)
        {
            switch (stateOnDamaged)
            {
                case StateOnDamaged.Wounded:
                    return "�λ���Ͽ����ϴ�.";

                case StateOnDamaged.Dead:
                    return "����Ͽ����ϴ�.";

                default:
                    return "";
            }
        }

        public static string ToLocalizedKorean(this InjuriedType injuriedType)
        {
            switch (injuriedType)
            {
                case InjuriedType.Head:
                    return "�Ӹ�";
                case InjuriedType.Body:
                    return "��";

                case InjuriedType.RightUpperArm:
                    return "������ ���";
                case InjuriedType.RightLowerArm:
                    return "������ �ȶ�";
                case InjuriedType.RightHand:
                    return "������";

                case InjuriedType.LeftUpperArm:
                    return "���� ���";
                case InjuriedType.LeftLowerArm:
                    return "���� �ȶ�";
                case InjuriedType.LeftHand:
                    return "�޼�";

                case InjuriedType.RightUpperLeg:
                    return "������ �����";
                case InjuriedType.RightLowerLeg:
                    return "������ ���Ƹ�";
                case InjuriedType.RightFoot:
                    return "������";

                case InjuriedType.LeftUpperLeg:
                    return "���� �����";
                case InjuriedType.LeftLowerLeg:
                    return "���� ���Ƹ�";
                case InjuriedType.LeftFoot:
                    return "�޹�";

                default:
                    return "����";
            }
        }
    }
}