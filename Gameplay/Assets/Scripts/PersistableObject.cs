using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PersistableObject : MonoBehaviour
{
    public int ObjectId
    {
        get => objectId;
        set => objectId = value;
    }

    private int objectId;

    public virtual void Save (GameDataWriter writer) {
        writer.Write(ObjectId);
        writer.Write(transform.localPosition);
        writer.Write(transform.localRotation);
        writer.Write(transform.localScale);
    }

    public virtual void Load (GameDataReader reader)
    {
        // ObjectId = reader.ReadInt();
        transform.localPosition = reader.ReadVector3();
        transform.localRotation = reader.ReadQuaternion();
        transform.localScale = reader.ReadVector3();
    }
}
