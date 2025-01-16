using Cinemachine;
using System;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

namespace _KMH_Framework
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AudioSource))]
    public class FirstPersonControllerEx : MonoBehaviour // FirstPersonControllerEx
    {
        private const string LOG_FORMAT = "<color=white><b>[FirstPersonControllerEx]</b></color> {0}";

        protected CharacterController _characterController;
        protected AudioSource _audioSource;

        [Header("Mouse Look")]
        [SerializeField]
        protected CinemachineVirtualCamera _vCam;
        [SerializeField]
        protected MouseLookEx mouseLook;

        protected Vector3 originCameraPosition;

        [Header("Physical Move Settings")]
        [ReadOnly]
        [SerializeField]
        protected bool isWalking;
        [SerializeField]
        protected float walkSpeed;

        [SerializeField]
        protected float runSpeed;
        [SerializeField]
        [Range(0f, 1f)]
        protected float m_RunstepLenghten;

        [SerializeField]
        protected float moveLerpThreshold;

        [SerializeField]
        protected float m_JumpSpeed;

        [SerializeField]
        protected float m_StickToGroundForce;

        [SerializeField]
        protected float m_GravityMultiplier;

        [SerializeField]
        protected bool m_UseFovKick;
        [SerializeField]
        protected FOVKickEx _fovKick = new FOVKickEx();

        [Space(10)]
        [SerializeField]
        protected bool isUseHeadBob;

        [SerializeField]
        protected CurveControlledBobEx headBobClass = new CurveControlledBobEx();
        [SerializeField]
        protected LerpControlledBob jumpBobClass = new LerpControlledBob();
        [SerializeField]
        protected float m_StepInterval;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected Vector2 _2DInput;
        [SerializeField]
        protected float _2DInputDelta;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected Vector3 _2DMoveDirection = Vector3.zero;

        protected CollisionFlags m_CollisionFlags;

        protected bool isPreviouslyGrounded;

        protected bool isJump;
        protected bool isJumping;

        protected float m_StepCycle;
        protected float m_NextStep;

        [Header("Sounds")]
        [SerializeField]
        protected AudioClip[] footstepAudioClips;    // an array of footstep sounds that will be randomly selected from.
        [SerializeField]
        protected AudioClip jumpAudioClip;           // the sound played when character leaves the ground.
        [SerializeField]
        protected AudioClip landAudioClip;           // the sound played when character touches back on ground.

        protected virtual void Awake()
        {
            _characterController = this.GetComponent<CharacterController>();
            _audioSource = this.GetComponent<AudioSource>();
        }

        protected virtual void Start()
        {
            originCameraPosition = _vCam.transform.localPosition;

            _fovKick.Setup(_vCam);
            headBobClass.Setup(_vCam.transform, m_StepInterval);

            m_StepCycle = 0f;
            m_NextStep = m_StepCycle / 2f;

            isJumping = false;

            mouseLook.Init(this.transform, _vCam.transform);
        }

        protected virtual void Update()
        {
            RotateView();

            // the jump state needs to read here to make sure it is not missed
            if (isJump == false)
            {
                isJump = CrossPlatformInputManager.GetButtonDown("Jump");
            }

            if ((isPreviouslyGrounded == false) && (_characterController.isGrounded == true))
            {
                StartCoroutine(jumpBobClass.DoBobCycle());

                _2DMoveDirection.y = 0f;
                isJumping = false;

                PlayLandingSound();
            }

            if ((_characterController.isGrounded == false) &&
                (isJumping == false) &&
                (isPreviouslyGrounded == true))
            {
                _2DMoveDirection.y = 0f;
            }

            isPreviouslyGrounded = _characterController.isGrounded;
        }

        protected virtual void FixedUpdate()
        {
            GetInput(out float _speed);

            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward * _2DInput.y + transform.right * _2DInput.x;

            // get a normal for the surface that is being touched to move along it
            Physics.SphereCast(this.transform.position, _characterController.radius, Vector3.down, out RaycastHit hitInfo,
                               _characterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            if (_characterController.isGrounded == true)
            {
                _2DMoveDirection.y = -m_StickToGroundForce;

                _2DMoveDirection.x = Mathf.Lerp(_2DMoveDirection.x, desiredMove.x * _speed,moveLerpThreshold);
                _2DMoveDirection.z = Mathf.Lerp(_2DMoveDirection.z, desiredMove.z * _speed,moveLerpThreshold);

                if (isJump == true)
                {
                    _2DMoveDirection.y = m_JumpSpeed;

                    isJump = false;
                    isJumping = true;

                    PlayJumpSound();
                }
            }
            else
            {
                _2DMoveDirection += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
            }
            m_CollisionFlags = _characterController.Move(_2DMoveDirection * Time.fixedDeltaTime);

            ProgressStepCycle(_speed);
            UpdateCameraPosition(_speed);
        }

        protected virtual void GetInput(out float _speed)
        {
            // Read input
            if (KeyType.Move_Right.IsInput() == true)
            {
                _2DInput.y = Mathf.MoveTowards(_2DInput.y, 1f, _2DInputDelta);
            }
            else if (KeyType.Move_Backward.IsInput() == true)
            {
                _2DInput.y = Mathf.MoveTowards(_2DInput.y, -1f, _2DInputDelta);
            }
            else
            {
                _2DInput.y = Mathf.MoveTowards(_2DInput.y, 0f, _2DInputDelta);
            }

            if (KeyType.Move_Left.IsInput() == true)
            {
                _2DInput.x = Mathf.MoveTowards(_2DInput.x, -1f, _2DInputDelta);
            }
            else if (KeyType.Move_Right.IsInput() == true)
            {
                _2DInput.x = Mathf.MoveTowards(_2DInput.x, 1f, _2DInputDelta);
            }
            else
            {
                _2DInput.x = Mathf.MoveTowards(_2DInput.x, 0f, _2DInputDelta);
            }

            bool wasWalking = isWalking;
            isWalking = KeyType.Sprint.IsInput();

            if (isWalking == true)
            {
                _speed = walkSpeed;
            }
            else
            {
                _speed = runSpeed;
            }

            if (_2DInput.sqrMagnitude > 1)
            {
                _2DInput.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (isWalking != wasWalking && m_UseFovKick && _characterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();

                if (isWalking == false)
                {
                    StartCoroutine(_fovKick.FOVKickUp());
                }
                else
                {
                    StartCoroutine(_fovKick.FOVKickDown());
                }
            }
        }

        protected virtual void ProgressStepCycle(float speed)
        {
            if (_characterController.velocity.sqrMagnitude > 0 && (_2DInput.x != 0 || _2DInput.y != 0))
            {
                float speedThreshold;
                if (isWalking == true)
                {
                    speedThreshold = 1f;
                }
                else
                {
                    speedThreshold = m_RunstepLenghten;
                }

                m_StepCycle += (_characterController.velocity.magnitude + (speed * speedThreshold)) * Time.fixedDeltaTime;
            }

            if (m_StepCycle <= m_NextStep)
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();
        }

        protected virtual void UpdateCameraPosition(float speed) // use head bob
        {
            Vector3 newCameraPosition;

            if (isUseHeadBob == false)
            {
                return;
            }

            if ((_characterController.velocity.magnitude > 0.5f) && (_characterController.isGrounded == true))
            {
                float speedThreshold;
                if (isWalking == true)
                {
                    speedThreshold = 1f;
                }
                else
                {
                    speedThreshold = m_RunstepLenghten;
                }

                _vCam.transform.localPosition = headBobClass.DoHeadBob(_characterController.velocity.magnitude + (speed * speedThreshold));

                newCameraPosition = _vCam.transform.localPosition;
                newCameraPosition.y = _vCam.transform.localPosition.y - jumpBobClass.Offset();

                _vCam.transform.localPosition = newCameraPosition;
            }
            else
            {
                newCameraPosition = _vCam.transform.localPosition;
                newCameraPosition.y = originCameraPosition.y - jumpBobClass.Offset();

                _vCam.transform.localPosition = Vector3.Lerp(_vCam.transform.localPosition, newCameraPosition, 0.1f);
            }
        }

        protected virtual void RotateView()
        {
            mouseLook.LookRotation(this.transform, _vCam.transform);
        }

        protected virtual void PlayFootStepAudio()
        {
            if (!_characterController.isGrounded == true)
            {
                return;
            }

            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int _randomized = Random.Range(1, footstepAudioClips.Length);

            _audioSource.clip = footstepAudioClips[_randomized];
            _audioSource.PlayOneShot(_audioSource.clip);

            // move picked sound to index 0 so it's not picked next time
            footstepAudioClips[_randomized] = footstepAudioClips[0];
            footstepAudioClips[0] = _audioSource.clip;
        }

        protected virtual void PlayJumpSound()
        {
            _audioSource.clip = jumpAudioClip;
            _audioSource.Play();
        }

        protected virtual void PlayLandingSound()
        {
            _audioSource.clip = landAudioClip;
            _audioSource.Play();
            m_NextStep = m_StepCycle + 0.5f;
        }

        protected virtual void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(_characterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
        }
    }
}
