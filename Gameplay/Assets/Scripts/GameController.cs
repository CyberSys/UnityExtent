using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;

public class GameController : PersistableObject
{
    public KeyCode createKey = KeyCode.C;
    public KeyCode saveKey = KeyCode.S;
    public KeyCode loadKey = KeyCode.L;
    public KeyCode clearKey = KeyCode.Escape;
    
    
    private List<GridController> grids;

    public PersistentStorage storage;

    private string savePath;
    void Awake()
    {
        grids = new List<GridController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(createKey)) {
            CreateGrid();
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
        for (int i = 0; i < grids.Count; i++) {
            Destroy(grids[i].gameObject);
        }
        grids.Clear();
    }
    
    void CreateGrid ()
    {
        GridController newGrid = Instantiate(storage.objectFactory.Get(0) as GridController);
        Transform t = newGrid.transform;
        t.localPosition = Random.insideUnitSphere * 100f;

        Cell cellPrefab = storage.objectFactory.Get(1) as Cell;
        newGrid.SetObjectFactory(storage.objectFactory);
        newGrid.GenerateGrid(cellPrefab);
        
        grids.Add(newGrid);
    }
    
    public override void Save (GameDataWriter writer) {
        writer.Write(grids.Count);
        for (int i = 0; i < grids.Count; i++) {
            grids[i].Save(writer);
        }
    }
    
    public override void Load (GameDataReader reader) {
        int count = reader.ReadInt();
        for (int i = 0; i < count; i++)
        {
            GridController newGrid = Instantiate(storage.objectFactory.Get(0) as GridController);
            newGrid.SetObjectFactory(storage.objectFactory);
            grids.Add(newGrid);
            grids[i].Load(reader);
        }
    }
}
