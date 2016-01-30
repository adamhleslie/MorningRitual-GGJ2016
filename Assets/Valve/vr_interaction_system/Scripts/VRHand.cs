//===================== Copyright (c) Valve Corporation. All Rights Reserved. ======================

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

/// <summary>
/// Links with an appropriate SteamVR controller and facilitates
/// interactions with objects in the virtual world.
/// </summary>
/// <seealso cref="VRInteractable"/>
public class VRHand : MonoBehaviour
{
	public enum HandType
	{
		Left,
		Right,
		Any
	};

	/// <summary>
	/// The flags used to determine how an object is attached to the hand.
	/// </summary>
	[Flags]
	public enum AttachmentFlags
	{
		///<summary>The object should snap to the position of the specified attachment point on the hand.</summary>
		SnapOnAttach = 1 << 0,
		///<summary>Other objects attached to this hand will be detached.</summary>
		DetachOthers = 1 << 1,
		///<summary>This object will be detached from the other hand.</summary>
		DetachFromOtherHand = 1 << 2,
		///<summary>The object will be parented to the hand.</summary>
		ParentToHand = 1 << 3,
	};

	public const AttachmentFlags defaultAttachmentFlags = AttachmentFlags.ParentToHand |
														  AttachmentFlags.DetachOthers |
														  AttachmentFlags.DetachFromOtherHand |
														  AttachmentFlags.SnapOnAttach;

	public VRHand otherHand;
	public HandType startingHandType;

	public Transform hoverSphereTransform;
	public float hoverSphereRadius = 0.05f;
	public LayerMask hoverLayerMask = -1;
	public float hoverUpdateInterval = 0.1f;

	public Camera noSteamVRFallbackCamera;
	private float noSteamVRFallbackInteractorDistance = -1.0f;

	public SteamVR_Controller.Device controller;

	public bool createRenderModel = false;
	private SteamVR_RenderModel renderModel = null;

	public bool showRenderModel = true;

	public bool showDebugText = false;
	public bool spewDebugText = false;

	public struct AttachedObject
	{
		public GameObject attachedObject;
		public GameObject originalParent;
		public bool isParentedToHand;
	}

	private List<AttachedObject> attachedObjects = new List<AttachedObject>();

	public ReadOnlyCollection<AttachedObject> AttachedObjects
	{
		get { return attachedObjects.AsReadOnly(); }
	}

	public bool hoverLocked { get; private set; }

	private VRInteractable _hoveringInteractable;

	private TextMesh debugText;
	private int prevOverlappingColliders = 0;

	/// <summary>
	/// The VRInteractable object this VRHand is currently hovering over.
	/// </summary>
	public VRInteractable hoveringInteractable
	{
		get { return _hoveringInteractable; }
		set
		{
			if ( _hoveringInteractable != value )
			{
				if ( _hoveringInteractable )
				{
					VRHandDebugLog( "HoverEnd " + _hoveringInteractable.gameObject );
					_hoveringInteractable.SendMessage( "OnHandHoverEnd", this, SendMessageOptions.DontRequireReceiver );
					this.BroadcastMessage( "OnParentHandHoverEnd", _hoveringInteractable, SendMessageOptions.DontRequireReceiver ); // let objects attached to the hand know that a hover has ended
				}

				_hoveringInteractable = value;

				if ( _hoveringInteractable )
				{
					VRHandDebugLog( "HoverBegin " + _hoveringInteractable.gameObject );
					_hoveringInteractable.SendMessage( "OnHandHoverBegin", this, SendMessageOptions.DontRequireReceiver );
					this.BroadcastMessage( "OnParentHandHoverBegin", _hoveringInteractable, SendMessageOptions.DontRequireReceiver ); // let objects attached to the hand know that a hover has begun
				}
			}
		}
	}

	/// <summary>
	/// Active GameObject attached to this VRHand.
	/// </summary>
	public GameObject currentAttachedObject
	{
		get
		{
			CleanUpAttachedObjectStack();

			if ( attachedObjects.Count > 0 )
			{
				return attachedObjects[attachedObjects.Count - 1].attachedObject;
			}

			return null;
		}
	}

	public Transform GetAttachmentTransform( string attachmentPoint = "" )
	{
		Transform attachmentTransform = null;

		if ( !string.IsNullOrEmpty( attachmentPoint ) )
		{
			attachmentTransform = transform.Find( attachmentPoint );
		}

		if ( !attachmentTransform )
		{
			attachmentTransform = this.transform;
		}

		return attachmentTransform;
	}

