//===================== Copyright (c) Valve Corporation. All Rights Reserved. ======================

using UnityEngine;
using System.Collections;

public class VRLinearBlendshape : MonoBehaviour
{
    public VRLinearMapping linearMapping;
    public SkinnedMeshRenderer skinnedMesh;

    private float lastValue;

    void Awake()
    {
        if ( skinnedMesh == null )
        {
            skinnedMesh = GetComponent<SkinnedMeshRenderer>();
        }

        if ( linearMapping == null )
        {
            linearMapping = GetComponent<VRLinearMapping>();
        }
    }

    void Update()
    {
        float value = linearMapping.value;

        //No need to set the blend if our value hasn't changed.
        if ( value != lastValue )
        {
            float blendValue = RemapNumberClamped( value, 0f, 1f, 1f, 100f );
            skinnedMesh.SetBlendShapeWeight( 0, blendValue );
        }

        lastValue = value;
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
