using _KMH_Framework;
using FPS_Framework.Pool;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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