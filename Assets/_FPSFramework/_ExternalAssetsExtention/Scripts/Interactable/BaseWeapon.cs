using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPSFramework
{
    [RequireComponent(typeof(Collider))]
    public class BaseWeapon : BaseEquipable
    {
        private const string LOG_FORMAT = "<color=white><b>[BaseWeapon]</b></color> {0}";

        public override void DoInteract()
        {
            Debug.LogFormat(LOG_FORMAT, "DoInteract()");


        }
    }
}