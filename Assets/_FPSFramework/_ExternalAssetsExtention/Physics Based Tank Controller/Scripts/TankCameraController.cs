using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Cinemachine;

public class TankCameraController : MonoBehaviour
{
	[SerializeField]
	protected Transform tankTransform;
	[SerializeField]
	protected CinemachineVirtualCamera tankVCam;

	[Space(10)]
	[SerializeField]
	protected float distance = 15.0f;

	[Space(10)]
	[SerializeField]
    protected float xSpeed = 100.0f;
    [SerializeField]
    protected float ySpeed = 100.0f;

	[Space(10)]
    [SerializeField]
    protected float yLimitMin = -20.0f;
    [SerializeField]
    protected float yLimitMax = 40.0f;

	[Space(10)]
    [SerializeField]
    protected float heightOffset = 3.5f;

    protected float xResult;
    protected float yResult;

	protected void Awake()
	{
		Vector3 angles = transform.eulerAngles;
		xResult = angles.x;
		yResult = angles.y;
	}

    protected void OnEnable()
    {
		tankVCam.Priority = 1;
    }

    protected void OnDisable()
    {
		tankVCam.Priority = 0;
    }

    protected void LateUpdate()
	{
		if(tankTransform != null)
		{
			xResult += (float)(Input.GetAxis("Mouse X") * xSpeed * 0.02f);
			yResult -= (float)(Input.GetAxis("Mouse Y") * ySpeed * 0.02f);
			
			yResult = ClampAngle(yResult, yLimitMin, yLimitMax);
			
			Quaternion rotation = Quaternion.Euler(yResult, xResult, 0);
			Vector3 position = rotation * (new Vector3(0.0f, heightOffset, -distance)) + tankTransform.position;
			
			this.transform.rotation = rotation;
			this.transform.position = position;
		}
	}
	
	protected float ClampAngle(float angle, float min, float max)
	{
		if(angle < -360f)
		{
			angle += 360f;
		}

		if(angle > 360f)
		{
			angle -= 360f;
		}

		return Mathf.Clamp (angle, min, max);
	}
}
