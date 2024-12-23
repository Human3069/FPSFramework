﻿#pragma warning disable 0414

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (Rigidbody))]

public class RTCTankController : MonoBehaviour {

	//Rigidbody.
	protected Rigidbody rigid;

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
	protected List <WheelCollider> allWheelColliders = new List<WheelCollider>();
		
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
	public Transform COM;

	//Mechanic.
	public AnimationCurve engineTorqueCurve;
	public float engineTorque = 3500.0f;
	public float brakeTorque = 5000.0f; 
	public float minEngineRPM = 1000.0f;
	public float maxEngineRPM = 5000.0f;
	public float maxSpeed = 60.0f;
	public float steerTorque = 5f;
	protected float speed;
	protected float defSteerAngle;
	protected float acceleration = 0f;
	protected float lastVelocity = 0f;
	protected float engineRPM = 0.0f;
	
	//Inputs.
	public float gasInput = 0f;
	public float brakeInput = 0f;
	public float steerInput = 0f;

	public float fuelInput = 1f;

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
	public float minEngineSoundPitch = .5f;
	public float maxEngineSoundPitch = 1.15f;
	public float minEngineSoundVolume = .05f;
	public float maxEngineSoundVolume = .85f;
	public float maxBrakeSoundVolume = .35f;

	//Smokes.
	public GameObject wheelSlip;
	protected List <ParticleSystem> wheelParticles = new List<ParticleSystem>();

	public ParticleSystem exhaustSmoke;
		
	protected virtual void Start ()
	{
		WheelCollidersInit();
		SoundsInit();
		if(wheelSlip)
			SmokeInit();

		rigid = GetComponent<Rigidbody>();

		rigid.maxAngularVelocity = 5f;
		rigid.centerOfMass = new Vector3((COM.localPosition.x) * transform.localScale.x , (COM.localPosition.y) * transform.localScale.y , (COM.localPosition.z) * transform.localScale.z);

		rotationValueL = new float[wheelColliders_L.Length];
		rotationValueR = new float[wheelColliders_R.Length];

		if(runEngineAtAwake)
			KillOrStartEngine();
	}

