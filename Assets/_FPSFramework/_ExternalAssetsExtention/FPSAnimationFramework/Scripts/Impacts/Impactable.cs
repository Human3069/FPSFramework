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
        public WarriorController Warrior;
        public float DamageMultiplier = 1f;
    }
}