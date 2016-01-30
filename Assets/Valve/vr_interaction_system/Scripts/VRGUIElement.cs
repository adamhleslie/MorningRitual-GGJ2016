//===================== Copyright (c) Valve Corporation. All Rights Reserved. ======================

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

[RequireComponent( typeof( VRInteractable ) )]
public class VRGUIElement : MonoBehaviour
{
	//public CustomEvents.UnityEventVRHand onHandClick;

	private VRHand currentHand;

	void Awake()
	{
		Button button = GetComponent<Button>();
		if ( button )
		{
			button.onClick.AddListener( OnButtonClick );
		}
	}

	void OnHandHoverBegin( VRHand hand )
	{
		currentHand = hand;
		VRInputModule.instance.HoverBegin( gameObject );
		VRControllerButtonHints.Show( hand, Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger );
	}

	void OnHandHoverEnd( VRHand hand )
	{
		VRInputModule.instance.HoverEnd( gameObject );
		VRControllerButtonHints.Hide( hand );
		currentHand = null;
	}

	void HandHoverUpdate( VRHand hand )
	{
		if ( hand.GetStandardInteractionButtonDown() )
		{
			VRInputModule.instance.Submit( gameObject );
			VRControllerButtonHints.Hide( hand );
		}
	}

	void OnButtonClick()
	{
		//onHandClick.Invoke( currentHand );
	}
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor( typeof( VRGUIElement ) )]
public class VRGUIElementEditor : UnityEditor.Editor
{
	// Custom Inspector GUI allows us to click from within the UI
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		VRGUIElement vrGuiElement = ( VRGUIElement )target;
		if ( GUILayout.Button( "Click" ) )
		{
			VRInputModule.instance.Submit( vrGuiElement.gameObject );
		}
	}
}
#endif
