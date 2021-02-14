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
        ObjectId = reader.ReadInt();
        transform.localPosition = reader.ReadVector3();
        transform.localRotation = reader.ReadQuaternion();
        transform.localScale = reader.ReadVector3();
    }

    public virtual void Set(GameDataReader reader)
    {
        Load(reader);
    }

    public static int SizeOf()
    {
        // obectId + Vector3 position + quaternion rotation + Vector3 scale
        return sizeof(int) + (sizeof(float) * 3) + (sizeof(float) * 4) + (sizeof(float) * 3);
    }
}