	/// <summary>
	/// Guess the type of this VRHand.
	/// </summary>
	/// <returns>
	/// If startingHandType is VRHand.Left or VRHand.Right, returns startingHandType.
	/// If otherHand is non-null and both VRHands are linked to controllers, returns
	/// VRHand.Left if this VRHand is leftmost relative to the HMD, otherwise VRHand.Right.
	/// Otherwise, returns VRHand.Any.
	/// </returns>
	public HandType GuessCurrentHandType()
	{
		if ( startingHandType == HandType.Left || startingHandType == HandType.Right )
		{
			return startingHandType;
		}

		if ( startingHandType == HandType.Any && otherHand != null && otherHand.controller == null )
		{
			return HandType.Right;
		}

		if ( controller == null || otherHand == null || otherHand.controller == null )
		{
			return startingHandType;
		}

		var invXform = SteamVR_Controller.Input( (int)Valve.VR.OpenVR.k_unTrackedDeviceIndex_Hmd ).transform.GetInverse();

		if ( Vector3.Dot( Vector3.left, invXform * controller.transform.pos ) >
			 Vector3.Dot( Vector3.left, invXform * otherHand.controller.transform.pos ) )
		{
			return HandType.Left;
		}

		return HandType.Right;
	}

	/// <summary>
	/// Attach a GameObject to this GameObject.
	/// </summary>
	/// <param name="objectToAttach">The GameObject to attach.</param>
	/// <param name="flags">The flags to use for attaching the object.</param>
	/// <param name="attachmentPoint">Name of the GameObject in the hierarchy of this VRHand which should act as the attachment point for this GameObject.</param>
	/// <seealso cref="DetachObject"/>
	public void AttachObject( GameObject objectToAttach, AttachmentFlags flags = defaultAttachmentFlags, string attachmentPoint = "" )
	{
		if ( flags == 0 )
		{
			flags = defaultAttachmentFlags;
		}

		//Make sure top object on stack is non-null
		CleanUpAttachedObjectStack();

		//Detach the object if it is already attached so that it can get re-attached at the top of the stack
		DetachObject( objectToAttach );

		//Detach from the other hand if requested
		if ( ( ( flags & AttachmentFlags.DetachFromOtherHand ) == AttachmentFlags.DetachFromOtherHand ) && otherHand )
		{
			otherHand.DetachObject( objectToAttach );
		}

		if ( ( flags & AttachmentFlags.DetachOthers ) == AttachmentFlags.DetachOthers )
		{
			//Detach all the objects from the stack
			while ( attachedObjects.Count > 0 )
			{
				DetachObject( attachedObjects[0].attachedObject );
			}
		}

		if ( currentAttachedObject )
		{
			currentAttachedObject.SendMessage( "OnHandFocusLost", this, SendMessageOptions.DontRequireReceiver );
		}

		AttachedObject attachedObject = new AttachedObject();
		attachedObject.attachedObject = objectToAttach;
		attachedObject.originalParent = objectToAttach.transform.parent != null ? objectToAttach.transform.parent.gameObject : null;
		if ( ( flags & AttachmentFlags.ParentToHand ) == AttachmentFlags.ParentToHand )
		{
			//Parent the object to the hand
			objectToAttach.transform.parent = GetAttachmentTransform( attachmentPoint );
			attachedObject.isParentedToHand = true;
		}
		else
		{
			attachedObject.isParentedToHand = false;
		}
		attachedObjects.Add( attachedObject );

		if ( ( flags & AttachmentFlags.SnapOnAttach ) == AttachmentFlags.SnapOnAttach )
		{
			objectToAttach.transform.localPosition = Vector3.zero;
			objectToAttach.transform.localRotation = Quaternion.identity;
		}

		VRHandDebugLog( "AttachObject " + objectToAttach );
		objectToAttach.SendMessage( "OnAttachedToHand", this, SendMessageOptions.DontRequireReceiver );

		UpdateHovering();
	}

	public void DestroyAttachedObjects()
	{
		//Make sure top object on stack is non-null
		CleanUpAttachedObjectStack();

		//Detach and destroy all the objects from the stack
		while ( attachedObjects.Count > 0 )
		{
			GameObject detachedObject = attachedObjects[0].attachedObject;
			DetachObject( attachedObjects[0].attachedObject );
			if ( detachedObject != null )
			{
				Destroy( detachedObject );
			}
		}
	}

