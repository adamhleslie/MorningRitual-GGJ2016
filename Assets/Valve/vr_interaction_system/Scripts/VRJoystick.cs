using UnityEngine;
using System.Collections;
using Valve.VR;

public class VRJoystick : MonoBehaviour
{
	public Transform playerOrigin;
	public float deadZone = 0.2f;

	void Start ()
	{

	}
	
	public Vector2 GetJoystickAxes( Transform hand )
	{
		Vector2 axesRaw = new Vector2();

		float x = Vector3.Dot( hand.transform.forward, playerOrigin.right );
		float y = Vector3.Dot( hand.transform.forward, playerOrigin.forward );

		axesRaw.x = x;
		axesRaw.y = y;

		// TODO: joystick rotation limits and deadzone

		return axesRaw;
	}
}
