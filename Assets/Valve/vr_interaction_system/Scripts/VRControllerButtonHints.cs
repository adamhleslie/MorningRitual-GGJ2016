using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VRControllerButtonHints : MonoBehaviour
{
	public Material controllerMaterial;
	public Color flashColor = new Color( 1.0f, 0.557f, 0.0f );

	private SteamVR_RenderModel renderModel;

	private List<MeshRenderer> renderers = new List<MeshRenderer>();
	private List<MeshRenderer> flashingRenderers = new List<MeshRenderer>();
	private List<MeshRenderer> temp = new List<MeshRenderer>();
	private float startTime;
	private float tickCount;

	private int colorID;

	void Awake()
	{
		colorID = Shader.PropertyToID( "_Color" );
	}

	void OnHandInitialized( int deviceIndex )
	{
		renderModel = new GameObject( "SteamVR_RenderModel" ).AddComponent<SteamVR_RenderModel>();
		renderModel.transform.parent = transform;
		renderModel.transform.localPosition = Vector3.zero;
		renderModel.transform.localRotation = Quaternion.identity;
		renderModel.SetDeviceIndex( deviceIndex );
		renderModel.gameObject.SetActive( false );
	}

	public void Show( params Valve.VR.EVRButtonId[] buttons )
	{
		Clear();

		renderModel.gameObject.SetActive( true );

		renderModel.GetComponentsInChildren<MeshRenderer>( renderers );
		for ( int i = 0; i < renderers.Count; i++ )
		{
			Texture mainTexture = renderers[i].material.mainTexture;
			renderers[i].sharedMaterial = controllerMaterial;
			renderers[i].material.mainTexture = mainTexture;

			// This is to poke unity into setting the correct render queue for the model
			renderers[i].material.renderQueue = controllerMaterial.shader.renderQueue;
		}

		for ( int i = 0; i < buttons.Length; i++ )
		{
			switch ( buttons[i] )
			{
			case Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger:
				FlashRenderers( "trigger" );
				break;
			case Valve.VR.EVRButtonId.k_EButton_ApplicationMenu:
				FlashRenderers( "button" );
				break;
			case Valve.VR.EVRButtonId.k_EButton_System:
				FlashRenderers( "sys_button" );
				break;
			case Valve.VR.EVRButtonId.k_EButton_Grip:
				FlashRenderers( "lgrip" );
				FlashRenderers( "rgrip" );
				break;
			case Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad:
				FlashRenderers( "trackpad" );
				break;
			}
		}

		startTime = Time.realtimeSinceStartup;
		tickCount = 0.0f;
	}

	public void Hide()
	{
		Clear();

		renderModel.gameObject.SetActive( false );
	}

	IEnumerator Test()
	{
		while ( true )
		{
			Show( Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger );
			yield return new WaitForSeconds( 1.0f );
			Show( Valve.VR.EVRButtonId.k_EButton_ApplicationMenu );
			yield return new WaitForSeconds( 1.0f );
			Show( Valve.VR.EVRButtonId.k_EButton_System );
			yield return new WaitForSeconds( 1.0f );
			Show( Valve.VR.EVRButtonId.k_EButton_Grip );
			yield return new WaitForSeconds( 1.0f );
			Show( Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad );
			yield return new WaitForSeconds( 1.0f );
		}
	}

	void Update()
	{
		if ( renderModel != null && renderModel.gameObject.activeInHierarchy )
		{
			Color baseColor = controllerMaterial.GetColor( colorID );

			float flash = ( Time.realtimeSinceStartup - startTime ) * Mathf.PI * 2.0f;
			flash = Mathf.Cos( flash );
			flash = RemapNumberClamped( flash, -1.0f, 1.0f, 0.0f, 1.0f );

			float ticks = ( Time.realtimeSinceStartup - startTime );
			if ( ticks - tickCount > 1.0f )
			{
				tickCount += 1.0f;
				SteamVR_Controller.Device device = SteamVR_Controller.Input( ( int )renderModel.index );
				if ( device != null )
				{
					device.TriggerHapticPulse();
				}
			}

			for ( int i = 0; i < flashingRenderers.Count; i++ )
			{
				Renderer r = flashingRenderers[i];
				r.material.SetColor( colorID, Color.Lerp( baseColor, flashColor, flash ) );
			}
		}
	}

	void OnDisable()
	{
		Clear();
	}

	void FlashRenderers( string componentName )
	{
		Transform component = renderModel.FindComponent( componentName );
		if ( component != null )
		{
			component.GetComponentsInChildren<MeshRenderer>( temp );
			flashingRenderers.AddRange( temp );
			temp.Clear();
		}
	}

	void Clear()
	{
		renderers.Clear();
		flashingRenderers.Clear();
	}

	public static void Show( VRHand hand, params Valve.VR.EVRButtonId[] buttons )
	{
		if ( hand != null )
		{
			VRControllerButtonHints hints = hand.GetComponentInChildren<VRControllerButtonHints>();
			if ( hints != null )
			{
				hints.Show( buttons );
			}
		}
	}

	public static void Hide( VRHand hand )
	{
		if ( hand != null )
		{
			VRControllerButtonHints hints = hand.GetComponentInChildren<VRControllerButtonHints>();
			if ( hints != null )
			{
				hints.Hide();
			}
		}
	}

	private float RemapNumber( float num, float low1, float high1, float low2, float high2 )
	{
		return low2 + ( num - low1 ) * ( high2 - low2 ) / ( high1 - low1 );
	}

	private float RemapNumberClamped( float num, float low1, float high1, float low2, float high2 )
	{
		return Mathf.Clamp( RemapNumber( num, low1, high1, low2, high2 ), Mathf.Min( low2, high2 ), Mathf.Max( low2, high2 ) );
	}
}
