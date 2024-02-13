#pragma warning disable 0414

using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]
[RequireComponent (typeof (HingeJoint))]

public class RTCTankGunController : MonoBehaviour {

	protected Rigidbody rigid;
	protected RTCTankController tank;
	protected GameObject tankCamera;

	public bool canControl = true;

	public GameObject barrel;
	public Transform barrelOut;
	protected HingeJoint joint;
	protected JointLimits jointRotationLimit;
	 
	protected float inputSteer;

	public int rotationTorque = 1000;
	public float maximumAngularVelocity = 1.5f;
	public int maximumRotationLimit = 160;
	public float minimumElevationLimit = 10;
	public float maximumElevationLimit = 25;
	public bool useLimitsForRotation = true;

	protected float rotationVelocity;
	public float rotationOfTheGun;

	[HideInInspector]
	public Transform target;

	public GameObject bullet;
	public int bulletVelocity = 250;
	public int recoilForce = 10000;
	public int ammo = 15;
	public float reloadTime = 3f;
	protected float loadingTime = 3f;

	protected AudioSource fireSoundSource;
	public AudioClip fireSoundClip;

	public GameObject groundSmoke;
	public GameObject fireSmoke;

	protected virtual void Awake () {

		rigid = GetComponent<Rigidbody>();
		tank = transform.root.gameObject.GetComponent<RTCTankController>();
		tankCamera = GameObject.FindObjectOfType<RTCCamera>().gameObject;

		GameObject newTarget = new GameObject("Target");
		target = newTarget.transform;
	
		rigid.maxAngularVelocity = maximumAngularVelocity;
		rigid.interpolation = RigidbodyInterpolation.None;
		rigid.interpolation = RigidbodyInterpolation.Interpolate;

		joint = GetComponent<HingeJoint>();

	}

    protected virtual void Update(){

		if(!tank.canControl || !canControl)
			return;

		Shooting();
		JointConfiguration();

	}

    protected virtual void FixedUpdate () {

		if(!tank.canControl || !canControl)
			return;

		if(transform.localEulerAngles.y > 0 && transform.localEulerAngles.y < 180)
			rotationOfTheGun = transform.localEulerAngles.y;
		else
			rotationOfTheGun = transform.localEulerAngles.y - 360;
	
		Vector3 targetPosition = transform.InverseTransformPoint(new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z));

		inputSteer = (targetPosition.x / targetPosition.magnitude);
		rotationVelocity = rigid.angularVelocity.y;

		rigid.AddRelativeTorque(0, (rotationTorque) * inputSteer, 0, ForceMode.Force);

		Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);
		barrel.transform.rotation = Quaternion.Slerp(barrel.transform.rotation, targetRotation, Time.deltaTime * 4f);

		if(barrel.transform.localEulerAngles.x > 0 && barrel.transform.localEulerAngles.x < 180)
			barrel.transform.localRotation = Quaternion.Euler(new Vector3(Mathf.Clamp (barrel.transform.localEulerAngles.x, -360, minimumElevationLimit), 0, 0));
		if(barrel.transform.localEulerAngles.x > 180 && barrel.transform.localEulerAngles.x < 360)
			barrel.transform.localRotation = Quaternion.Euler(new Vector3(Mathf.Clamp (barrel.transform.localEulerAngles.x -360, -maximumElevationLimit, 360), 0, 0));

	}

    protected virtual void Shooting(){

		loadingTime += Time.deltaTime;
		target.position = tankCamera.transform.position + (tankCamera.transform.forward * 100);

		if(Input.GetButtonDown("Fire1") && loadingTime > reloadTime && ammo > 0){

			rigid.AddForce(-transform.forward * recoilForce, ForceMode.VelocityChange);
			Rigidbody shot = Instantiate(bullet.GetComponent<Rigidbody>(), barrelOut.position, barrelOut.rotation) as Rigidbody;
			shot.AddForce(barrelOut.forward * bulletVelocity, ForceMode.VelocityChange);
			if(groundSmoke)
				Instantiate(groundSmoke, new Vector3(tank.transform.position.x, tank.transform.position.y, tank.transform.position.z), tank.transform.rotation);
			if(fireSmoke)
				Instantiate(fireSmoke, barrelOut.transform.position, barrelOut.transform.rotation);
			fireSoundSource = RTCCreateAudioSource.NewAudioSource(gameObject, "FireSound", 30f, 1f, fireSoundClip, false, true, true);
			ammo --;
			loadingTime = 0;

		}
	}

    protected virtual void JointConfiguration(){

		if(useLimitsForRotation){

			jointRotationLimit.min = -maximumRotationLimit;
			jointRotationLimit.max = maximumRotationLimit;

			joint.limits = jointRotationLimit;
			joint.useLimits = true;

		}else{

			joint.useLimits = false;

		}

	}

}
