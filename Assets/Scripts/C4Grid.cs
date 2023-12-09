using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class C4Grid : MonoBehaviour
{
    #region Variables

    [Header("Grid Properties")]
    public static int Width = 7;
    public static int Height = 6;
    private float startX;
    private float startY;
    private int[,] cellInfo; // 0 for no player, 1 for p1, 2 for p2
    [SerializeField] private GameObject cellPrefab; // gameobject of a single cell
    
    //public getters
    public float StartX { get { return startX; } private set { startX = value; } }
    public float StartY { get { return startY; } private set { startY = value; } }
    public int[,] CellInfo { get { return cellInfo; } private set { cellInfo = value; } }
    
    [Header("Object Pooling")]
    private Queue<GameObject> pooledCells = new Queue<GameObject>();
    private Queue<GameObject> activeCells = new Queue<GameObject>();
    private int amountToPool = 42;
    
    //Scripts
    [SerializeField]private TokenGen tokenGenScript;

    #endregion

    private void Awake()
    {
        for (int i = 0; i < amountToPool; i++)
        {
            GameObject cellClone = Instantiate(cellPrefab, Vector3.zero, Quaternion.identity, transform);
            cellClone.SetActive(false);
            pooledCells.Enqueue(cellClone);
        }
    }

    //--------------------------------------GRID BEHAVIOUR-----------------------------------
    
    #region Grid Behaviour

    /// <summary>
    /// Creates the grid and starting board of the game
    /// </summary>
    public void CreateGrid()
    {
        if (Width == 0)
            Width = 7;
        if (Height == 0)
            Height = 6;
        
        cellInfo = new int[Width, Height];
        
        startX = -Width/2f + 0.5f;
        startY = Mathf.Round(-Height/2f);

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                cellInfo[x, y] = 0;
                
                //get a cell object
                ReturnPooledObject().transform.position = new Vector3(startX + x, startY + y);
            }
        }
        
        //make sure camera sees the whole grid
        UIManager.instance.SetUpCamera(Width,Height);
    }

    /// <summary>
    /// Updates the grid information by making the available lowest cell occupied by whichever player placed the token in the x pos
    /// </summary>
    /// <param name="xPos"> In which xPos should the token go</param>
    public void UpdateGridInfo(int xPos)
    {
        for (int yPos = 0; yPos < Height; yPos++)
        {
            if (cellInfo[xPos, yPos] == 0)
            {
                GameObject newToken = tokenGenScript.ActivateToken(xPos, yPos);
                cellInfo[xPos, yPos] = GameManager.instance.PlayerID;
                GameManager.instance.GridTokenObjects.Add((xPos, yPos), newToken);

                break;
            }
        }
    }
    
    #endregion
    
    //--------------------------------------POOLED CELLS-----------------------------------

    #region Pooled Objects

    /// <summary>
    /// Returns the first inactive cell object and turns it to active
    /// </summary>
    private GameObject ReturnPooledObject()
    {
        if (pooledCells.Count == 0)
        {
            for (int i = 0; i < amountToPool; i++)
            {
                GameObject cellClone = Instantiate(cellPrefab, Vector3.zero, Quaternion.identity, transform);
                cellClone.SetActive(false);
                pooledCells.Enqueue(cellClone);
            }
        }
        GameObject newCell = pooledCells.Dequeue();
        activeCells.Enqueue(newCell);
        newCell.SetActive(true);
        return newCell;
    }
    
    /// <summary>
    /// Deactivate all cells
    /// </summary>
    public void DeactivateAllCells()
    {
        List<GameObject> activatedCells = activeCells.ToList();
        foreach (var cel in activatedCells)
        {
            cel.SetActive(false);
            pooledCells.Enqueue(cel);
            activeCells.Dequeue();
        }
    }
    
    #endregion
}