	public virtual void CreateWheelColliders ()
	{
		List <Transform> allWheelTransformsL = new List<Transform>();
		List <Transform> allWheelTransformsR = new List<Transform>();

		foreach(Transform wheel in wheelTransform_L){
			allWheelTransformsL.Add(wheel);
		}
		foreach(Transform wheel in wheelTransform_R){
			allWheelTransformsR.Add(wheel);
		}
		
		if(allWheelTransformsR[0] == null || allWheelTransformsL[0] == null){
			Debug.LogError("You haven't choose your Wheel Transforms. Please select all of your Wheel Transforms before creating Wheel Colliders. Script needs to know their positions, aye?");
			return;
		}
		
		transform.rotation = Quaternion.identity;
		
		GameObject _WheelColliders_L = new GameObject("WheelColliders_L");
		_WheelColliders_L.transform.parent = transform;
		_WheelColliders_L.transform.rotation = transform.rotation;
		_WheelColliders_L.transform.localPosition = Vector3.zero;
		_WheelColliders_L.transform.localScale = Vector3.one;

		GameObject _WheelColliders_R = new GameObject("WheelColliders_R");
		_WheelColliders_R.transform.parent = transform;
		_WheelColliders_R.transform.rotation = transform.rotation;
		_WheelColliders_R.transform.localPosition = Vector3.zero;
		_WheelColliders_R.transform.localScale = Vector3.one;

		#region Wheel Collider Properties
		foreach(Transform wheel in allWheelTransformsL){
			
			GameObject wheelcolliderL = new GameObject(wheel.transform.name); 
			
			wheelcolliderL.transform.position = wheel.transform.position;
			wheelcolliderL.transform.rotation = transform.rotation;
			wheelcolliderL.transform.name = wheel.transform.name;
			wheelcolliderL.transform.parent = _WheelColliders_L.transform;
			wheelcolliderL.transform.localScale = Vector3.one;
			wheelcolliderL.AddComponent<WheelCollider>();
			wheelcolliderL.GetComponent<WheelCollider>().radius = (wheel.GetComponent<MeshRenderer>().bounds.size.y / 2f) / transform.localScale.y;
			
			JointSpring spring = wheelcolliderL.GetComponent<WheelCollider>().suspensionSpring;
			
			spring.spring = 50000f;
			spring.damper = 5000f;
			spring.targetPosition = .5f;

			wheelcolliderL.GetComponent<WheelCollider>().mass = 200f;
			wheelcolliderL.GetComponent<WheelCollider>().wheelDampingRate = 10f;
			wheelcolliderL.GetComponent<WheelCollider>().suspensionDistance = .3f;
			wheelcolliderL.GetComponent<WheelCollider>().forceAppPointDistance = .25f;
			wheelcolliderL.GetComponent<WheelCollider>().suspensionSpring = spring;

			wheelcolliderL.transform.localPosition = new Vector3(wheelcolliderL.transform.localPosition.x, wheelcolliderL.transform.localPosition.y + (wheelcolliderL.GetComponent<WheelCollider>().suspensionDistance / 2f), wheelcolliderL.transform.localPosition.z);
			
			WheelFrictionCurve sidewaysFriction = wheelcolliderL.GetComponent<WheelCollider>().sidewaysFriction;
			WheelFrictionCurve forwardFriction = wheelcolliderL.GetComponent<WheelCollider>().forwardFriction;
			
			forwardFriction.extremumSlip = .4f;
			forwardFriction.extremumValue = 1;
			forwardFriction.asymptoteSlip = .8f;
			forwardFriction.asymptoteValue = .75f;
			forwardFriction.stiffness = 1.75f;
			
			sidewaysFriction.extremumSlip = .25f;
			sidewaysFriction.extremumValue = 1;
			sidewaysFriction.asymptoteSlip = .5f;
			sidewaysFriction.asymptoteValue = .75f;
			sidewaysFriction.stiffness = 2f;
			
			wheelcolliderL.GetComponent<WheelCollider>().sidewaysFriction = sidewaysFriction;
			wheelcolliderL.GetComponent<WheelCollider>().forwardFriction = forwardFriction;
			
		}

		foreach(Transform wheel in allWheelTransformsR){
			
			GameObject wheelcolliderR = new GameObject(wheel.transform.name); 
			
			wheelcolliderR.transform.position = wheel.transform.position;
			wheelcolliderR.transform.rotation = transform.rotation;
			wheelcolliderR.transform.name = wheel.transform.name;
			wheelcolliderR.transform.parent = _WheelColliders_R.transform;
			wheelcolliderR.transform.localScale = Vector3.one;
			wheelcolliderR.AddComponent<WheelCollider>();
			wheelcolliderR.GetComponent<WheelCollider>().radius = (wheel.GetComponent<MeshRenderer>().bounds.size.y / 2f) / transform.localScale.y;
			
			JointSpring spring = wheelcolliderR.GetComponent<WheelCollider>().suspensionSpring;
			
			spring.spring = 50000f;
			spring.damper = 5000f;
			spring.targetPosition = .5f;
			
			wheelcolliderR.GetComponent<WheelCollider>().mass = 200f;
			wheelcolliderR.GetComponent<WheelCollider>().wheelDampingRate = 10f;
			wheelcolliderR.GetComponent<WheelCollider>().suspensionDistance = .3f;
			wheelcolliderR.GetComponent<WheelCollider>().forceAppPointDistance = .25f;
			wheelcolliderR.GetComponent<WheelCollider>().suspensionSpring = spring;

			wheelcolliderR.transform.localPosition = new Vector3(wheelcolliderR.transform.localPosition.x, wheelcolliderR.transform.localPosition.y + (wheelcolliderR.GetComponent<WheelCollider>().suspensionDistance / 2f), wheelcolliderR.transform.localPosition.z);
			
			WheelFrictionCurve sidewaysFriction = wheelcolliderR.GetComponent<WheelCollider>().sidewaysFriction;
			WheelFrictionCurve forwardFriction = wheelcolliderR.GetComponent<WheelCollider>().forwardFriction;
			
			forwardFriction.extremumSlip = .4f;
			forwardFriction.extremumValue = 1;
			forwardFriction.asymptoteSlip = .8f;
			forwardFriction.asymptoteValue = .75f;
			forwardFriction.stiffness = 1.75f;
			
			sidewaysFriction.extremumSlip = .25f;
			sidewaysFriction.extremumValue = 1;
			sidewaysFriction.asymptoteSlip = .5f;
			sidewaysFriction.asymptoteValue = .75f;
			sidewaysFriction.stiffness = 2f;
			
			wheelcolliderR.GetComponent<WheelCollider>().sidewaysFriction = sidewaysFriction;
			wheelcolliderR.GetComponent<WheelCollider>().forwardFriction = forwardFriction;
			
		}
		#endregion

		wheelColliders_L = _WheelColliders_L.GetComponentsInChildren<WheelCollider>();
		wheelColliders_R = _WheelColliders_R.GetComponentsInChildren<WheelCollider>();
	}

