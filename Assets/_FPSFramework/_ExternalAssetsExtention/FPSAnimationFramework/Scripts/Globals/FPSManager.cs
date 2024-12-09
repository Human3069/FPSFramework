using UnityEngine;

namespace FPS_Framework
{
    [RequireComponent(typeof(AudioSource))]
    public class FPSManager : MonoBehaviour
    {
        protected static FPSManager _instance;
        public static FPSManager Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

        protected AudioSource audioSource;

        [SerializeField]
        protected float _mouseSenstivity = 1f;
        
        [ReadOnly]
        [SerializeField]
        protected float _aimThreshold = 1f;
        public float AimThreshold
        {
            get
            {
                return _aimThreshold;
            }
            set
            {
                _aimThreshold = value;
            }
        }

        [Space(10)]
        [SerializeField]
        protected bool isAllowHitMarkerSound = true;
        [SerializeField]
        protected AudioClip hitMarkerSoundClip;

        public float ActualSenstivity
        {
            get
            {
                return _mouseSenstivity * AimThreshold;
            }
        }

        protected void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("");
                Destroy(this.gameObject);
                return;
            }

            audioSource = this.GetComponent<AudioSource>();
        }

        protected void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

        public void PlayHitMarkerSoundIfAllowed()
        {
            if (isAllowHitMarkerSound == true)
            {
                audioSource.PlayOneShot(hitMarkerSoundClip);
            }
        }
    }
}