	/// <summary>
	/// Detach this GameObject from the attached object stack of this VRHand.
	/// </summary>
	/// <param name="objectToDetach">The GameObject to detach from this VRHand</param>
	/// <seealso cref="AttachObject"/>
	public void DetachObject( GameObject objectToDetach, bool restoreOriginalParent = true )
	{
		int index = attachedObjects.FindIndex( l => l.attachedObject == objectToDetach );
		if ( index != -1 )
		{
			VRHandDebugLog( "DetachObject " + objectToDetach );

			GameObject prevTopObject = currentAttachedObject;

			Transform parentTransform = null;
			if ( attachedObjects[index].isParentedToHand )
			{
				if ( restoreOriginalParent && ( attachedObjects[index].originalParent != null ) )
				{
					parentTransform = attachedObjects[index].originalParent.transform;
				}
				attachedObjects[index].attachedObject.transform.parent = parentTransform;
			}

			attachedObjects[index].attachedObject.SetActive( true );
			attachedObjects[index].attachedObject.SendMessage( "OnDetachedFromHand", this, SendMessageOptions.DontRequireReceiver );
			attachedObjects.RemoveAt( index );

			GameObject newTopObject = currentAttachedObject;

			//Give focus to the top most object on the stack if it changed
			if ( newTopObject != null && newTopObject != prevTopObject )
			{
				newTopObject.SetActive( true );
				newTopObject.SendMessage( "OnHandFocusAcquired", this, SendMessageOptions.DontRequireReceiver );
			}
		}

		CleanUpAttachedObjectStack();
	}

    /// <summary>
    /// Get the world velocity of the VR Hand.
    /// Note: controller velocity value only updates on controller events (Button but and down) so good for throwing
    /// </summary>
    public Vector3 GetTrackedObjectVelocity()
    {
        SteamVR_TrackedObject trackedObj = gameObject.GetComponent<SteamVR_TrackedObject>();
        if (trackedObj)
        {
            var origin = trackedObj.origin ? trackedObj.origin : trackedObj.transform.parent;
            if (origin != null)
            {
                return origin.TransformVector(controller.velocity);
            }
            return controller.velocity;
        }
        return controller.velocity;
    }

    /// <summary>
    /// Get the world angular velocity of the VR Hand.
    /// Note: controller velocity value only updates on controller events (Button but and down) so good for throwing
    /// </summary>
    public Vector3 GetTrackedObjectAngularVelocity()
    {
        SteamVR_TrackedObject trackedObj = gameObject.GetComponent<SteamVR_TrackedObject>();
        if (trackedObj)
        {
            var origin = trackedObj.origin ? trackedObj.origin : trackedObj.transform.parent;
            if (origin != null)
            {
                return origin.TransformVector(controller.angularVelocity);
            }
            return controller.angularVelocity;
        }
        return controller.angularVelocity;
    }

	private void CleanUpAttachedObjectStack()
	{
		attachedObjects.RemoveAll( l => l.attachedObject == null );
	}

	void Awake()
	{
		if ( hoverSphereTransform == null )
		{
			hoverSphereTransform = this.transform;
		}

		//
		// Find render model and deactivate until we know which controller we're tracking
		//
		renderModel = GetComponentInChildren<SteamVR_RenderModel>();
		if ( ( renderModel != null ) && ( renderModel.gameObject != gameObject ) )
		{
			renderModel.gameObject.SetActive( false );
		}
	}

