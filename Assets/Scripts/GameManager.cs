using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Variables

    //dictionary
    [Tooltip("A dictionary that stores the grid positions and what token game objects occupy them")]
    public Dictionary<(int,int), GameObject> GridTokenObjects = new Dictionary<(int,int), GameObject>();
    
    //player related
    [HideInInspector] public bool canDropToken;
    private int currentPlayerId = 1;
    private int mousePosX;
    private Camera _camera;
    
    //public getter
    public int PlayerID { get { return currentPlayerId; } private set { currentPlayerId = value; } }
    
    [Header("Scripts")]
    [SerializeField]private C4Grid c4GridScript;
    public static GameManager instance;
    
    #endregion

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Update()
    {
        if (!canDropToken) return;
        
        if(_camera != null)
            mousePosX = Mathf.RoundToInt(_camera.ScreenToWorldPoint(Input.mousePosition).x - c4GridScript.StartX);

        if (IsWithinBounds())
        {
            TokenGen.instance.ChangeTokenGhostPos(mousePosX);
            
            //on mouse click
            if (Input.GetMouseButtonDown(0))
            {
                canDropToken = false;
                c4GridScript.UpdateGridInfo(mousePosX);

            }
        }
    }
    
    //--------------------------------------PLAYER RELATED-------------------------------------

    public bool IsWithinBounds()
    {
        return mousePosX >= 0 && mousePosX < C4Grid.Width;
    }
    public void ChangePlayerTurn()
    {
        currentPlayerId = currentPlayerId == 2 ? 1 : 2;
        UIManager.instance.VisualizePlayerTurn(currentPlayerId);
        canDropToken = true;
    }
    
    //--------------------------------------DRAW CONDITION-------------------------------------

    /// <summary>
    /// Checks if the board is full with tokens
    /// </summary>
    public bool BoardFull()
    {
        for (int x = 0; x < C4Grid.Width; x++)
        {
            for (int y = 0; y < C4Grid.Height; y++)
            {
                if (c4GridScript.CellInfo[x, y] == 0) //if cell not occupied
                {
                    return false;
                }
            }
        }

        return true;
    }

    public IEnumerator HandleDraw()
    {
        
        // an animation
        yield return new WaitForSeconds(1);
        UIManager.instance.TurnOnDrawMenu();
    }

    //--------------------------------------WIN CONDITION-------------------------------------

    /// <summary>
    /// Turns the winning tokens to a win color
    /// </summary>
    /// <param name="winPos"> The pos in the grid of the winning tokens </param>
    /// <param name="playerId"> The player that won </param>
    private IEnumerator VisualizeWinningTokens(List<(int,int)> winPos, int playerId)
    {
        Color plCol = Color.white;

        float pitchSound = 0.8f;
        
        for (int i = 0; i < winPos.Count; i++)
        {
            if (GridTokenObjects.TryGetValue(winPos[i], out GameObject tokenObject))
            {
                //play sound
                TokenGen.instance.PlayWinningTokensSound(pitchSound);
                pitchSound += 0.1f;
                
                //Change color
                tokenObject.GetComponent<SpriteRenderer>().color = UIManager.instance.winCol;
                yield return new WaitForSeconds(0.1f);  
                tokenObject.GetComponent<SpriteRenderer>().color = plCol;
                yield return new WaitForSeconds(0.1f);  
                tokenObject.GetComponent<SpriteRenderer>().color = UIManager.instance.winCol;
                yield return new WaitForSeconds(0.1f);  
                tokenObject.GetComponent<SpriteRenderer>().color = plCol;
                yield return new WaitForSeconds(0.1f);  
                tokenObject.GetComponent<SpriteRenderer>().color = UIManager.instance.winCol;
                
            }
        }
    }
   
    /// <summary>
    /// Check for a win condition horizontally, vertically and diagonally
    /// and adds the winning grid positions to a list
    /// </summary>
    /// <param name="player">The player value to check for</param>
    /// <param name="currTokenX">The X value of the current token placed</param>
    /// <param name="currTokenY">The Y value of the current token placed</param>
    public bool CheckWinCondition(int player, int currTokenX, int currTokenY)
    {
        List<(int,int)> winPos = new List<(int,int)>();

        // Check horizontally
        for (int x = 0; x < C4Grid.Width - 3; x++)
        {
            if (c4GridScript.CellInfo[x, currTokenY] == player && c4GridScript.CellInfo[x + 1, currTokenY] == player &&
                c4GridScript.CellInfo[x + 2, currTokenY] == player && c4GridScript.CellInfo[x + 3, currTokenY] == player)
            {
                // Add winning positions to the list
                for (int i = 0; i < 4; i++)
                {
                    winPos.Add((x + i, currTokenY));
                }

                StartCoroutine(VisualizeWinningTokens(winPos, player));
                return true;
            }
        }

        // Check vertically
        for (int y = 0; y < C4Grid.Height - 3; y++)
        {
            if (c4GridScript.CellInfo[currTokenX, y] == player && c4GridScript.CellInfo[currTokenX, y + 1] == player &&
                c4GridScript.CellInfo[currTokenX, y + 2] == player && c4GridScript.CellInfo[currTokenX, y + 3] == player)
            {
                // Add winning positions to the list
                for (int i = 0; i < 4; i++)
                {
                    winPos.Add((currTokenX, y + i));
                }
                
                StartCoroutine(VisualizeWinningTokens(winPos, player));
                return true;
            }
        }

        // Check diagonally (down-right)
        if (CheckDiagonal(player, currTokenX, currTokenY, -1, -1) || 
            CheckDiagonal(player, currTokenX, currTokenY, 1, 1))
        {
            return true;
        }

        // Check diagonally (up-right)
        if (CheckDiagonal(player, currTokenX, currTokenY, -1, 1) || 
            CheckDiagonal(player, currTokenX, currTokenY, 1, -1))
        {
            return true;
        }

        // No win condition found
        return false;
    }
    
    /// <summary>
    /// Checks for a win condition along a diagonal line in both directions
    /// </summary>
    /// <param name="player">The player value to check for</param>
    /// <param name="startX">The starting X value of the check</param>
    /// <param name="startY">The starting Y value of the check</param>
    /// <param name="xDir">The direction of movement in the X axis (1 or -1)</param>
    /// <param name="yDir">The direction of movement in the Y axis (1 or -1)</param>
    /// <param name="winPos">A list of the winning tokens positions</param>
    private bool CheckDiagonal(int player, int startX, int startY, int xDir, int yDir)
    {
        int count = 0;
        List<(int,int)> winPos = new List<(int,int)>();

        for (int i = -3; i <= 3; i++)
        {
            int col = startX + i * xDir;
            int row = startY + i * yDir;

            if (col >= 0 && col < C4Grid.Width && row >= 0 && row < C4Grid.Height && c4GridScript.CellInfo[col, row] == player)
            {
                count++;
                winPos.Add((col, row)); // Add winning position

                if (count == 4)
                {
                    StartCoroutine(VisualizeWinningTokens(winPos, player));
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// This void is called when a player wins the game
    /// </summary>
    public void HandleWin(int playerId)
    {
        UIManager.instance.TurnOnWinMenu(playerId);
    }
    
    //---------------------------------BUTTONS--------------------------------

    public void StartNewGame()
    {
        StartCoroutine(NewGame());
    }

    private IEnumerator NewGame()
    {
        //make all tokens false
        TokenGen.instance.DeactivateAllTokens();
        
        //reset the grid to all 0
        for (int x = 0; x < C4Grid.Width; x++)
        {
            for (int y = 0; y < C4Grid.Height; y++)
            {
                c4GridScript.CellInfo[x, y] = 0;
            }
        }

        //reset the dictionary
        GridTokenObjects.Clear();
        
        //make possible to drop tokens 
        yield return new WaitForSeconds(0.5f);
        canDropToken = true;
        StartCoroutine(TokenGen.instance.CreateTokenGhost());
    }
}