	protected virtual void WheelCollidersInit()
	{
		WheelCollider[] wheelcolliders = GetComponentsInChildren<WheelCollider>();
		
		foreach(WheelCollider wc in wheelcolliders){
			allWheelColliders.Add (wc);
		}
	}

    protected virtual void SoundsInit()
	{
		engineIdleAudio = RTCCreateAudioSource.NewAudioSource(gameObject, "engineIdleAudio", 5f, .5f, engineIdleAudioClip, true, true, false);
		engineRunningAudio = RTCCreateAudioSource.NewAudioSource(gameObject, "engineRunningAudio", 5f, 0f, engineRunningAudioClip, true, true, false);
		brakeAudio = RTCCreateAudioSource.NewAudioSource(gameObject, "Brake Sound AudioSource", 5, 0, brakeClip, true, true, false);
	}

	public virtual void KillOrStartEngine ()
	{
		if(engineRunning && !engineStarting){
			engineRunning = false;
		}else if(!engineStarting){
			StartCoroutine("StartEngine");
		}
	}

	protected virtual IEnumerator StartEngine ()
	{
		engineRunning = false;
		engineStarting = true;
		if(!engineRunning)
			engineStartUpAudio = RTCCreateAudioSource.NewAudioSource(gameObject, "Engine Start AudioSource", 5, 1, engineStartUpAudioClip, false, true, true);
		yield return new WaitForSeconds(1f);
		engineRunning = true;
		yield return new WaitForSeconds(1f);
		engineStarting = false;
	}

    protected virtual void SmokeInit()
	{
		for(int i = 0; i < allWheelColliders.Count; i++){
			GameObject wp = (GameObject)Instantiate(wheelSlip, allWheelColliders[i].transform.position, transform.rotation) as GameObject;
			wheelParticles.Add (wp.GetComponent<ParticleSystem>());
		}
		
		for(int i = 0; i < allWheelColliders.Count; i++){
			wheelParticles[i].transform.position = allWheelColliders[i].transform.position;
			wheelParticles[i].transform.parent = allWheelColliders[i].transform;
		}
	}

    protected virtual void Update()
	{
		WheelAlign();
		Sounds();
	}

    protected virtual void FixedUpdate ()
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
		if(gasInput < 0  && transform.InverseTransformDirection(rigid.velocity).z < 1 && canGoReverseNow)
			reversing = true;
		else
			reversing = false;
		
		speed = rigid.velocity.magnitude * 3.0f;
		
		//Acceleration Calculation.
		acceleration = 0f;
		acceleration = (transform.InverseTransformDirection(rigid.velocity).z - lastVelocity) / Time.fixedDeltaTime;
		lastVelocity = transform.InverseTransformDirection(rigid.velocity).z;
		
		//Drag Limit.
		rigid.drag = Mathf.Clamp((acceleration / 30f), 0f, 1f);

		float rpm = 0f;
		float wheelRPM = ((Mathf.Abs((wheelColliders_L[2].rpm * wheelColliders_L[2].radius) + (wheelColliders_R[2].rpm * wheelColliders_R[2].radius)) / 2f) / 3.25f);

