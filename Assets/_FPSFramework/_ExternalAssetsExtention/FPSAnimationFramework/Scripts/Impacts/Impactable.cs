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
                    return "부상당하였습니다.";

                case StateOnDamaged.Dead:
                    return "사망하였습니다.";

                default:
                    return "";
            }
        }

        public static string ToLocalizedKorean(this InjuriedType injuriedType)
        {
            switch (injuriedType)
            {
                case InjuriedType.Head:
                    return "머리";
                case InjuriedType.Body:
                    return "몸";

                case InjuriedType.RightUpperArm:
                    return "오른쪽 상완";
                case InjuriedType.RightLowerArm:
                    return "오른쪽 팔뚝";
                case InjuriedType.RightHand:
                    return "오른손";

                case InjuriedType.LeftUpperArm:
                    return "왼쪽 상완";
                case InjuriedType.LeftLowerArm:
                    return "왼쪽 팔뚝";
                case InjuriedType.LeftHand:
                    return "왼손";

                case InjuriedType.RightUpperLeg:
                    return "오른쪽 허벅지";
                case InjuriedType.RightLowerLeg:
                    return "오른쪽 종아리";
                case InjuriedType.RightFoot:
                    return "오른발";

                case InjuriedType.LeftUpperLeg:
                    return "왼쪽 허벅지";
                case InjuriedType.LeftLowerLeg:
                    return "왼쪽 종아리";
                case InjuriedType.LeftFoot:
                    return "왼발";

                default:
                    return "없음";
            }
        }
    }
}