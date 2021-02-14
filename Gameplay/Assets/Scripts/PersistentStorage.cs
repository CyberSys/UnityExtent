using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class PersistentStorage : MonoBehaviour
{

    private List<MemoryStream> frameStreams;
    private MemoryStream memoryStream;
    private BinaryWriter binaryWriter;
    private BinaryReader binaryReader;

    string savePath;
    public ObjectFactory objectFactory;
    
    const int numberOfFrames = 600;
    const int sizeOfFrame = 28560;

    void Awake () {
        savePath = System.IO.Path.Combine(Application.persistentDataPath, "saveFile");
        frameStreams = new List<MemoryStream>(numberOfFrames);
    }

    public void Save (PersistableObject o) {
        using (
            var writer = new BinaryWriter(File.Open(savePath, FileMode.Create))
        ) {
            o.Save(new GameDataWriter(writer));
        }
    }

    public void SaveToMemory(PersistableObject o)
    {
        memoryStream = new MemoryStream();
        binaryWriter = new BinaryWriter(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        
        o.Save(new GameDataWriter(binaryWriter));
        
        frameStreams.Add(memoryStream);
        
        if (frameStreams.Count > numberOfFrames)
        {
            frameStreams.RemoveAt(frameStreams.Count-1);
        }
    }

    public void LoadFromMemory(PersistableObject o, float sliderValue)
    {
        int frameIndex = (int) Mathf.Clamp(sliderValue * frameStreams.Count, 0, frameStreams.Count-1);
        MemoryStream currentStream = frameStreams.ElementAt(frameIndex);
        currentStream.Seek(0, SeekOrigin.Begin);
        binaryReader = new BinaryReader(currentStream);
        
    
        o.Set(new GameDataReader(binaryReader));
    }

    public void Load (PersistableObject o) {
        using (
            var reader = new BinaryReader(File.Open(savePath, FileMode.Open))
        ) {
            o.Load(new GameDataReader(reader));
        }
    }
}