	IEnumerator Start()
	{
		// We are a "no SteamVR fallback hand" if we have this camera set
		// we'll use the right mouse to look around and left mouse to interact
		// - don't need to find the device
		if ( noSteamVRFallbackCamera )
			yield break;

		//Debug.Log( "VRHand - initializing connection routine" );

		// Acquire the correct device index for the hand we want to be
		// Also for the other hand if we get there first
		while ( true )
		{
			// Don't need to run this every frame
			yield return new WaitForSeconds( 1.0f );

			// We have a controller now, break out of the loop!
			if ( controller != null )
				break;
			
			//Debug.Log( "VRHand - checking controllers..." );

			// Initialize both hands simultaneously
			if ( startingHandType == HandType.Left || startingHandType == HandType.Right )
			{
				// Left/right relationship.
				// Wait until we have a clear unique left-right relationship to initialize.
				int leftIndex = SteamVR_Controller.GetDeviceIndex( SteamVR_Controller.DeviceRelation.Leftmost );
				int rightIndex = SteamVR_Controller.GetDeviceIndex( SteamVR_Controller.DeviceRelation.Rightmost );
				if ( leftIndex == -1 || rightIndex == -1 || leftIndex == rightIndex )
				{
					//Debug.Log( string.Format( "...Left/right hand relationship not yet established: leftIndex={0}, rightIndex={1}", leftIndex, rightIndex ) );
					continue;
				}

				int myIndex = ( startingHandType == HandType.Right ) ? rightIndex : leftIndex;
				int otherIndex = ( startingHandType == HandType.Right ) ? leftIndex : rightIndex;

				InitController( myIndex );
				if ( otherHand )
				{
					otherHand.InitController( otherIndex );
				}
			}
			else
			{
				// No left/right relationship. Just wait for a connection

				var vr = SteamVR.instance;
				for ( int i = 0; i < Valve.VR.OpenVR.k_unMaxTrackedDeviceCount; i++ )
				{
					if ( vr.hmd.GetTrackedDeviceClass( (uint)i ) != Valve.VR.ETrackedDeviceClass.Controller )
					{
						//Debug.Log( string.Format( "VRHand - device {0} is not a controller", i ) );
						continue;
					}
						
					var device = SteamVR_Controller.Input( i );
					if ( !device.valid )
					{
						//Debug.Log( string.Format( "VRHand - device {0} is not valid", i ) );
						continue;
					}

					if ( ( otherHand != null ) && ( otherHand.controller != null ) )
					{
						// Other hand is using this index, so we cannot use it.
						if ( i == (int)otherHand.controller.index )
						{
							//Debug.Log( string.Format( "VRHand - device {0} is owned by the other hand", i ) );
							continue;
						}
					}

					InitController( i );
				}
			}
		}
	}

	void UpdateHovering()
	{
		if ( ( noSteamVRFallbackCamera == null ) && ( controller == null ) )
		{
			return;
		}

		if ( hoverLocked )
			return;

		float closestDistance = float.MaxValue;
		VRInteractable closestInteractable = null;

		// Pick the closest hovering
		Collider[] overlappingColliders = Physics.OverlapSphere( hoverSphereTransform.position, hoverSphereRadius, hoverLayerMask.value );
		if ( overlappingColliders.Length > 0 && overlappingColliders.Length != prevOverlappingColliders )
		{
			prevOverlappingColliders = overlappingColliders.Length;
			VRHandDebugLog( "Found " + overlappingColliders.Length + " overlapping colliders." );
		}

		foreach ( Collider collider in overlappingColliders )
		{
			VRInteractable contacting = collider.GetComponentInParent<VRInteractable>();

			// Yeah, it's null, skip
			if ( contacting == null )
				continue;

			// Ignore this collider for hovering
			VRIgnoreHovering ignore = collider.GetComponent<VRIgnoreHovering>();
			if ( ignore != null )
			{
				if ( ignore.onlyIgnoreHand == null || ignore.onlyIgnoreHand == this )
				{
					continue;
				}
			}

			// Can't hover over the object if it's attached
			if ( attachedObjects.FindIndex( l => l.attachedObject == contacting.gameObject ) != -1 )
				continue;

			// Occupied by another hand, so we can't touch it
			if ( otherHand && otherHand.hoveringInteractable == contacting )
				continue;

			// Best candidate so far...
			float distance = Vector3.Distance( contacting.transform.position, hoverSphereTransform.position );
			if ( distance < closestDistance )
			{
				closestDistance = distance;
				closestInteractable = contacting;
			}
		}

		// Hover on this one
		hoveringInteractable = closestInteractable;
	}

	void UpdateNoSteamVRFallback()
	{
		if ( noSteamVRFallbackCamera )
		{
			Ray ray = noSteamVRFallbackCamera.ScreenPointToRay( Input.mousePosition );

			if ( GetStandardInteractionButton() )
			{
				// Holding down the mouse:
				// move around a fixed distance from the camera
				transform.position = ray.origin + noSteamVRFallbackInteractorDistance * ray.direction;
			}
			else
			{
				// Not holding down the mouse:
				// cast out a ray to see what we should mouse over

				// Don't want to hit the hand and anything underneath it
				// So move it back behind the camera when we do the raycast
				Vector3 oldPosition = transform.position;
				transform.position = noSteamVRFallbackCamera.transform.forward * ( -1000.0f );

				RaycastHit raycastHit;
				if ( Physics.Raycast( ray, out raycastHit, 4.0f ) )
				{
					transform.position = raycastHit.point;

					// Remember this distance in case we click and drag the mouse
					noSteamVRFallbackInteractorDistance = raycastHit.distance;
				}
				else if ( noSteamVRFallbackInteractorDistance > 0.0f )
				{
					// Move it around at the distance we last had a hit
					transform.position = ray.origin + noSteamVRFallbackInteractorDistance * ray.direction;
				}
				else
				{
					// Didn't hit, just leave it where it was
					transform.position = oldPosition;
				}
			}
		}
	}