		if(!reversing)
			rpm = Mathf.Clamp((Mathf.Lerp(minEngineRPM, maxEngineRPM, wheelRPM / maxSpeed) + minEngineRPM) * (Mathf.Clamp01(gasInput + Mathf.Abs(steerInput))), minEngineRPM, maxEngineRPM);
		else
			rpm = Mathf.Clamp((Mathf.Lerp(minEngineRPM, maxEngineRPM, wheelRPM / maxSpeed) + minEngineRPM) * (Mathf.Clamp01(brakeInput + Mathf.Abs(steerInput))), minEngineRPM, maxEngineRPM);

		engineRPM = Mathf.Lerp(engineRPM, (rpm + UnityEngine.Random.Range(-50f, 50f)) * fuelInput, Time.deltaTime * 2f);

		if(!engineRunning)
			fuelInput = 0;
		else
			fuelInput = 1;

		//Auto Reverse Bool.
		if(autoReverse){
			canGoReverseNow = true;
		}else{
			if(brakeInput > .1f && speed < 5)
				reverseDelay += Time.deltaTime;
			else if(brakeInput > 0 && transform.InverseTransformDirection(rigid.velocity).z > 1f)
				reverseDelay = 0f;
		}

		if(reverseDelay >= .75f)
			canGoReverseNow = true;
		else
			canGoReverseNow = false;

		for(int i = 0; i < wheelColliders_L.Length; i++){

			ApplyMotorTorque(wheelColliders_L[i], engineTorque, true);
			wheelColliders_L[i].wheelDampingRate = Mathf.Lerp(100f, 0f, gasInput);

		}

		for(int i = 0; i < wheelColliders_R.Length; i++){
			
			ApplyMotorTorque(wheelColliders_R[i], engineTorque, false);
			wheelColliders_R[i].wheelDampingRate = Mathf.Lerp(100f, 0f, gasInput);
			
		}

