using UnityEngine;
using UnityEngine.UI;

namespace FPS_Framework.ZuluWar
{
    public class UI_FixedProgress : MonoBehaviour
    {
        [SerializeField]
        protected Image[] progressImages;
        [SerializeField]
        protected Color onColor;
        [SerializeField]
        protected Color offColor;

        public int Max
        {
            get
            {
                return progressImages.Length;
            }
        }

        protected void Awake()
        {
            Debug.Assert(progressImages.Length > 0);

            foreach (Image image in progressImages)
            {
                image.color = offColor;
            }
        }

        public void Set(int progress)
        {
            Debug.Assert(progress >= 0 && progress <= progressImages.Length);

            for (int i = 0; i < progressImages.Length; i++)
            {
                progressImages[i].color = i < progress ? onColor : offColor;
            }
        }
    }
}