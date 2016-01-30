//===================== Copyright (c) Valve Corporation. All Rights Reserved. ======================

using UnityEngine;
using System.Collections;

[RequireComponent( typeof( VRInteractable ) )]
[RequireComponent( typeof( Rigidbody ) )]
[RequireComponent( typeof( VelocityEstimator ) )]
public class VRThrowable : MonoBehaviour
{
	[Tooltip( "Should this object snap to the specified attachment transform, or keep its current position?" )]
	public bool snapOnAttach = false;

	[Tooltip( "Name of the attachment transform under in the hand's hierarchy which the object should should snap to." )]
	public string attachmentPoint;

	[Tooltip( "Should the hand detach all of its attached objects when attaching this one?" )]
	public bool detachOthers = false;

	[Tooltip( "How fast must this object be moving to attach due to a trigger hold instead of a trigger press?" )]
	public float catchSpeedThreshold = 0.0f;

	[Tooltip( "When detaching the object, should it return to its original parent?" )]
	public bool restoreOriginalParent = false;

	private VelocityEstimator velocityEstimator;
	private bool attached = false;

	void Awake()
	{
		velocityEstimator = GetComponent<VelocityEstimator>();
	}

	void OnHandHoverBegin( VRHand hand )
	{
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
					hand.AttachObject( gameObject, snapOnAttach, attachmentPoint, detachOthers );
				}
			}
		}
	}

	void HandHoverUpdate( VRHand hand )
	{
		//Trigger got pressed
		if ( hand.GetStandardInteractionButtonDown() )
		{
			hand.AttachObject( gameObject, snapOnAttach, attachmentPoint, detachOthers );
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
		// point physics will happily take over simulation
		float t = ( Time.fixedDeltaTime + Time.fixedTime ) - Time.time;
		transform.position += t * velocity;
		// TODO: update transform.rotation? probably too subtle, but should be fixed
		// TODO: verify the fix using large fixed timestep, determine if the warning should be deleted
	}

	void HandAttachedUpdate( VRHand hand )
	{
		//Trigger got released
		if ( hand.GetStandardInteractionButtonUp() )
		{
			// Detach ourselves late in the frame.
			// This is so that any vehicles the player is attached to
			// have a chance to update themselves.
			// If we detach now, our position will be behind the frame,
			// and it will feel like a weird teleport.
			StartCoroutine( LateDetach( hand ) );
		}
	}

	IEnumerator LateDetach( VRHand hand )
	{
		yield return new WaitForEndOfFrame();

		hand.DetachObject( gameObject, restoreOriginalParent );
	}
}
