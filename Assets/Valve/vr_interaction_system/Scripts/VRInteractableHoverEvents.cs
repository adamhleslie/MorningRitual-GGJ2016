//===================== Copyright (c) Valve Corporation. All Rights Reserved. ======================

using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[RequireComponent( typeof( VRInteractable ) )]
public class VRInteractableHoverEvents : MonoBehaviour
{
	public UnityEvent onHandHoverBegin;
	public UnityEvent onHandHoverEnd;
	public UnityEvent onAttachedToHand;
	public UnityEvent onDetachedFromHand;

	void OnHandHoverBegin()
	{
		onHandHoverBegin.Invoke();
	}

	void OnHandHoverEnd()
	{
		onHandHoverEnd.Invoke();
	}

	void OnAttachedToHand( VRHand hand )
	{
		onAttachedToHand.Invoke();
	}

	void OnDetachedFromHand( VRHand hand )
	{
		onDetachedFromHand.Invoke();
	}
}
