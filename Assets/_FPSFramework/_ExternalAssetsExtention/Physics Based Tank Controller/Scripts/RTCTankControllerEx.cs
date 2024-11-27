#pragma warning disable 0414

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using _KMH_Framework;
using FPS_Framework;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class RTCTankControllerEx : MonoBehaviour
{
    //Rigidbody.
    protected Rigidbody _rigidbody;

    //Enables/Disables controlling the vehicle.
    public bool canControl = true;
    public bool runEngineAtAwake = true;
    public bool engineRunning = false;
    protected bool engineStarting = false;

    //Reversing Bool.
    protected bool reversing = false;
    protected bool autoReverse = false;
    protected bool canGoReverseNow = false;
    protected float reverseDelay = 0f;

    //Wheel Transforms Of The Vehicle.	
    public Transform[] wheelTransform_L;
    public Transform[] wheelTransform_R;

    //Wheel colliders of the vehicle.
    public WheelCollider[] wheelColliders_L;
    public WheelCollider[] wheelColliders_R;

    //All Wheel Colliders.
    protected List<WheelCollider> allWheelColliders = new List<WheelCollider>();

    //Useless Gear Wheels.
    public Transform[] uselessGearTransform_L;
    public Transform[] uselessGearTransform_R;

    //Track Bones.
    public Transform[] trackBoneTransform_L;
    public Transform[] trackBoneTransform_R;

    //Track Customization.
    public GameObject leftTrackMesh;
    public GameObject rightTrackMesh;
    public float trackOffset = 0.025f;
    public float trackScrollSpeedMultiplier = 1f;

    //Wheels Rotation.
    protected float[] rotationValueL;
    protected float[] rotationValueR;

    //Center Of Mass.
    [SerializeField]
    protected Transform _centerOfMassTransform;
    public Transform CenterOfMessTransform
    {
        get
        {
            return _centerOfMassTransform;
        }
        set
        {
            _centerOfMassTransform = value;
        }
    }

    //Mechanic.
    public AnimationCurve engineTorqueCurve;
    public float engineTorque = 3500f;
    public float brakeTorque = 5000f;
    public float minEngineRPM = 1000f;
    public float maxEngineRPM = 5000f;
    public float maxSpeed = 60f;
    public float steerTorque = 5f;
    protected float speed;
    protected float defSteerAngle;
    protected float acceleration = 0f;
    protected float lastVelocity = 0f;
    protected float engineRPM = 0f;

    [SerializeField]
    protected RTCTankGunControllerEx tankGunController;

    [Header("Inputs")]
    [ReadOnly]
    [SerializeField]
    protected float gasInput = 0f;
    [ReadOnly]
    [SerializeField]
    protected float brakeInput = 0f;
    [ReadOnly]
    [SerializeField]
    protected float steerInput = 0f;

    [Space(10)]
    [ReadOnly]
    [SerializeField]
    protected float fuelInput = 1f;

    //Sound Effects.
    protected AudioSource engineStartUpAudio;
    protected AudioSource engineIdleAudio;
    protected AudioSource engineRunningAudio;
    protected AudioSource brakeAudio;

    public AudioClip engineStartUpAudioClip;
    public AudioClip engineIdleAudioClip;
    public AudioClip engineRunningAudioClip;
    public AudioClip brakeClip;

    //Sound Limits.
    public float minEngineSoundPitch = 0.5f;
    public float maxEngineSoundPitch = 1.15f;
    public float minEngineSoundVolume = 0.05f;
    public float maxEngineSoundVolume = 0.85f;
    public float maxBrakeSoundVolume = 0.35f;

    //Smokes.
    public GameObject wheelSlip;
    protected List<ParticleSystem> wheelParticles = new List<ParticleSystem>();

    public ParticleSystem exhaustSmoke;

    protected virtual void Awake()
    {
        Debug.Assert(tankGunController != null);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    protected virtual void OnEnable()
    {
        tankGunController.enabled = true;
    }

    protected virtual void OnDisable()
    {
        tankGunController.enabled = false;
    }

    protected virtual void Start()
    {
        Initialize_WheelColliders();
        Initialize_Sounds();
        if (wheelSlip == true)
        {
            Initialize_Smoke();
        }

        _rigidbody = this.GetComponent<Rigidbody>();
        _rigidbody.maxAngularVelocity = 5f;
        _rigidbody.centerOfMass = new Vector3((CenterOfMessTransform.localPosition.x) * this.transform.localScale.x,
                                              (CenterOfMessTransform.localPosition.y) * this.transform.localScale.y,
                                              (CenterOfMessTransform.localPosition.z) * this.transform.localScale.z);

        rotationValueL = new float[wheelColliders_L.Length];
        rotationValueR = new float[wheelColliders_R.Length];

        if (runEngineAtAwake == true)
        {
            KillOrStartEngine();
        }
    }

    public virtual void CreateWheelColliders()
    {
        List<Transform> allWheelTransformsL = new List<Transform>();
        List<Transform> allWheelTransformsR = new List<Transform>();

        foreach (Transform wheel in wheelTransform_L)
        {
            allWheelTransformsL.Add(wheel);
        }
        foreach (Transform wheel in wheelTransform_R)
        {
            allWheelTransformsR.Add(wheel);
        }

        if (allWheelTransformsR[0] == null ||
            allWheelTransformsL[0] == null)
        {
            // Debug.LogErrorFormat(_Log._Format(this), "You haven't choose your Wheel Transforms. Please select all of your Wheel Transforms before creating Wheel Colliders. Script needs to know their positions, aye?");
            return;
        }

        this.transform.rotation = Quaternion.identity;

        GameObject _WheelColliders_LObj = new GameObject("WheelColliders_L");
        _WheelColliders_LObj.transform.parent = this.transform;
        _WheelColliders_LObj.transform.rotation = this.transform.rotation;
        _WheelColliders_LObj.transform.localPosition = Vector3.zero;
        _WheelColliders_LObj.transform.localScale = Vector3.one;

        GameObject _WheelColliders_RObj = new GameObject("WheelColliders_R");
        _WheelColliders_RObj.transform.parent = this.transform;
        _WheelColliders_RObj.transform.rotation = this.transform.rotation;
        _WheelColliders_RObj.transform.localPosition = Vector3.zero;
        _WheelColliders_RObj.transform.localScale = Vector3.one;

        foreach (Transform wheelTransform in allWheelTransformsL)
        {
            GameObject wheelcolliderLObj = new GameObject(wheelTransform.name);

            wheelcolliderLObj.transform.position = wheelTransform.position;
            wheelcolliderLObj.transform.rotation = this.transform.rotation;
            wheelcolliderLObj.transform.name = wheelTransform.name;
            wheelcolliderLObj.transform.parent = _WheelColliders_LObj.transform;
            wheelcolliderLObj.transform.localScale = Vector3.one;

            WheelCollider _wheelCollider = wheelcolliderLObj.AddComponent<WheelCollider>();
            _wheelCollider.radius = (wheelTransform.GetComponent<MeshRenderer>().bounds.size.y / 2f) / transform.localScale.y;

            JointSpring spring = _wheelCollider.suspensionSpring;

            spring.spring = 50000f;
            spring.damper = 5000f;
            spring.targetPosition = 0.5f;

            _wheelCollider.mass = 200f;
            _wheelCollider.wheelDampingRate = 10f;
            _wheelCollider.suspensionDistance = 0.3f;
            _wheelCollider.forceAppPointDistance = 0.25f;
            _wheelCollider.suspensionSpring = spring;

            Vector3 suspensionDistancedPos = new Vector3(0f, (_wheelCollider.suspensionDistance / 2f), 0f);
            wheelcolliderLObj.transform.localPosition += suspensionDistancedPos;

            WheelFrictionCurve sidewaysFriction = _wheelCollider.sidewaysFriction;
            WheelFrictionCurve forwardFriction = _wheelCollider.forwardFriction;

            forwardFriction.extremumSlip = 0.4f;
            forwardFriction.extremumValue = 1f;
            forwardFriction.asymptoteSlip = 0.8f;
            forwardFriction.asymptoteValue = 0.75f;
            forwardFriction.stiffness = 1.75f;

            sidewaysFriction.extremumSlip = 0.25f;
            sidewaysFriction.extremumValue = 1f;
            sidewaysFriction.asymptoteSlip = 0.5f;
            sidewaysFriction.asymptoteValue = 0.75f;
            sidewaysFriction.stiffness = 2f;

            _wheelCollider.sidewaysFriction = sidewaysFriction;
            _wheelCollider.forwardFriction = forwardFriction;
        }

        foreach (Transform wheelTransform in allWheelTransformsR)
        {
            GameObject wheelcolliderRObj = new GameObject(wheelTransform.name);

            wheelcolliderRObj.transform.position = wheelTransform.position;
            wheelcolliderRObj.transform.rotation = this.transform.rotation;
            wheelcolliderRObj.transform.name = wheelTransform.name;
            wheelcolliderRObj.transform.parent = _WheelColliders_RObj.transform;
            wheelcolliderRObj.transform.localScale = Vector3.one;
            WheelCollider _wheelCollider = wheelcolliderRObj.AddComponent<WheelCollider>();
            _wheelCollider.radius = (wheelTransform.GetComponent<MeshRenderer>().bounds.size.y / 2f) / transform.localScale.y;

            JointSpring spring = _wheelCollider.suspensionSpring;

            spring.spring = 50000f;
            spring.damper = 5000f;
            spring.targetPosition = 0.5f;

            _wheelCollider.mass = 200f;
            _wheelCollider.wheelDampingRate = 10f;
            _wheelCollider.suspensionDistance = 0.3f;
            _wheelCollider.forceAppPointDistance = 0.25f;
            _wheelCollider.suspensionSpring = spring;

            Vector3 suspensionDistancedPos = new Vector3(0f, (_wheelCollider.suspensionDistance / 2f), 0f);
            wheelcolliderRObj.transform.localPosition += suspensionDistancedPos;

            WheelFrictionCurve sidewaysFriction = _wheelCollider.sidewaysFriction;
            WheelFrictionCurve forwardFriction = _wheelCollider.forwardFriction;

            forwardFriction.extremumSlip = 0.4f;
            forwardFriction.extremumValue = 1f;
            forwardFriction.asymptoteSlip = 0.8f;
            forwardFriction.asymptoteValue = 0.75f;
            forwardFriction.stiffness = 1.75f;

            sidewaysFriction.extremumSlip = 0.25f;
            sidewaysFriction.extremumValue = 1f;
            sidewaysFriction.asymptoteSlip = 0.5f;
            sidewaysFriction.asymptoteValue = 0.75f;
            sidewaysFriction.stiffness = 2f;

            _wheelCollider.sidewaysFriction = sidewaysFriction;
            _wheelCollider.forwardFriction = forwardFriction;
        }

        wheelColliders_L = _WheelColliders_LObj.GetComponentsInChildren<WheelCollider>();
        wheelColliders_R = _WheelColliders_RObj.GetComponentsInChildren<WheelCollider>();
    }

    protected virtual void Initialize_WheelColliders()
    {
        WheelCollider[] _wheelColliders = GetComponentsInChildren<WheelCollider>();
        for (int i = 0; i < _wheelColliders.Length; i++)
        {
            allWheelColliders.Add(_wheelColliders[i]);
        }
    }

    protected virtual void Initialize_Sounds()
    {
        engineIdleAudio = RTCCreateAudioSource.NewAudioSource(gameObject, "engineIdleAudio", 5f, .5f, engineIdleAudioClip, true, true, false);
        engineRunningAudio = RTCCreateAudioSource.NewAudioSource(gameObject, "engineRunningAudio", 5f, 0f, engineRunningAudioClip, true, true, false);
        brakeAudio = RTCCreateAudioSource.NewAudioSource(gameObject, "Brake Sound AudioSource", 5, 0, brakeClip, true, true, false);
    }

    public virtual void KillOrStartEngine()
    {
        if (engineRunning == true &&
            engineStarting == false)
        {
            engineRunning = false;
        }
        else if (engineStarting == false)
        {
            StartCoroutine(PostStartEngine());
        }
        else
        {
            // do nothing
        }
    }

    protected virtual IEnumerator PostStartEngine()
    {
        engineRunning = false;
        engineStarting = true;
        if (engineRunning == false)
        {
            engineStartUpAudio = RTCCreateAudioSource.NewAudioSource(gameObject, "Engine Start AudioSource", 5, 1, engineStartUpAudioClip, false, true, true);
        }

        yield return new WaitForSeconds(1f);
        engineRunning = true;
        yield return new WaitForSeconds(1f);
        engineStarting = false;
    }

    protected virtual void Initialize_Smoke()
    {
        for (int i = 0; i < allWheelColliders.Count; i++)
        {
            GameObject wheelSlipParticleObj = (GameObject)Instantiate(wheelSlip, allWheelColliders[i].transform.position, transform.rotation) as GameObject;
            wheelParticles.Add(wheelSlipParticleObj.GetComponent<ParticleSystem>());
        }

        for (int i = 0; i < allWheelColliders.Count; i++)
        {
            wheelParticles[i].transform.position = allWheelColliders[i].transform.position;
            wheelParticles[i].transform.parent = allWheelColliders[i].transform;
        }
    }

    protected virtual void Update()
    {
        WheelAlign();
        Sounds();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Time.timeScale = 0.02f;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Time.timeScale = 1f;
        }
    }

    protected virtual void FixedUpdate()
    {
        AnimateGears();
        Engine();
        Braking();
        Inputs();
        Smoke();
    }

    protected virtual void Engine()
    {
        //Reversing Bool.
        if (gasInput < 0 &&
            this.transform.InverseTransformDirection(_rigidbody.velocity).z < 1 &&
            canGoReverseNow == true)
        {
            reversing = true;
        }
        else
        {
            reversing = false;
        }

        speed = _rigidbody.velocity.magnitude * 3.0f;

        //Acceleration Calculation.
        acceleration = 0f;
        acceleration = (transform.InverseTransformDirection(_rigidbody.velocity).z - lastVelocity) / Time.fixedDeltaTime;
        lastVelocity = transform.InverseTransformDirection(_rigidbody.velocity).z;

        //Drag Limit.
        _rigidbody.drag = Mathf.Clamp((acceleration / 30f), 0f, 1f);

        float rpm;
        float wheelRPM = ((Mathf.Abs((wheelColliders_L[2].rpm * wheelColliders_L[2].radius) + (wheelColliders_R[2].rpm * wheelColliders_R[2].radius)) / 2f) / 3.25f);

        float lerpedValue = (Mathf.Lerp(minEngineRPM, maxEngineRPM, wheelRPM / maxSpeed) + minEngineRPM);
        if (reversing == false)
        {
            rpm = Mathf.Clamp(lerpedValue * (Mathf.Clamp01(gasInput + Mathf.Abs(steerInput))), minEngineRPM, maxEngineRPM);
        }
        else
        {
            rpm = Mathf.Clamp(lerpedValue * (Mathf.Clamp01(brakeInput + Mathf.Abs(steerInput))), minEngineRPM, maxEngineRPM);
        }

        engineRPM = Mathf.Lerp(engineRPM, (rpm + UnityEngine.Random.Range(-50f, 50f)) * fuelInput, Time.deltaTime * 2f);

        if (engineRunning == false)
        {
            fuelInput = 0;
        }
        else
        {
            fuelInput = 1;
        }

        //Auto Reverse Bool.
        if (autoReverse == true)
        {
            canGoReverseNow = true;
        }
        else
        {
            if (brakeInput > 0.1f &&
                speed < 5)
            {
                reverseDelay += Time.deltaTime;
            }
            else if (brakeInput > 0 &&
                     this.transform.InverseTransformDirection(_rigidbody.velocity).z > 1f)
            {
                reverseDelay = 0f;
            }
        }

        canGoReverseNow = (reverseDelay >= 0.75f);

        for (int i = 0; i < wheelColliders_L.Length; i++)
        {
            ApplyMotorTorque(wheelColliders_L[i], engineTorque, true);
            wheelColliders_L[i].wheelDampingRate = Mathf.Lerp(100f, 0f, gasInput);
        }

        for (int i = 0; i < wheelColliders_R.Length; i++)
        {
            ApplyMotorTorque(wheelColliders_R[i], engineTorque, false);
            wheelColliders_R[i].wheelDampingRate = Mathf.Lerp(100f, 0f, gasInput);
        }

        //Steering.
        if (reversing == false)
        {
            if (wheelColliders_L[2].isGrounded == true ||
                wheelColliders_R[2].isGrounded == true)
            {
                if (Mathf.Abs(_rigidbody.angularVelocity.y) < 1f)
                {
                    _rigidbody.AddRelativeTorque((Vector3.up * steerInput) * steerTorque, ForceMode.Acceleration);
                }
            }
        }
        else
        {
            if (wheelColliders_L[2].isGrounded == true||
                wheelColliders_R[2].isGrounded == true)
            {
                if (Mathf.Abs(_rigidbody.angularVelocity.y) < 1f)
                {
                    _rigidbody.AddRelativeTorque((-Vector3.up * steerInput) * steerTorque, ForceMode.Acceleration);
                }
            }
        }
    }

    public virtual void ApplyMotorTorque(WheelCollider _wheelCollider, float torque, bool leftSide)
    {
        WheelHit _wheelHit;
        _wheelCollider.GetGroundHit(out _wheelHit);

        if (Mathf.Abs(_wheelHit.forwardSlip) > 1f)
        {
            torque = 0;
        }

        if (speed > maxSpeed ||
            Mathf.Abs(_wheelCollider.rpm) > 1000 ||
            Mathf.Abs(_wheelCollider.rpm) > 1000 ||
            engineRunning == false)
        {
            torque = 0;
        }

        if (reversing == true &&
            speed > 55)
        {
            torque = 0;
        }

        if (engineRunning == false ||
            _wheelCollider.isGrounded == false)
        {
            torque = 0;
        }

        if (reversing == false)
        {
            if (leftSide == true)
            {
                _wheelCollider.motorTorque = torque * Mathf.Clamp((Mathf.Clamp(gasInput * fuelInput, 0f, 1f)) + Mathf.Clamp(steerInput, -1f, 1f), -1f, 1f) * engineTorqueCurve.Evaluate(speed);
            }
            else
            {
                _wheelCollider.motorTorque = torque * Mathf.Clamp((Mathf.Clamp(gasInput * fuelInput, 0f, 1f)) + Mathf.Clamp(-steerInput, -1f, 1f), -1f, 1f) * engineTorqueCurve.Evaluate(speed);
            }
        }
        else
        {
            _wheelCollider.motorTorque = (-torque) * brakeInput;
        }
    }

    public virtual void ApplyBrakeTorque(WheelCollider _wheelCollider, float brake)
    {
        _wheelCollider.brakeTorque = brake;
    }

    public virtual void Braking()
    {
        for (int i = 0; i < allWheelColliders.Count; i++)
        {
            if (brakeInput > 0.1f &&
                reversing == false)
            {
                ApplyBrakeTorque(allWheelColliders[i], brakeTorque * (brakeInput));
            }
            else if (brakeInput > 0.1f &&
                     reversing == true)
            {
                ApplyBrakeTorque(allWheelColliders[i], 0f);
            }
            else if (gasInput < 0.1f &&
                     Mathf.Abs(steerInput) < 0.1f)
            {
                ApplyBrakeTorque(allWheelColliders[i], 10f);
            }
            else
            {
                ApplyBrakeTorque(allWheelColliders[i], 0f);
            }
        }
    }

    protected virtual void Inputs()
    {
        if (canControl == false)
        {
            gasInput = 0;
            brakeInput = 0;
            steerInput = 0;

            return;
        }

        //Motor Input.
        gasInput = Input.GetAxis("Vertical");

        //Brake Input
        brakeInput = -Mathf.Clamp(Input.GetAxis("Vertical"), -1f, 0f);

        //Steering Input.
        steerInput = Input.GetAxis("Horizontal");
    }

    public virtual void Sounds()
    {
        //Engine Audio Volume.
        if (engineRunningAudioClip == true)
        {
            if (reversing == false)
            {
                engineRunningAudio.volume = Mathf.Lerp(engineRunningAudio.volume, Mathf.Clamp(Mathf.Clamp01(gasInput + Mathf.Abs(steerInput / 2f)), minEngineSoundVolume, maxEngineSoundVolume), Time.deltaTime * 10f);
            }
            else
            {
                engineRunningAudio.volume = Mathf.Lerp(engineRunningAudio.volume, Mathf.Clamp(brakeInput, minEngineSoundVolume, maxEngineSoundVolume), Time.deltaTime * 10f);
            }

            if (engineRunning == true)
            {
                engineRunningAudio.pitch = Mathf.Lerp(engineRunningAudio.pitch, Mathf.Lerp(minEngineSoundPitch, maxEngineSoundPitch, (engineRPM) / (maxEngineRPM)), Time.deltaTime * 10f);
            }
            else
            {
                engineRunningAudio.pitch = Mathf.Lerp(engineRunningAudio.pitch, 0, Time.deltaTime * 10f);
            }
        }

        if (engineIdleAudioClip == true)
        {
            if (reversing == false)
            {
                engineIdleAudio.volume = Mathf.Lerp(engineIdleAudio.volume, Mathf.Clamp((1 + (-gasInput)), minEngineSoundVolume, 1f), Time.deltaTime * 10f);
            }
            else
            {
                engineIdleAudio.volume = Mathf.Lerp(engineIdleAudio.volume, Mathf.Clamp((1 + (-brakeInput)), minEngineSoundVolume, 1f), Time.deltaTime * 10f);
            }

            if (engineRunning == true)
            {
                engineIdleAudio.pitch = Mathf.Lerp(engineIdleAudio.pitch, Mathf.Lerp(minEngineSoundPitch, maxEngineSoundPitch, (engineRPM) / (maxEngineRPM)), Time.deltaTime * 10f);
            }
            else
            {
                engineIdleAudio.pitch = Mathf.Lerp(engineIdleAudio.pitch, 0, Time.deltaTime * 10f);
            }
        }

        if (reversing == false)
        {
            brakeAudio.volume = Mathf.Lerp(0f, maxBrakeSoundVolume, Mathf.Clamp01(brakeInput) * Mathf.Lerp(0f, 1f, wheelColliders_L[2].rpm / 50f));
        }
        else
        {
            brakeAudio.volume = 0f;
        }
    }

    protected virtual void AnimateGears()
    {
        for (int i = 0; i < uselessGearTransform_R.Length; i++)
        {
            uselessGearTransform_R[i].transform.rotation = wheelColliders_R[i].transform.rotation * Quaternion.Euler(rotationValueR[Mathf.CeilToInt((wheelColliders_R.Length) / 2)], wheelColliders_R[i].steerAngle, 0);
        }

        for (int i = 0; i < uselessGearTransform_L.Length; i++)
        {
            uselessGearTransform_L[i].transform.rotation = wheelColliders_L[i].transform.rotation * Quaternion.Euler(rotationValueL[Mathf.CeilToInt((wheelColliders_L.Length) / 2)], wheelColliders_L[i].steerAngle, 0);
        }
    }

    protected virtual void WheelAlign()
    {
        RaycastHit hit;

        //Right Wheels Transform.
        for (int k = 0; k < wheelColliders_R.Length; k++)
        {
            Vector3 ColliderCenterPoint = wheelColliders_R[k].transform.TransformPoint(wheelColliders_R[k].center);

            if (Physics.Raycast(ColliderCenterPoint, -wheelColliders_R[k].transform.up, out hit, (wheelColliders_R[k].suspensionDistance + wheelColliders_R[k].radius) * transform.localScale.y) && hit.transform.root != transform)
            {
                wheelTransform_R[k].transform.position = hit.point + (wheelColliders_R[k].transform.up * wheelColliders_R[k].radius) * transform.localScale.y;
                if (trackBoneTransform_R.Length > 0)
                {
                    trackBoneTransform_R[k].transform.position = hit.point + (wheelColliders_R[k].transform.up * trackOffset) * transform.localScale.y;
                }
            }
            else
            {
                wheelTransform_R[k].transform.position = ColliderCenterPoint - (wheelColliders_R[k].transform.up * wheelColliders_R[k].suspensionDistance) * transform.localScale.y;
                if (trackBoneTransform_R.Length > 0)
                {
                    trackBoneTransform_R[k].transform.position = ColliderCenterPoint - (wheelColliders_R[k].transform.up * (wheelColliders_R[k].suspensionDistance + wheelColliders_R[k].radius - trackOffset)) * transform.localScale.y;
                }
            }

            wheelTransform_R[k].transform.rotation = wheelColliders_R[k].transform.rotation * Quaternion.Euler(rotationValueR[Mathf.CeilToInt((wheelColliders_R.Length) / 2)], 0, 0);
            rotationValueR[k] += wheelColliders_R[k].rpm * (6) * Time.deltaTime;
        }

        //Left Wheels Transform.
        for (int i = 0; i < wheelColliders_L.Length; i++)
        {
            Vector3 ColliderCenterPoint = wheelColliders_L[i].transform.TransformPoint(wheelColliders_L[i].center);

            if (Physics.Raycast(ColliderCenterPoint, -wheelColliders_L[i].transform.up, out hit, (wheelColliders_L[i].suspensionDistance + wheelColliders_L[i].radius) * transform.localScale.y) && hit.transform.root != transform)
            {
                wheelTransform_L[i].transform.position = hit.point + (wheelColliders_L[i].transform.up * wheelColliders_L[i].radius) * transform.localScale.y;
                if (trackBoneTransform_L.Length > 0)
                {
                    trackBoneTransform_L[i].transform.position = hit.point + (wheelColliders_L[i].transform.up * trackOffset) * transform.localScale.y;
                }
            }
            else
            {
                wheelTransform_L[i].transform.position = ColliderCenterPoint - (wheelColliders_L[i].transform.up * wheelColliders_L[i].suspensionDistance) * transform.localScale.y;
                if (trackBoneTransform_L.Length > 0)
                {
                    trackBoneTransform_L[i].transform.position = ColliderCenterPoint - (wheelColliders_L[i].transform.up * (wheelColliders_L[i].suspensionDistance + wheelColliders_L[i].radius - trackOffset)) * transform.localScale.y;
                }
            }

            wheelTransform_L[i].transform.rotation = wheelColliders_L[i].transform.rotation * Quaternion.Euler(rotationValueL[Mathf.CeilToInt((wheelColliders_L.Length) / 2)], 0, 0);
            rotationValueL[i] += wheelColliders_L[i].rpm * (6) * Time.deltaTime;
        }

        //Scrolling Track Texture Offset.
        if (leftTrackMesh == true &&
            rightTrackMesh == true)
        {
            leftTrackMesh.GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2((rotationValueL[Mathf.CeilToInt((wheelColliders_L.Length) / 2)] / 1000) * trackScrollSpeedMultiplier, 0));
            rightTrackMesh.GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2((rotationValueR[Mathf.CeilToInt((wheelColliders_R.Length) / 2)] / 1000) * trackScrollSpeedMultiplier, 0));
            leftTrackMesh.GetComponent<Renderer>().material.SetTextureOffset("_BumpMap", new Vector2((rotationValueL[Mathf.CeilToInt((wheelColliders_L.Length) / 2)] / 1000) * trackScrollSpeedMultiplier, 0));
            rightTrackMesh.GetComponent<Renderer>().material.SetTextureOffset("_BumpMap", new Vector2((rotationValueR[Mathf.CeilToInt((wheelColliders_R.Length) / 2)] / 1000) * trackScrollSpeedMultiplier, 0));
        }
    }

    protected virtual void Smoke()
    {
        if (wheelParticles.Count > 0)
        {
            for (int i = 0; i < allWheelColliders.Count; i++)
            {
                WheelHit CorrespondingGroundHit;
                allWheelColliders[i].GetGroundHit(out CorrespondingGroundHit);

                if (speed > 1f && allWheelColliders[i].isGrounded)
                {
                    wheelParticles[i].enableEmission = true;
                    wheelParticles[i].emissionRate = Mathf.Lerp(0f, 10f, speed / 25f);
                }
                else
                {
                    wheelParticles[i].enableEmission = false;
                }
            }
        }

        if (exhaustSmoke == true)
        {
            exhaustSmoke.emissionRate = Mathf.Lerp(0f, 50f, engineRPM / maxEngineRPM);
            exhaustSmoke.startSpeed = Mathf.Lerp(0f, 10f, engineRPM / maxEngineRPM);
            exhaustSmoke.startSize = Mathf.Lerp(.1f, 1f, engineRPM / maxEngineRPM);
        }
    }
}