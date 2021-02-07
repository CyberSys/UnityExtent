using UnityEngine;

[CreateAssetMenu]
public class ObjectFactory : ScriptableObject
{
    [SerializeField] private PersistableObject[] prefabs;

    public PersistableObject Get(int objectId)
    {
        return prefabs[objectId];
    }
    
    public PersistableObject GetRandom () {
        return Get(Random.Range(0, prefabs.Length));
    }
}
