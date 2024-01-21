using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(Light))]
public class VisualFxPlayer : MonoBehaviour
{
    [SerializeField]
    protected float lightDuration;

    protected ParticleSystem _particleSystem;
    protected Light _light;

    protected void Awake()
    {
        _particleSystem = this.GetComponent<ParticleSystem>();
        _light = this.GetComponent<Light>();

        Debug.Assert(_light.enabled == false);
    }

    public void Play()
    {
        StartCoroutine(PostPlay());
    }

    protected IEnumerator PostPlay()
    {
        _particleSystem.Play();
        _light.enabled = true;
        yield return new WaitForSeconds(lightDuration);
        _light.enabled = false;
    }
}
