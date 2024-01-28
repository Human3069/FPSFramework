using _KMH_Framework;
using System.Collections;
using UnityEngine;

namespace FPSFramework
{
    public class ImpactHandler : MonoBehaviour
    {
        [SerializeField]
        protected Impactable.MaterialType _materialType;
        [SerializeField]
        protected float lifeTime = 20;
   
        protected void OnEnable()
        {
            StartCoroutine(PostOnEnable());
        }

        protected IEnumerator PostOnEnable()
        {
            yield return new WaitForSeconds(lifeTime);

            string impactName = Impactable.GetName(_materialType);
            ImpactPoolManager.Instance.PoolHandlerDictionary[impactName].ReturnObject(this.gameObject);
        }
    }
}