		//Steering.
		if(!reversing){
			if(wheelColliders_L[2].isGrounded || wheelColliders_R[2].isGrounded){
				if(Mathf.Abs(rigid.angularVelocity.y) < 1f){
					rigid.AddRelativeTorque((Vector3.up * steerInput) * steerTorque, ForceMode.Acceleration);
				}
			}
		}else{
			if(wheelColliders_L[2].isGrounded || wheelColliders_R[2].isGrounded){
				if(Mathf.Abs(rigid.angularVelocity.y) < 1f){
					rigid.AddRelativeTorque((-Vector3.up * steerInput) * steerTorque, ForceMode.Acceleration);
				}
			}
		}
	}

	public virtual void ApplyMotorTorque(WheelCollider wc, float torque, bool leftSide)
	{
		WheelHit hit;
		wc.GetGroundHit(out hit);
		
		if(Mathf.Abs(hit.forwardSlip) > 1f)
			torque = 0;
		
		if(speed > maxSpeed || Mathf.Abs(wc.rpm) > 1000 || Mathf.Abs(wc.rpm) > 1000 || !engineRunning)
			torque = 0;
		
		if(reversing && speed > 55)
			torque = 0;
		
		if(!engineRunning || !wc.isGrounded)
			torque = 0;

		if(!reversing){
			if(leftSide)
				wc.motorTorque = torque * Mathf.Clamp((Mathf.Clamp(gasInput * fuelInput, 0f, 1f)) + Mathf.Clamp(steerInput, -1f, 1f), -1f, 1f) * engineTorqueCurve.Evaluate(speed);
			else
				wc.motorTorque = torque * Mathf.Clamp((Mathf.Clamp(gasInput * fuelInput, 0f, 1f)) + Mathf.Clamp(-steerInput, -1f, 1f), -1f, 1f) * engineTorqueCurve.Evaluate(speed);
		}else{
			wc.motorTorque =  (-torque) * brakeInput;
		}
	}
	
	public virtual void ApplyBrakeTorque(WheelCollider wc, float brake)
	{
		wc.brakeTorque = brake;
	}
	
	public virtual void Braking ()
	{
		for(int i = 0; i < allWheelColliders.Count; i++){
			
			if(brakeInput > .1f && !reversing){
				ApplyBrakeTorque(allWheelColliders[i], brakeTorque * (brakeInput));
			}else if(brakeInput > .1f && reversing){
				ApplyBrakeTorque(allWheelColliders[i], 0f);
			}else if(gasInput < .1f && Mathf.Abs(steerInput) < .1f){
				ApplyBrakeTorque(allWheelColliders[i], 10f);
			}else{
				ApplyBrakeTorque(allWheelColliders[i], 0f);
			}
		}
	}

    protected virtual void Inputs()
	{
		if(!canControl){
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
		if(engineRunningAudioClip){
			
			if(!reversing)
				engineRunningAudio.volume = Mathf.Lerp (engineRunningAudio.volume, Mathf.Clamp (Mathf.Clamp01(gasInput + Mathf.Abs(steerInput / 2f)), minEngineSoundVolume, maxEngineSoundVolume), Time.deltaTime * 10f);
			else
				engineRunningAudio.volume = Mathf.Lerp (engineRunningAudio.volume, Mathf.Clamp (brakeInput, minEngineSoundVolume, maxEngineSoundVolume), Time.deltaTime * 10f);
			
			if(engineRunning)
				engineRunningAudio.pitch = Mathf.Lerp (engineRunningAudio.pitch, Mathf.Lerp (minEngineSoundPitch, maxEngineSoundPitch, (engineRPM) / (maxEngineRPM)), Time.deltaTime * 10f);
			else
				engineRunningAudio.pitch = Mathf.Lerp (engineRunningAudio.pitch, 0, Time.deltaTime * 10f);
			
		}
		
		if(engineIdleAudioClip){
			
			if(!reversing)
				engineIdleAudio.volume = Mathf.Lerp (engineIdleAudio.volume, Mathf.Clamp ((1 + (-gasInput)), minEngineSoundVolume, 1f), Time.deltaTime * 10f);
			else
				engineIdleAudio.volume = Mathf.Lerp (engineIdleAudio.volume, Mathf.Clamp ((1 + (-brakeInput)), minEngineSoundVolume, 1f), Time.deltaTime * 10f);
			
			if(engineRunning)
				engineIdleAudio.pitch = Mathf.Lerp (engineIdleAudio.pitch, Mathf.Lerp (minEngineSoundPitch, maxEngineSoundPitch, (engineRPM) / (maxEngineRPM)), Time.deltaTime * 10f);
			else
				engineIdleAudio.pitch = Mathf.Lerp (engineIdleAudio.pitch, 0, Time.deltaTime * 10f);
			
		}

		if(!reversing)
			brakeAudio.volume = Mathf.Lerp (0f, maxBrakeSoundVolume, Mathf.Clamp01(brakeInput) * Mathf.Lerp(0f, 1f, wheelColliders_L[2].rpm / 50f));
		else
			brakeAudio.volume = 0f;
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

    protected virtual void WheelAlign ()
	{
		RaycastHit hit;
			
		//Right Wheels Transform.
		for(int k = 0; k < wheelColliders_R.Length; k++){
			
			Vector3 ColliderCenterPoint = wheelColliders_R[k].transform.TransformPoint( wheelColliders_R[k].center );
			
			if(Physics.Raycast( ColliderCenterPoint, -wheelColliders_R[k].transform.up, out hit, (wheelColliders_R[k].suspensionDistance + wheelColliders_R[k].radius) * transform.localScale.y) && hit.transform.root != transform){
				wheelTransform_R[k].transform.position = hit.point + (wheelColliders_R[k].transform.up * wheelColliders_R[k].radius) * transform.localScale.y;
				if(trackBoneTransform_R.Length > 0)
					trackBoneTransform_R[k].transform.position = hit.point + (wheelColliders_R[k].transform.up * trackOffset) * transform.localScale.y;
			}else{
				wheelTransform_R[k].transform.position = ColliderCenterPoint - (wheelColliders_R[k].transform.up * wheelColliders_R[k].suspensionDistance) * transform.localScale.y;
				if(trackBoneTransform_R.Length > 0)
					trackBoneTransform_R[k].transform.position = ColliderCenterPoint - (wheelColliders_R[k].transform.up * (wheelColliders_R[k].suspensionDistance + wheelColliders_R[k].radius - trackOffset)) * transform.localScale.y;
			}
			
			wheelTransform_R[k].transform.rotation = wheelColliders_R[k].transform.rotation * Quaternion.Euler( rotationValueR[Mathf.CeilToInt((wheelColliders_R.Length) / 2)], 0, 0);
			rotationValueR[k] += wheelColliders_R[k].rpm * ( 6 ) * Time.deltaTime;
			
		}
		
		//Left Wheels Transform.
		for(int i = 0; i < wheelColliders_L.Length; i++){
			
			Vector3 ColliderCenterPoint = wheelColliders_L[i].transform.TransformPoint( wheelColliders_L[i].center );
			
			if(Physics.Raycast( ColliderCenterPoint, -wheelColliders_L[i].transform.up, out hit, (wheelColliders_L[i].suspensionDistance + wheelColliders_L[i].radius) * transform.localScale.y) && hit.transform.root != transform){
				wheelTransform_L[i].transform.position = hit.point + (wheelColliders_L[i].transform.up * wheelColliders_L[i].radius) * transform.localScale.y;
				if(trackBoneTransform_L.Length > 0)
					trackBoneTransform_L[i].transform.position = hit.point + (wheelColliders_L[i].transform.up * trackOffset) * transform.localScale.y;
			}else{
				wheelTransform_L[i].transform.position = ColliderCenterPoint - (wheelColliders_L[i].transform.up * wheelColliders_L[i].suspensionDistance) * transform.localScale.y;
				if(trackBoneTransform_L.Length > 0)
					trackBoneTransform_L[i].transform.position = ColliderCenterPoint - (wheelColliders_L[i].transform.up * (wheelColliders_L[i].suspensionDistance + wheelColliders_L[i].radius - trackOffset)) * transform.localScale.y;
			}
			
			wheelTransform_L[i].transform.rotation = wheelColliders_L[i].transform.rotation * Quaternion.Euler( rotationValueL[Mathf.CeilToInt((wheelColliders_L.Length) / 2)], 0, 0);
			rotationValueL[i] += wheelColliders_L[i].rpm * ( 6 ) * Time.deltaTime;
			
		}
		
		//Scrolling Track Texture Offset.
		if(leftTrackMesh && rightTrackMesh){
			leftTrackMesh.GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2((rotationValueL[Mathf.CeilToInt((wheelColliders_L.Length) / 2)]/1000) * trackScrollSpeedMultiplier, 0));
			rightTrackMesh.GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2((rotationValueR[Mathf.CeilToInt((wheelColliders_R.Length) / 2)]/1000) * trackScrollSpeedMultiplier, 0));
			leftTrackMesh.GetComponent<Renderer>().material.SetTextureOffset("_BumpMap", new Vector2((rotationValueL[Mathf.CeilToInt((wheelColliders_L.Length) / 2)]/1000) * trackScrollSpeedMultiplier, 0));
			rightTrackMesh.GetComponent<Renderer>().material.SetTextureOffset("_BumpMap", new Vector2((rotationValueR[Mathf.CeilToInt((wheelColliders_R.Length) / 2)]/1000) * trackScrollSpeedMultiplier, 0));
		}
	}

    protected virtual void Smoke ()
	{
		if (wheelParticles.Count > 0){

			for(int i = 0; i < allWheelColliders.Count; i++){

				WheelHit CorrespondingGroundHit;
				allWheelColliders[i].GetGroundHit( out CorrespondingGroundHit );

				if(speed > 1f && allWheelColliders[i].isGrounded){
					wheelParticles[i].enableEmission = true;
					wheelParticles[i].emissionRate = Mathf.Lerp(0f, 10f, speed / 25f);
				}else{
					wheelParticles[i].enableEmission = false;
				}

			}
			
		}
			
		if(exhaustSmoke){
			exhaustSmoke.emissionRate = Mathf.Lerp(0f, 50f, engineRPM / maxEngineRPM);
			exhaustSmoke.startSpeed = Mathf.Lerp(0f, 10f, engineRPM / maxEngineRPM);
			exhaustSmoke.startSize = Mathf.Lerp(.1f, 1f, engineRPM / maxEngineRPM);
		}
		
	}
}
