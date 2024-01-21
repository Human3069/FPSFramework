using _KMH_Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class BulletHandler : MonoBehaviour
{
    [SerializeField]
    protected float lifeTime;
    [SerializeField]
    protected float speed;

    protected Rigidbody _rigidbody;

    protected void Awake()
    {
        _rigidbody = this.GetComponent<Rigidbody>();
    }

    protected void OnEnable()
    {
        _rigidbody.velocity = this.transform.forward * speed;
        _rigidbody.angularVelocity = Vector3.zero;
        StartCoroutine(PostOnEnable());
    }

    protected IEnumerator PostOnEnable()
    {
        yield return new WaitForSeconds(lifeTime);

        BulletPoolManager.Instance.PoolHandlerDictionary[BulletPoolManager.COMMON_BULLET].ReturnObject(this.gameObject);
    }
}
