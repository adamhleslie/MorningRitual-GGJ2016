//===================== Copyright (c) Valve Corporation. All Rights Reserved. ======================

using UnityEngine;
using System.Collections;

[RequireComponent( typeof( VRInteractable ) )]
[RequireComponent( typeof( Rigidbody ) )]
[RequireComponent( typeof( VelocityEstimator ) )]
public class VRThrowable : MonoBehaviour
{
	[EnumFlags]
	[Tooltip( "The flags used to attach this object to the hand." )]
	public VRHand.AttachmentFlags attachmentFlags = VRHand.AttachmentFlags.ParentToHand | VRHand.AttachmentFlags.DetachFromOtherHand;

	[Tooltip( "Name of the attachment transform under in the hand's hierarchy which the object should should snap to." )]
	public string attachmentPoint;

	[Tooltip( "How fast must this object be moving to attach due to a trigger hold instead of a trigger press?" )]
	public float catchSpeedThreshold = 0.0f;

	[Tooltip( "When detaching the object, should it return to its original parent?" )]
	public bool restoreOriginalParent = false;

	private VelocityEstimator velocityEstimator;
	bool attached = false;

	void Awake()
	{
		velocityEstimator = GetComponent<VelocityEstimator>();
	}

	void OnHandHoverBegin( VRHand hand )
	{
		bool showHint = true;

		// "Catch" the throwable by holding down the interaction button instead of pressing it.
		// Only do this if the throwable is moving faster than the prescribed threshold speed,
		// and if it isn't attached to another hand
		if ( !attached )
		{
			if ( hand.GetStandardInteractionButton() )
			{
				Rigidbody rb = GetComponent<Rigidbody>();
				if ( rb.velocity.magnitude >= catchSpeedThreshold )
				{
					hand.AttachObject( gameObject, attachmentFlags, attachmentPoint );
					showHint = false;
				}
			}
		}

		if ( showHint )
		{
			VRControllerButtonHints.Show( hand, Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger );
		}
	}

	void OnHandHoverEnd( VRHand hand )
	{
		VRControllerButtonHints.Hide( hand );
	}

	void HandHoverUpdate( VRHand hand )
	{
		//Trigger got pressed
		if ( hand.GetStandardInteractionButtonDown() )
		{
			hand.AttachObject( gameObject, attachmentFlags, attachmentPoint );
			VRControllerButtonHints.Hide( hand );
		}
	}

	void OnAttachedToHand( VRHand hand )
	{
		attached = true;

		hand.HoverLock( null );
		
		Rigidbody rb = GetComponent<Rigidbody>();
		rb.isKinematic = true;
		rb.interpolation = RigidbodyInterpolation.None;

		velocityEstimator.BeginEstimatingVelocity();
	}

	void OnDetachedFromHand( VRHand hand )
	{
		attached = false;

		hand.HoverUnlock( null );

		Rigidbody rb = GetComponent<Rigidbody>();
		rb.isKinematic = false;
		rb.interpolation = RigidbodyInterpolation.Interpolate;

		velocityEstimator.FinishEstimatingVelocity();
		Vector3 velocity = velocityEstimator.GetVelocityEstimate();
		Vector3 angularVelocity = velocityEstimator.GetAngularVelocityEstimate();
		rb.velocity = velocity;
		rb.angularVelocity = angularVelocity;

		// Make the object travel at the release velocity for the amount
		// of time it will take until the next fixed update, at which
		// point Unity physics will take over
		float timeUntilFixedUpdate = ( Time.fixedDeltaTime + Time.fixedTime ) - Time.time;
		transform.position += timeUntilFixedUpdate * velocity;
		float angle = Mathf.Rad2Deg * angularVelocity.magnitude;
		Vector3 axis = angularVelocity.normalized;
		transform.rotation *= Quaternion.AngleAxis( angle * timeUntilFixedUpdate, axis );
	}

	void HandAttachedUpdate( VRHand hand )
	{
		//Trigger got released
		if ( hand.GetStandardInteractionButtonUp() )
		{
			// Detach ourselves late in the frame.
			// This is so that any vehicles the player is attached to
			// have a chance to finish updating themselves.
			// If we detach now, our position could be behind what it
			// will be at the end of the frame, and the object may appear
			// to teleport behind the hand when the player releases it.
			StartCoroutine( LateDetach( hand ) );
		}
	}

	IEnumerator LateDetach( VRHand hand )
	{
		yield return new WaitForEndOfFrame();

		hand.DetachObject( gameObject, restoreOriginalParent );
	}


	void OnHandFocusAcquired( VRHand hand )
	{
		gameObject.SetActive( true );
		velocityEstimator.BeginEstimatingVelocity();
	}


	void OnHandFocusLost( VRHand hand )
	{
		gameObject.SetActive( false );
		velocityEstimator.FinishEstimatingVelocity();
	}
}
