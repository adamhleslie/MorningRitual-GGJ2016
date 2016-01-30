//===================== Copyright (c) Valve Corporation. All Rights Reserved. ======================

using UnityEngine;

public class VRIgnoreHovering : MonoBehaviour
{
    [Tooltip( "If VRHand is not null, only ignore the specified hand" )]
    public VRHand onlyIgnoreHand = null;
}
