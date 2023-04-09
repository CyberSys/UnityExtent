using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameController : PersistableObject
{
    public KeyCode createKey = KeyCode.C;
    public KeyCode saveKey = KeyCode.S;
    public KeyCode loadKey = KeyCode.L;
    public KeyCode pauseKey = KeyCode.Escape;
    public KeyCode rewindKey = KeyCode.Backspace;

    struct FrameData
    {
        public DateTime frameTime;
        public GridController grid;
    }

    private Stack<FrameData> rewindData = new Stack<FrameData>();
    private bool rewinding = false;

    private bool playerRewind = false;

    private GameObject rewindSlider;
    private GameObject rewindBackground;

    

    private bool playerPaused = false;

    private GameObject pausedText;
    private GameObject pausedBackground;

    private int activeGridCount = 0;
    
    private List<GridController> grids;

    public PersistentStorage storage;

    private string savePath;
    void Awake()
    {
        grids = new List<GridController>();
        
        pausedText = GameObject.Find("Paused");
        pausedBackground = GameObject.Find("Paused Background");
        
        rewindSlider = GameObject.Find("Rewind Slider");
        rewindBackground = GameObject.Find("Rewind Background");
        rewindSlider.SetActive(false);
    }

    static TimeSpan GetDeltaTime(DateTime a, DateTime b)
    {
        return a.Subtract(b);
    }

    // Update is called once per frame
    void Update()
    {
        // if (Time.timeScale > 0 & grids.Count > 0)
        // {
        //     // check whether to add new frame
        //     if (rewindData.Count < 1 ||
        //         GetDeltaTime(DateTime.Now, rewindData.ElementAt(0).frameTime).Milliseconds >= 30)
        //     {
        //         FrameData newFrame = new FrameData();
        //         newFrame.frameTime = DateTime.Now;
        //         newFrame.grid = grids[0];
        //         rewindData.Push(newFrame);
        //     }
        // }
        
        if (Input.GetKeyDown(createKey)) {
            CreateGrid();
        }
        // else if (Input.GetKeyDown(saveKey)) {
        //     storage.Save(this);
        // }
        // else if (Input.GetKeyDown(loadKey)) {
        //     BeginNewGame();
        //     storage.Load(this);
        // }
        // else if (Input.GetKeyDown(pauseKey))
        // {
        //     if (Time.timeScale > 0.0f)
        //     {
        //         playerPaused = true;
        //     }
        //     else
        //     {
        //         playerPaused = false;
        //     }
        // }
        // else if (Input.GetKeyDown(rewindKey) && grids.Count > 0)
        // {
        //     rewinding = !rewinding;
        // }
        //
        // if (playerPaused)
        // {
        //     Time.timeScale = 0.0f;
        //     pausedText.GetComponent<Text>().enabled = true;
        //     pausedBackground.GetComponent<Image>().enabled = true;
        // }
        // else
        // {
        //     Time.timeScale = 1.0f;
        //     pausedText.GetComponent<Text>().enabled = false;
        //     pausedBackground.GetComponent<Image>().enabled = false;
        // }
        
        // if (rewinding)
        // {
        //     rewindSlider.SetActive(true);
        //     rewindBackground.GetComponent<Image>().enabled = true;
        //     if (rewindSlider.activeSelf)
        //     {
        //         float sliderValue = rewindSlider.GetComponent<Slider>().value;
        //         storage.LoadFromMemory(this, sliderValue);
        //     }
        // }
        // else
        // {
        //     if(grids.Count > 0)
        //         storage.SaveToMemory(this);
        //     
        //     rewindSlider.SetActive(false);
        //     rewindBackground.GetComponent<Image>().enabled = false;
        // }
    }
    
    void BeginNewGame () {
        for (int i = 0; i < grids.Count; i++) {
            Destroy(grids[i].gameObject);
        }
        grids.Clear();
    }

    public void StartGame()
    {
        CreateGrid();
    }

    void CreateGrid ()
    {
        GridController newGrid = Instantiate(storage.objectFactory.Get(1) as GridController);
        Transform t = newGrid.transform;
        t.localPosition = Random.insideUnitSphere * 100f;

        Cell cellPrefab = storage.objectFactory.Get(2) as Cell;
        newGrid.SetObjectFactory(storage.objectFactory);
        newGrid.GenerateGrid(cellPrefab);
        
        grids.Add(newGrid);

        activeGridCount = grids.Count;
    }
    
    public override void Save (GameDataWriter writer) {
        base.Save(writer);
        
        writer.Write(activeGridCount);
        
        for (int i = 0; i < grids.Count; i++) {
            grids[i].Save(writer);
        }
        
        long test = writer.Size();
    }
    
    public override void Load (GameDataReader reader) {
        base.Load(reader);
        activeGridCount = reader.ReadInt();
        for (int i = 0; i < activeGridCount; i++)
        {
            GridController newGrid = Instantiate(storage.objectFactory.Get(1) as GridController);
            newGrid.SetObjectFactory(storage.objectFactory);
            grids.Add(newGrid);
            grids[i].Load(reader);
        }
    }
    
    public override void Set (GameDataReader reader) {
        base.Load(reader);
        activeGridCount = reader.ReadInt();
        for (int i = 0; i < activeGridCount; i++)
        {
            grids[i].Set(reader);
        }
    }
    
    public static int SizeOf(List<GridController> grids)
    {
        var value = PersistableObject.SizeOf();

        value += sizeof(int);
        
        for (int i = 0; i < grids.Count; i++) {
            value += GridController.SizeOf(grids[i].rowNumber, grids[i].columnNumber);
        }

        return value;
    }
}