	void UpdateDebugText()
	{
		if ( showDebugText )
		{
			if ( debugText == null )
			{
				debugText = new GameObject( "_debug_text" ).AddComponent<TextMesh>();
				debugText.fontSize = 120;
				debugText.characterSize = 0.001f;
				debugText.transform.parent = transform;

				debugText.transform.localRotation = Quaternion.Euler( 90.0f, 0.0f, 0.0f );
			}

			if ( GuessCurrentHandType() == HandType.Right )
			{
				debugText.transform.localPosition = new Vector3( -0.05f, 0.0f, 0.0f );
				debugText.alignment = TextAlignment.Right;
				debugText.anchor = TextAnchor.UpperRight;
			}
			else
			{
				debugText.transform.localPosition = new Vector3( 0.05f, 0.0f, 0.0f );
				debugText.alignment = TextAlignment.Left;
				debugText.anchor = TextAnchor.UpperLeft;
			}

			debugText.text = string.Format(
				"Hovering: {0}\n" +
				"Hover Lock: {1}\n" +
				"Attached: {2}\n" +
				"Total Attached: {3}\n" +
				"Type: {4}\n",
				( hoveringInteractable ? hoveringInteractable.gameObject.name : "null" ),
				hoverLocked,
				( currentAttachedObject ? currentAttachedObject.name : "null" ),
				attachedObjects.Count,
				GuessCurrentHandType().ToString() );
		}
		else
		{
			if ( debugText != null )
			{
				Destroy( debugText.gameObject );
			}
		}
	}

	void OnEnable()
	{
		// Stagger updates between hands
		float hoverUpdateBegin = ( ( otherHand != null ) && ( otherHand.GetInstanceID() < GetInstanceID() ) ) ? ( 0.5f * hoverUpdateInterval ) : ( 0.0f );
		InvokeRepeating( "UpdateHovering", hoverUpdateBegin, hoverUpdateInterval );
		InvokeRepeating( "UpdateDebugText", hoverUpdateBegin, hoverUpdateInterval );
	}

	void OnDisable()
	{
		CancelInvoke();
	}

