
using System;

public enum State
{
    Idle,
    Attack,
    Dead
}

public interface IDamageable
{
    State _State
    {
        get;
        set;
    }

    delegate void StateChanged(State state);
    event StateChanged OnStateChanged;

    float CurrentHealth
    {
        get;
        set;
    }

    void OnDamaged();
    void OnDead();
}
