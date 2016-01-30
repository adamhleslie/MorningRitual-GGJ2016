//===================== Copyright (c) Valve Corporation. All Rights Reserved. ======================

using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[RequireComponent( typeof( VRInteractable ) )]
public class VRInteractableLinearDriveEvents : MonoBehaviour
{
	public UnityEvent onLinearMappingReachStart;
	public UnityEvent onLinearMappingLeaveStart;
	public UnityEvent onLinearMappingReachEnd;
	public UnityEvent onLinearMappingLeaveEnd;

	void OnReachStart()
	{
		onLinearMappingReachStart.Invoke();
	}

	void OnLeaveStart()
	{
		onLinearMappingLeaveStart.Invoke();
	}

	void OnReachEnd()
	{
		onLinearMappingReachEnd.Invoke();
	}

	void OnLeaveEnd()
	{
		onLinearMappingLeaveEnd.Invoke();
	}
}
