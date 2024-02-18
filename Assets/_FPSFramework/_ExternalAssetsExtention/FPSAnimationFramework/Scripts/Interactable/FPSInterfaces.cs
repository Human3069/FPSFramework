using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPS_Framework
{
    public interface IInteractable
    {
        //
    }

    ////////////////////////////////////////////////////

    public interface IEquipable : IInteractable
    {
        //
    }

    public interface IWeapon : IEquipable
    {
        //
    }

    ////////////////////////////////////////////////////

    public interface ISeatable : IInteractable
    {
        //
    }
}