using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public Transform PlayerTransform;

    public class GridCell
    {
        public Vector3 Centre;

        public GridCell(Vector3 centrePosition)
        {
            Centre = centrePosition;
        }
    }

    private List<List<GridCell>> Grid;
    
    // Start is called before the first frame update
    void Start()
    {
        Grid = new List<List<GridCell>>();

        for (int i = 0; i < 10; i++)
        {
            List<GridCell> row = new List<GridCell>();
            for (int j = 0; j < 10; j++)
            {
                row.Add(new GridCell(new Vector3(i + 0.5f, 0.0f, j + 0.5f)));
            }
            
            Grid.Add(row);
        }

        int test = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
