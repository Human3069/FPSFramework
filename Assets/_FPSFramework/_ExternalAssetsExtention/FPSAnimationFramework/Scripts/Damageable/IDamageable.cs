using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State
{
    Idle,
    Attack,
    Dead
}

public interface IDamageable
{
    float CurrentHealth
    {
        get;
        set;
    }

    void OnDamaged();
    void OnDead();
}
