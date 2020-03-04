using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class MaterialSwitch : MonoBehaviour
{
    public DecalProjector decal;

    public Material decalIdle;
    public Material decalRight;
    public Material decalLeft;

    public void IdleMaterial()
    {
        //if(decal.material != decalIdle)
            decal.material = decalIdle;
    }

    public void RightMaterial()
    {
        //if (decal.material != decalRight)
            decal.material = decalRight;
    }

    public void LeftMaterial()
    {
        //if (decal.material != decalLeft)
            decal.material = decalLeft;
    }
}
