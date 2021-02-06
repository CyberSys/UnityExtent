using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public class GameController : PersistableObject
{
    public KeyCode createKey = KeyCode.C;
    public KeyCode saveKey = KeyCode.S;
    public KeyCode loadKey = KeyCode.L;
    public KeyCode clearKey = KeyCode.Escape;
    
    public ObjectFactory objectFactory;
    private List<PersistableObject> objects;

    public PersistentStorage storage;

    private string savePath;
    void Awake()
    {
        objects = new List<PersistableObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(createKey)) {
            CreateObject();
        }
        else if (Input.GetKeyDown(saveKey)) {
            storage.Save(this);
        }
        else if (Input.GetKeyDown(loadKey)) {
            BeginNewGame();
            storage.Load(this);
        }
        else if (Input.GetKeyDown(clearKey))
        {
            BeginNewGame();
        }
    }
    
    void BeginNewGame () {
        for (int i = 0; i < objects.Count; i++) {
            Destroy(objects[i].gameObject);
        }
        objects.Clear();
    }
    
    void CreateObject ()
    {
        PersistableObject o = objectFactory.GetRandom();
        Transform t = o.transform;
        t.localPosition = Random.insideUnitSphere * 5f;
        t.localRotation = Random.rotation;

        Cell newCell = (Cell) o;

        if (newCell)
        {
            newCell.SetBackBound(true);
            newCell.SetRightBound(true);
        }
        
        objects.Add(o);
    }
    
    public override void Save (GameDataWriter writer) {
        writer.Write(objects.Count);
        for (int i = 0; i < objects.Count; i++) {
            Cell cellObject = objects[i] as Cell;

            if (cellObject != null)
            {
                cellObject.Save(writer);
            }
            else
            {
                objects[i].Save(writer);
            }
        }
    }
    
    public override void Load (GameDataReader reader) {
        int count = reader.ReadInt();
        for (int i = 0; i < count; i++)
        {
            int objectId = reader.ReadInt();
            PersistableObject o = objectFactory.Get(objectId);
            
            Cell cellObject = o as Cell;

            if (cellObject != null)
            {
                cellObject.Load(reader);
                objects.Add(cellObject);
            }
            else
            {
                objects[i].Load(reader);
                objects.Add(o);
            }
            
        }
    }
}
