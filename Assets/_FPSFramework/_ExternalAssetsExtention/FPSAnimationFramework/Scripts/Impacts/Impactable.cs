using _KMH_Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FPS_Framework
{
    [RequireComponent(typeof(Collider))]
    public class Impactable : MonoBehaviour
    {
        public enum MaterialType
        {
            Plaster,
            Metal,
            Foliage,
            Rock,
            Wood,
            Brick,
            Concrete,
            Dirt,
            Glass,
            Water,
            Flesh,

            ShootingTarget,

            Explosion_105mm
        }

        [SerializeField]
        protected MaterialType _materialType;
        public MaterialType _MaterialType
        {
            get
            {
                return _materialType;
            }
        }

        public float Thickness = 1f;

        [Space(10)]
        public WarriorController Warrior;
        public float DamageMultiplier = 1f;

        public static string GetName(MaterialType _type)
        {
            string impactName;
            if (_type == MaterialType.Brick)
            {
                impactName = ImpactPoolManager.BRICK_IMPACT;
            }
            else if (_type == MaterialType.Concrete)
            {
                impactName = ImpactPoolManager.CONCRETE_IMPACT;
            }
            else if (_type == MaterialType.Dirt)
            {
                impactName = ImpactPoolManager.DIRT_IMPACT;
            }
            else if (_type == MaterialType.Foliage)
            {
                impactName = ImpactPoolManager.FOLIAGE_IMPACT;
            }
            else if (_type == MaterialType.Glass)
            {
                impactName = ImpactPoolManager.GLASS_IMPACT;
            }
            else if (_type == MaterialType.Metal)
            {
                impactName = ImpactPoolManager.METAL_IMPACT;
            }
            else if (_type == MaterialType.Plaster)
            {
                impactName = ImpactPoolManager.PLASTER_IMPACT;
            }
            else if (_type == MaterialType.Rock)
            {
                impactName = ImpactPoolManager.ROCK_IMPACT;
            }
            else if (_type == MaterialType.Water)
            {
                impactName = ImpactPoolManager.WATER_IMPACT;
            }
            else if (_type == MaterialType.Wood)
            {
                impactName = ImpactPoolManager.WOOD_IMPACT;
            }
            else if (_type == MaterialType.Flesh)
            {
                impactName = ImpactPoolManager.FLESH_IMPACT;
            }
            else if (_type == MaterialType.ShootingTarget)
            {
                impactName = ImpactPoolManager.PLASTER_IMPACT;
            }
            else if (_type == MaterialType.Explosion_105mm)
            {
                impactName = ImpactPoolManager.EXPLOSION_105_CANNON;
            }
            else
            {
                impactName = "";
                Debug.Assert(false);
            }

            return impactName;
        }

        protected ShootingTarget _shootingTarget;
        public ShootingTarget _ShootingTarget
        {
            get
            {
                return _shootingTarget;
            }
            protected set
            {
                _shootingTarget = value;
            }
        }

        protected void Awake()
        {
            if (_MaterialType == MaterialType.ShootingTarget)
            {
                _ShootingTarget = this.transform.GetComponentInChildren<ShootingTarget>();
            }
        }
    }
}