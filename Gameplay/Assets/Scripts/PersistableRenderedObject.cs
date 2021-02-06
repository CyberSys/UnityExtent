using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistableRenderedObject : PersistableObject
{
    MeshRenderer meshRenderer;
    MaterialPropertyBlock materialPropertyBlock;

    void Awake () {
        meshRenderer = GetComponent<MeshRenderer>();
        materialPropertyBlock = new MaterialPropertyBlock();
        meshRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    public void SetBaseColour(Color colour)
    {
        materialPropertyBlock.SetColor("_BaseColor", colour);
    }

    public void SetEmissionColour(Color colour)
    {
        materialPropertyBlock.SetColor("_EmissionColor", colour);
    }
}
