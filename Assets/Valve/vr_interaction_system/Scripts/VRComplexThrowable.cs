//===================== Copyright (c) Valve Corporation. All Rights Reserved. ======================

using UnityEngine;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
[RequireComponent( typeof( VRInteractable ) )]
public class VRComplexThrowable : MonoBehaviour
{
	public enum AttachMode
	{
		FixedJoint,
		Force,
	}

	//Public variables
	public float attachForce = 800.0f;
	public float attachForceDamper = 25.0f;

	public AttachMode attachMode = AttachMode.FixedJoint;

	[EnumFlags]
	public VRHand.AttachmentFlags attachmentFlags = 0;

	//Private data
	private List<VRHand> holdingHands = new List<VRHand>();
	private List<Rigidbody> holdingBodies = new List<Rigidbody>();
	private List<Vector3> holdingPoints = new List<Vector3>();

	private List<Rigidbody> rigidBodies = new List<Rigidbody>();

	//-----------------------------------------------------
	void Awake()
	{
		GetComponentsInChildren<Rigidbody>( rigidBodies );
	}


	//-----------------------------------------------------
	void Update()
	{
		for ( int i = 0; i < holdingHands.Count; i++ )
		{
			if ( !holdingHands[i].GetStandardInteractionButton() )
			{
				PhysicsDetach( holdingHands[i] );
			}
		}
	}


	//-----------------------------------------------------
	void OnHandHoverBegin( VRHand hand )
	{
		if ( holdingHands.IndexOf( hand ) == -1 )
		{
			if ( hand.controller != null )
			{
				hand.controller.TriggerHapticPulse( 800 );
			}
		}
	}


	//-----------------------------------------------------
	void OnHandHoverEnd( VRHand hand )
	{
		if ( holdingHands.IndexOf( hand ) == -1 )
		{
			if ( hand.controller != null )
			{
				hand.controller.TriggerHapticPulse( 500 );
			}
		}
	}


	//-----------------------------------------------------
	void HandHoverUpdate( VRHand hand )
	{
		if ( hand.GetStandardInteractionButtonDown() )
		{
			PhysicsAttach( hand );
		}
	}


	//-----------------------------------------------------
	public void PhysicsAttach( VRHand hand )
	{
		PhysicsDetach( hand );

		Rigidbody holdingBody = null;
		Vector3 holdingPoint = Vector3.zero;

		// The hand should grab onto the nearest rigid body
		float closestDistance = float.MaxValue;
		for ( int i = 0; i < rigidBodies.Count; i++ )
		{
			float distance = Vector3.Distance( rigidBodies[i].worldCenterOfMass, hand.transform.position );
			if ( distance < closestDistance )
			{
				holdingBody = rigidBodies[i];
				closestDistance = distance;
			}
		}

		// Couldn't grab onto a body
		if ( holdingBody == null )
			return;

		// Create a fixed joint from the hand to the holding body
		if ( attachMode == AttachMode.FixedJoint )
		{
			Rigidbody handRigidbody = FindOrAddComponent<Rigidbody>( hand.gameObject );
			handRigidbody.isKinematic = true;

			FixedJoint handJoint = hand.gameObject.AddComponent<FixedJoint>();
			handJoint.connectedBody = holdingBody;
		}

		// Don't let the hand interact with other things while it's holding us
		hand.HoverLock( null );

		// Affix this point
		Vector3 offset = hand.transform.position - holdingBody.worldCenterOfMass;
		offset = Mathf.Min( offset.magnitude, 1.0f ) * offset.normalized;
		holdingPoint = holdingBody.transform.InverseTransformPoint( holdingBody.worldCenterOfMass + offset );

		hand.AttachObject( this.gameObject, attachmentFlags );

		// Update holding list
		holdingHands.Add( hand );
		holdingBodies.Add( holdingBody );
		holdingPoints.Add( holdingPoint );
	}


	//-----------------------------------------------------
	private bool PhysicsDetach( VRHand hand )
	{
		int i = holdingHands.IndexOf( hand );

		if ( i != -1 )
		{
			// Detach this object from the hand
			holdingHands[i].DetachObject( this.gameObject, false );

			// Allow the hand to do other things
			holdingHands[i].HoverUnlock( null );

			// Delete any existing joints from the hand
			if ( attachMode == AttachMode.FixedJoint )
			{
				Destroy( holdingHands[i].GetComponent<FixedJoint>() );
			}

			FastRemove( holdingHands, i );
			FastRemove( holdingBodies, i );
			FastRemove( holdingPoints, i );

			return true;
		}

		return false;
	}


	//-----------------------------------------------------
	void FixedUpdate()
	{
		if ( attachMode == AttachMode.Force )
		{
			for ( int i = 0; i < holdingHands.Count; i++ )
			{
				Vector3 targetPoint = holdingBodies[i].transform.TransformPoint( holdingPoints[i] );
				Vector3 vdisplacement = holdingHands[i].transform.position - targetPoint;

				holdingBodies[i].AddForceAtPosition( attachForce * vdisplacement, targetPoint, ForceMode.Acceleration );
				holdingBodies[i].AddForceAtPosition( -attachForceDamper * holdingBodies[i].GetPointVelocity( targetPoint ), targetPoint, ForceMode.Acceleration );
			}
		}
	}

	private T FindOrAddComponent<T>( GameObject gameObject ) where T : Component
	{
		T component = gameObject.GetComponent<T>();
		if ( component )
			return component;

		return gameObject.AddComponent<T>();
	}

	private void FastRemove<T>( List<T> list, int index )
	{
		list[index] = list[list.Count - 1];
		list.RemoveAt( list.Count - 1 );
	}
}