	void Update()
	{
		UpdateNoSteamVRFallback();

		GameObject attached = currentAttachedObject;
		if ( attached )
		{
			attached.SendMessage( "HandAttachedUpdate", this, SendMessageOptions.DontRequireReceiver );
		}

		if ( ( renderModel != null ) && ( renderModel.gameObject != gameObject ) )
		{
			if ( showRenderModel == false )
			{
				renderModel.gameObject.SetActive( false );
			}
			else
			{
				renderModel.gameObject.SetActive( ( attached ) ? false : true );
			}
		}

		if ( hoveringInteractable )
		{
			hoveringInteractable.SendMessage( "HandHoverUpdate", this, SendMessageOptions.DontRequireReceiver );
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.color = new Color( 0.5f, 1.0f, 0.5f, 0.9f );
		Transform sphereTransform = hoverSphereTransform ? hoverSphereTransform : this.transform;
		Gizmos.DrawWireSphere( sphereTransform.position, hoverSphereRadius );
	}


	private void VRHandDebugLog( string msg )
	{
		if ( spewDebugText || !Application.isEditor )
		{
			Debug.Log( "VRHand: " + msg );
		}
	}

	/// <summary>
	/// Continue to hover over this object indefinitely, whether or not the VRHand moves out of its interaction trigger volume.
	/// </summary>
	/// <param name="interactable">The VRInteractable to hover over indefinitely.</param>
	/// <seealso cref="HoverUnlock"/>
	public void HoverLock( VRInteractable interactable )
	{
		VRHandDebugLog( "HoverLock " + interactable );
		hoverLocked = true;
		hoveringInteractable = interactable;
	}

	/// <summary>
	/// Stop hovering over this object indefinitely.
	/// </summary>
	/// <param name="interactable">The hover-locked VRInteractable to stop hovering over indefinitely.</param>
	/// <seealso cref="HoverLock"/>
	public void HoverUnlock( VRInteractable interactable )
	{
		VRHandDebugLog( "HoverUnlock " + interactable );
		if ( hoveringInteractable == interactable )
		{
			hoverLocked = false;
		}
	}

	/// <summary>
	/// Was the standard interaction button just pressed? In VR, this is a trigger press. In 2D fallback, this is a mouse left-click.
	/// </summary>
	public bool GetStandardInteractionButtonDown()
	{
		if ( noSteamVRFallbackCamera )
		{
			return Input.GetMouseButtonDown( 0 );
		}
		else if ( controller != null )
		{
			return controller.GetTouchDown( Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger );
		}

		return false;
	}

	/// <summary>
	/// Was the standard interaction button just released? In VR, this is a trigger press. In 2D fallback, this is a mouse left-click.
	/// </summary>
	public bool GetStandardInteractionButtonUp()
	{
		if ( noSteamVRFallbackCamera )
		{
			return Input.GetMouseButtonUp( 0 );
		}
		else if ( controller != null )
		{
			return controller.GetTouchUp( Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger );
		}

		return false;
	}

	/// <summary>
	/// Is the standard interaction button being pressed? In VR, this is a trigger press. In 2D fallback, this is a mouse left-click.
	/// </summary>
	public bool GetStandardInteractionButton()
	{
		if ( noSteamVRFallbackCamera )
		{
			return Input.GetMouseButton( 0 );
		}
		else if ( controller != null )
		{
			return controller.GetTouch( Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger );
		}

		return false;
	}

	private void InitController( int index )
	{
		if ( controller == null )
		{
			controller = SteamVR_Controller.Input( index );
			gameObject.AddComponent<SteamVR_TrackedObject>().index = (SteamVR_TrackedObject.EIndex)index;

			//
			// Update and show SteamVR_RenderModel
			//
			if ( createRenderModel )
			{
				renderModel = new GameObject( "SteamVR_RenderModel" ).AddComponent<SteamVR_RenderModel>();
				renderModel.transform.parent = transform;
				renderModel.transform.localPosition = Vector3.zero;
				renderModel.transform.localRotation = Quaternion.identity;
				renderModel.transform.localScale = Vector3.one;
			}
			else
			{
				renderModel = GetComponentInChildren<SteamVR_RenderModel>();
			}

			if ( renderModel != null )
			{
				renderModel.modelOverride = null;
				renderModel.index = (SteamVR_TrackedObject.EIndex)index;
				renderModel.enabled = true;
				renderModel.UpdateModel();
			}

			this.BroadcastMessage( "OnHandInitialized", index, SendMessageOptions.DontRequireReceiver ); // let child objects know we've initialized
		}
	}
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor( typeof( VRHand ) )]
public class VRHandEditor : UnityEditor.Editor
{
	// Custom Inspector GUI allows us to click from within the UI
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		VRHand vrHand = (VRHand)target;

		if ( vrHand.otherHand )
		{
			if ( vrHand.otherHand.otherHand != vrHand )
			{
				UnityEditor.EditorGUILayout.HelpBox( "The otherHand of this VRHand's otherHand is not this VRHand.", UnityEditor.MessageType.Warning );
			}

			if ( vrHand.startingHandType == VRHand.HandType.Left && vrHand.otherHand.startingHandType != VRHand.HandType.Right )
			{
				UnityEditor.EditorGUILayout.HelpBox( "This is a left VRHand but otherHand is not a right VRHand.", UnityEditor.MessageType.Warning );
			}

			if ( vrHand.startingHandType == VRHand.HandType.Right && vrHand.otherHand.startingHandType != VRHand.HandType.Left )
			{
				UnityEditor.EditorGUILayout.HelpBox( "This is a right VRHand but otherHand is not a left VRHand.", UnityEditor.MessageType.Warning );
			}

			if ( vrHand.startingHandType == VRHand.HandType.Any && vrHand.otherHand.startingHandType != VRHand.HandType.Any )
			{
				UnityEditor.EditorGUILayout.HelpBox( "This is an any-handed VRHand but otherHand is not an any-handed VRHand.", UnityEditor.MessageType.Warning );
			}
		}
	}
}
#endif
