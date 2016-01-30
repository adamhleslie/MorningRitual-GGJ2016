//===================== Copyright (c) Valve Corporation. All Rights Reserved. ======================

using UnityEngine;
using UnityEngine.Events;

//Sends simple controller button events to UnityEvents
[RequireComponent( typeof( VRInteractable ) )]
public class VRInteractableButtonEvents : MonoBehaviour
{
	public UnityEvent onTriggerDown;
	public UnityEvent onTriggerUp;
	public UnityEvent onGripDown;
	public UnityEvent onGripUp;
	public UnityEvent onTouchpadDown;
	public UnityEvent onTouchpadUp;
	public UnityEvent onTouchpadTouch;
	public UnityEvent onTouchpadRelease;
	
	void Start ()
	{
	
	}

	void Update ()
	{
        for ( int i = 0; i < VRPlayer.instance.handCount; i++ )
        {
            VRHand hand = VRPlayer.instance.GetHand( i );

            if ( hand.controller != null )
            {
                if ( hand.controller.GetPressDown( Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger ) )
                {
                    onTriggerDown.Invoke();
                }

                if ( hand.controller.GetPressUp( Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger ) )
                {
                    onTriggerUp.Invoke();
                }

                if ( hand.controller.GetPressDown( Valve.VR.EVRButtonId.k_EButton_Grip ) )
                {
                    onGripDown.Invoke();
                }

                if ( hand.controller.GetPressUp( Valve.VR.EVRButtonId.k_EButton_Grip ) )
                {
                    onGripUp.Invoke();
                }

                if ( hand.controller.GetPressDown( Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad ) )
                {
                    onTouchpadDown.Invoke();
                }

                if ( hand.controller.GetPressUp( Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad ) )
                {
                    onTouchpadUp.Invoke();
                }

                if ( hand.controller.GetTouchDown( Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad ) )
                {
                    onTouchpadTouch.Invoke();
                }

                if ( hand.controller.GetTouchUp( Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad ) )
                {
                    onTouchpadRelease.Invoke();
                }
            }
        }

	}
}
