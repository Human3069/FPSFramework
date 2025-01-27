using UnityEngine;

namespace FPS_Framework.ZuluWar
{
    public class Installable : MonoBehaviour
    {
        [SerializeField]
        protected Transform leftT;
        [SerializeField]
        protected Transform rightT;

        [Space(10)]
        public InstallableDummy DummyPrefab;

        public void OnInstalled()
        {
            float hitDistance = 0f;
            RaycastHit hit;

            if (Physics.Raycast(leftT.position, -leftT.up, out hit, 10f) == true)
            {
                if (hitDistance < hit.distance)
                {
                    hitDistance = hit.distance;
                }
            }

            if (Physics.Raycast(rightT.position, -rightT.up, out hit, 10f) == true)
            {
                if (hitDistance < hit.distance)
                {
                    hitDistance = hit.distance;
                }
            }

            this.transform.position = this.transform.position + Vector3.down * hitDistance;
        }
    }
}