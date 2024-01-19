using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPSFramework
{
    public interface IInteractable
    {
        private const string LOG_FORMAT = "<color=white><b>[BaseInteractable]</b></color> {0}";

        public abstract void DoInteract();
    }
}