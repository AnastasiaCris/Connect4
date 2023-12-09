using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Variables

    //dictionary
    [Tooltip("A dictionary that stores the grid positions and what token game objects occupy them")]
    public Dictionary<(int,int), GameObject> GridTokenObjects = new Dictionary<(int,int), GameObject>();
    
    //game related
    [HideInInspector] public int winNr = 4; // amount of tokens in a row to win
    [HideInInspector] public bool canDropToken;
    private int currentPlayerId; //the current player's turn
    private int firstPlayer = 2; // which player starts first
    private int mousePosX;
    private Camera _camera;
    
    //public getter
    public int PlayerID { get { return currentPlayerId; } private set { currentPlayerId = value; } }
    
    //scripts
    [SerializeField]private C4Grid c4GridScript;
    [SerializeField]private TokenGen tokenGenScript;
    public static GameManager instance;

    #endregion

    private void Awake()
    {
        if (instance == null) instance = this;
    }
    private void Start()
    {
        currentPlayerId = firstPlayer;
        _camera = Camera.main;
    }
    
    private void Update()
    {
        PlayerInput();
    }
    
    //--------------------------------------NEW GAME-------------------------------------
    
    #region New Game
    public void StartNewGame()
    {
        if (winNr == 0) winNr = 4; // make sure wining condition is never 0
        firstPlayer = firstPlayer == 1 ? 2 : 1; //change the first player to start the game
        currentPlayerId = firstPlayer;
        UIManager.instance.VisualizePlayerTurnText(currentPlayerId);
        
        StartCoroutine(NewGame());
    }

    private IEnumerator NewGame()
    {
        //make all tokens false
        tokenGenScript.DeactivateAllTokens();
        
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
        StartCoroutine(tokenGenScript.CreateTokenGhost());
        
        //reset sound
        UIManager.instance.ResetSounds();
    }
    #endregion
    
    //--------------------------------------PLAYER RELATED-------------------------------------

    #region Player Related

    /// <summary>
    /// Get the player mouse x pos and drops a token whenever the player clicks within grid bounds
    /// </summary>
    private void PlayerInput()
    {
        if (!canDropToken) return;

        if(_camera != null) //calculate the mouse x pos relative to the grid
            mousePosX = Mathf.RoundToInt(_camera.ScreenToWorldPoint(Input.mousePosition).x - c4GridScript.StartX);

        if (IsWithinGridBounds()) //check if mouse is within grid bounds
        {
            tokenGenScript.ChangeTokenGhostPos(mousePosX);
            
            if (Input.GetMouseButtonDown(0))
            {
                canDropToken = false;
                c4GridScript.UpdateGridInfo(mousePosX);

            }
        }
    }
    public bool IsWithinGridBounds()
    {
        return mousePosX >= 0 && mousePosX < C4Grid.Width;
    }
    private void ChangePlayerTurn()
    {
        currentPlayerId = currentPlayerId == 2 ? 1 : 2;
        UIManager.instance.VisualizePlayerTurnText(currentPlayerId);
        canDropToken = true;
        
        tokenGenScript.VisualizeTokenGhost();//change appearance of the token ghost
    }

    /// <summary>
    /// Checks for a win/draw condition
    /// Played when a token landed
    /// </summary>
    /// <param name="xPos"> the X position of the token </param>
    /// <param name="yPos"> the Y position of the token </param>
    public void TokenLanded(int xPos, int yPos)
    {
        List<(int, int)> winPos = new List<(int, int)>();
        
        if (CheckWinCondition( xPos, yPos, out winPos)) //Checking for a win condition
        {
            StartCoroutine(VisualizeWinningTokens(winPos)); 

        }
        else if (BoardFull()) //check if the board is full (draw)
        {
            StartCoroutine(HandleDraw());
        }
        else
        {
            ChangePlayerTurn();
        }
    }
    #endregion
    
    //--------------------------------------DRAW CONDITION-------------------------------------

    #region Draw Condition
    /// <summary>
    /// Checks if the board is full with tokens
    /// </summary>
    private bool BoardFull()
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

    private IEnumerator HandleDraw()
    {
        List<GameObject> allTokens = tokenGenScript.ReturnListOfPlacedTokens();

        // an animation
        for (int i = 0; i < allTokens.Count; i++)
        {
            allTokens[i].GetComponent<SpriteRenderer>().color = Color.black;
            yield return new WaitForSeconds(0.2f);  
        }

        UIManager.instance.TurnOnEndMenu(false);
    }
    
    #endregion

    //--------------------------------------WIN CONDITION-------------------------------------

    #region Win Condition
    /// <summary>
    /// Animates the winning tokens and at the end of the animation handles win
    /// </summary>
    /// <param name="winPos"> The pos in the grid of the winning tokens </param>
    private IEnumerator VisualizeWinningTokens(List<(int,int)> winPos)
    {
        Color plCol = Color.white;

        float pitchSound = 0.8f;
        
        for (int i = 0; i < winPos.Count; i++)
        {
            if (GridTokenObjects.TryGetValue(winPos[i], out GameObject tokenObject))
            {
                //play sound
                tokenGenScript.PlayEndGameTokensSound(pitchSound);
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
        
        yield return new WaitForSeconds(0.5f); // once animation is done -> handle win
        HandleWin();
    }
   
    /// <summary>
    /// Check for a win condition horizontally, vertically and diagonally
    /// and adds the winning grid positions to a list
    /// </summary>
    /// <param name="currTokenX">The X value of the current token placed</param>
    /// <param name="currTokenY">The Y value of the current token placed</param>
    /// <param name="winPositions">A list of the winning tokens positions</param>
    private bool CheckWinCondition( int currTokenX, int currTokenY, out List<(int,int)> winPositions)
    {
        List<(int,int)> winPos = new List<(int,int)>();

        // Check horizontally
        for (int x = 0; x < C4Grid.Width - (winNr - 1); x++)
        {
            if (CheckHorizontal(x, currTokenY))
            {
                // Add winning positions to the list
                for (int i = 0; i < 4; i++)
                {
                    winPos.Add((x + i, currTokenY));
                }

                winPositions = winPos;
                return true;
            }
        }

        // Check vertically
        for (int y = 0; y < C4Grid.Height - (winNr - 1); y++)
        {
            if (CheckVertical(currTokenX, y))
            {
                // Add winning positions to the list
                for (int i = 0; i < 4; i++)
                {
                    winPos.Add((currTokenX, y + i));
                }
                
                winPositions = winPos;

                return true;
            }
        }

        // Check diagonally (down-right)
        if (CheckDiagonal( currTokenX, currTokenY, -1, -1, out winPos) || 
            CheckDiagonal( currTokenX, currTokenY, 1, 1, out winPos))
        {
            winPositions = winPos;

            return true;
        }

        // Check diagonally (up-right)
        if (CheckDiagonal(currTokenX, currTokenY, -1, 1, out winPos) || 
            CheckDiagonal(currTokenX, currTokenY, 1, -1, out winPos))
        {
            winPositions = winPos;

            return true;
        }
        
        winPositions = null;
        return false;
    }

    /// <summary>
    /// Checks for a win condition along a Horizontal line
    /// </summary>
    /// <param name="x"> the starting x position to check from</param>
    /// <param name="y"> the y position on which to check</param>
    /// <returns></returns>
    private bool CheckHorizontal(int x, int y)
    {
        int amountToCheck = 0;

        for (int i = 0; i < winNr; i++)
        {
            if (c4GridScript.CellInfo[x + amountToCheck, y] == currentPlayerId)
            {
                amountToCheck++;

                if (amountToCheck == winNr)
                    return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Checks for a win condition along a Vertical line
    /// </summary>
    /// <param name="x"> the y position on which to check </param>
    /// <param name="y"> the starting y position to check from </param>
    /// <returns></returns>
    private bool CheckVertical(int x, int y)
    {
        int amountToCheck = 0;

        for (int i = 0; i < winNr; i++)
        {
            if (c4GridScript.CellInfo[x , y + amountToCheck] == currentPlayerId)
            {
                amountToCheck++;

                if (amountToCheck == winNr)
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks for a win condition along a diagonal line in both directions
    /// </summary>
    /// <param name="startX">The starting X value of the check</param>
    /// <param name="startY">The starting Y value of the check</param>
    /// <param name="xDir">The direction of movement in the X axis (1 or -1)</param>
    /// <param name="yDir">The direction of movement in the Y axis (1 or -1)</param>
    /// <param name="winPositions">A list of the winning tokens positions</param>
    private bool CheckDiagonal(int startX, int startY, int xDir, int yDir, out List<(int,int)> winPositions)
    {
        int count = 0;
        List<(int,int)> winPos = new List<(int,int)>();

        for (int i = -(winNr - 1); i <= winNr - 1; i++)
        {
            int col = startX + i * xDir;
            int row = startY + i * yDir;

            if (col >= 0 && col < C4Grid.Width && row >= 0 && row < C4Grid.Height && c4GridScript.CellInfo[col, row] == currentPlayerId)
            {
                count++;
                winPos.Add((col, row)); // Add winning position

                if (count == winNr)
                {
                    winPositions = winPos;
                    return true;
                }
            }
        }
        winPositions = winPos;
        return false;
    }

    /// <summary>
    /// This void is called when a player wins the game
    /// </summary>
    private void HandleWin()
    {
        UIManager.instance.TurnOnEndMenu(true, currentPlayerId);
    }
    
    #endregion
    

}
