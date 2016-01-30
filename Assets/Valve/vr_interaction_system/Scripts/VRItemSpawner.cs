//===================== Copyright (c) Valve Corporation. All Rights Reserved. ======================

using UnityEngine;
using System.Collections;

[RequireComponent( typeof( VRInteractable ) )]
public class VRItemSpawner : MonoBehaviour
{
	//Public variables
	public GameObject objectToSpawn;
	public bool requireTriggerPress;
	public string attachmentPoint;

	[EnumFlags]
	public VRHand.AttachmentFlags attachmentFlags = VRHand.defaultAttachmentFlags;

	//Private variables
	private bool alreadySpawned;

	//-----------------------------------------------------
	void OnHandHoverBegin( VRHand hand )
	{
		Debug.Log( "OnHoverBegin" );
		if ( !requireTriggerPress )
		{
			SpawnAndAttachObject( hand );
		}
	}


	//-----------------------------------------------------
	void OnHandHoverEnd( VRHand hand )
	{
		Debug.Log( "OnHandHoverEnd" );
		alreadySpawned = false;
	}


	//-----------------------------------------------------
	void HandHoverUpdate( VRHand hand )
	{
		if ( requireTriggerPress && hand.GetStandardInteractionButtonDown() )
		{
			SpawnAndAttachObject( hand );
		}
	}


	//-----------------------------------------------------
	private void SpawnAndAttachObject( VRHand hand )
	{
		if ( !alreadySpawned )
		{
			GameObject objectToAttach = GameObject.Instantiate( objectToSpawn );
			hand.AttachObject( objectToAttach, attachmentFlags, attachmentPoint );

			alreadySpawned = true;
		}
	}
}
