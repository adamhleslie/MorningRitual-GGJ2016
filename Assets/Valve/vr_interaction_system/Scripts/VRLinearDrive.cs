//===================== Copyright (c) Valve Corporation. All Rights Reserved. ======================

// INCLUDES DOOR HANDLE LOADING NEXT SCENE CODE

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent( typeof( VRInteractable ) )]
public class VRLinearDrive : MonoBehaviour
{
	public Transform startPosition;
	public Transform endPosition;
	public VRLinearMapping linearMapping;
	public bool repositionGameObject = true;
	public bool maintainMomemntum = true;
	public float momemtumDampenRate = 5.0f;

	private float initialMappingOffset;
	private int numMappingChangeSamples = 5;
	private float[] mappingChangeSamples;
	private float prevMapping = 0.0f;
	private float mappingChangeRate;
	private int sampleCount = 0;

	public bool doorHandle;

	// RUNS WHEN DOOR HANDLE PRESSED ALL THE WAY DOWN
	void LoadScene()
	{
		bool nextLevel = true;
		Debug.Log("COUNT: " + Globals.ritualSequence.Count);
		for(int i = 0; i < Globals.ritualSequence.Count && nextLevel; i++)
		{
			nextLevel = (bool) Globals.ritualSequence[i];
		}

		if(nextLevel) {
			Globals.curScene++;
			Debug.Log("NEXT SCENE");
		}

		Debug.Log("Loading: " + Globals.curScene);
		SceneManager.LoadScene(Globals.curScene);
	}

	void Awake()
	{
		mappingChangeSamples = new float[numMappingChangeSamples];
	}

	void Start()
	{
		if ( linearMapping == null )
		{
			linearMapping = GetComponent<VRLinearMapping>();
		}

		if ( linearMapping == null )
		{
			linearMapping = gameObject.AddComponent<VRLinearMapping>();
		}

		if ( repositionGameObject )
		{
			UpdateLinearMapping( transform );
		}
	}

	void HandHoverUpdate( VRHand hand )
	{
		if ( hand.GetStandardInteractionButtonDown() )
		{
			hand.HoverLock( GetComponent<VRInteractable>() );

			initialMappingOffset = linearMapping.value - CalculateLinearMapping( hand.transform );
			sampleCount = 0;
			mappingChangeRate = 0.0f;
		}

		if ( hand.GetStandardInteractionButtonUp() )
		{
			hand.HoverUnlock( GetComponent<VRInteractable>() );

			CalculateMappingChangeRate();
		}

		if ( hand.GetStandardInteractionButton() )
		{
			UpdateLinearMapping( hand.transform );
		}
	}

	void CalculateMappingChangeRate()
	{
		//Compute the mapping change rate
		mappingChangeRate = 0.0f;
		int mappingSamplesCount = Mathf.Min( sampleCount, mappingChangeSamples.Length );
		if ( mappingSamplesCount != 0 )
		{
			for ( int i = 0; i < mappingSamplesCount; ++i )
			{
				mappingChangeRate += mappingChangeSamples[i];
			}
			mappingChangeRate /= mappingSamplesCount;
		}
	}

	void UpdateLinearMapping( Transform tr )
	{
		prevMapping = linearMapping.value;
		linearMapping.value = Mathf.Clamp01( initialMappingOffset + CalculateLinearMapping( tr ) );

		mappingChangeSamples[sampleCount % mappingChangeSamples.Length] = ( 1.0f / Time.deltaTime ) * ( linearMapping.value - prevMapping );
		sampleCount++;

		if ( repositionGameObject )
		{

			if ( doorHandle && linearMapping.value == 1 )
			{
				LoadScene();
			}

			transform.position = Vector3.Lerp( startPosition.position, endPosition.position, linearMapping.value );
		}
	}

	float CalculateLinearMapping( Transform tr )
	{
		Vector3 direction = endPosition.position - startPosition.position;
		float length = direction.magnitude;
		direction.Normalize();

		Vector3 displacement = tr.position - startPosition.position;

		return Vector3.Dot( displacement, direction ) / length;
	}

	void Update()
	{
		if ( maintainMomemntum && mappingChangeRate != 0.0f )
		{
			//Dampen the mapping change rate and apply it to the mapping
			mappingChangeRate = Mathf.Lerp( mappingChangeRate, 0.0f, momemtumDampenRate * Time.deltaTime );
			linearMapping.value = Mathf.Clamp01( linearMapping.value + ( mappingChangeRate * Time.deltaTime ) );

			if ( repositionGameObject )
			{
				transform.position = Vector3.Lerp( startPosition.position, endPosition.position, linearMapping.value );
			}
		}
	}
}
