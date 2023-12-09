using System;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class UIManager : MonoBehaviour
{
    #region Variables

        [Header("Menus")]
        [SerializeField] private TextMeshProUGUI winTMP;
        [SerializeField] private GameObject winMenu;
        [SerializeField] private AudioClip endSound;
        [SerializeField] private AudioClip buttonSound;
        [SerializeField] private AudioSource bckgSoundSource;
        [SerializeField] private AudioSource soundEfxSource;
        public Color winCol;
        
        [Header("Player Visualizer")]
        [SerializeField] private TextMeshProUGUI playerTurnTMP;
        
        [Header("Base Game")]
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private AudioMixer audioMixer;
        
        public static UIManager instance;
    
    #endregion

    private void Awake()
    {
        if (instance == null) instance = this;
    }
    
    //---------------------------------GAME RELATED--------------------------------

    # region Game Related

    public void VisualizePlayerTurnText(int playerId)
    {
        playerTurnTMP.text = $"P{playerId} turn";
    }

    /// <summary>
    /// Turns on the end menu
    /// </summary>
    /// <param name="win"> Is it the winning menu</param>
    /// <param name="playerID"> Which player won </param>
    public void TurnOnEndMenu(bool win,int playerID = 1)
    {
        if (win)
        {
            winTMP.text = $"PLAYER {playerID} WON!";
        }
        else
        {
            winTMP.text = $"DRAW!";

        }
        bckgSoundSource.clip = endSound;
        bckgSoundSource.loop = false;
        bckgSoundSource.Play();
        winMenu.SetActive(true);
    }
    
    #endregion
    
    //---------------------------------SOUND--------------------------------

    #region Sound

    /// <summary>
    /// Changes the overall volume of the game 
    /// </summary>
    /// <param name="vol"></param>
    public void SetVolume(float vol)
    {
        audioMixer.SetFloat("Volume", Mathf.Log10(vol) * 20);
    }
    
    /// <summary>
    /// resets the background music
    /// </summary>
    public void ResetBckgSound()
    {
        bckgSoundSource.clip = backgroundMusic;
        bckgSoundSource.loop = true;
        bckgSoundSource.Play();
    }
    
    /// <summary>
    /// played whenever hovering over a button
    /// </summary>
    public void PlayButtonSound()
    {
        soundEfxSource.pitch = 1;
        soundEfxSource.volume = 0.3f;
        soundEfxSource.PlayOneShot(buttonSound);//the hover sound
    }
    
    #endregion
    
    //---------------------------------GRID VALUES--------------------------------

    #region Grid Values

    
    /// <summary>
    /// Change Width of the grid in the input field
    /// </summary>
    public void OnWidthValueChanged(TMP_InputField textField)
    {
        int parseW = 0;
        if (int.TryParse(textField.text, out parseW))
        {
            parseW = Int32.Parse(textField.text);
        }
        if (parseW <= 4 && textField.text.Length > 0) //width can't be smaller then 4
        {
            parseW = 4;
            textField.text = parseW.ToString();
        }
        if (parseW >= 15) //width can't be bigger then 15
        {
            parseW = 15;
            textField.text = parseW.ToString();
        }

        C4Grid.Width = parseW;
    }
    
    /// <summary>
    /// Change Height of the grid in the input field
    /// </summary>
    public void OnHeightValueChanged(TMP_InputField textField)
    {
        int parseH = 0;
        if (int.TryParse(textField.text, out parseH))
        {
            parseH = Int32.Parse(textField.text);
        }
        if (parseH <= 4 && textField.text.Length > 0) //height can't be smaller then 4
        {
            parseH = 4;
            textField.text = parseH.ToString();
        }
        if (parseH >= 14) //height can't be bigger then 14
        {
            parseH = 14;
            textField.text = parseH.ToString();
        }

        C4Grid.Height = parseH;
    }
    
    #endregion
    
    //---------------------------------WIN CONDITION VALUE--------------------------------

    #region Win Condition Value
    
    /// <summary>
    /// Change the win condition (how many tokens in a row to win)
    /// </summary>
    public void OnWinNrValueChanged(TMP_InputField textField)
    {
        int parseWin = 0;
        if (int.TryParse(textField.text, out parseWin))
        {
            parseWin = Int32.Parse(textField.text);
        }
        if (parseWin <= 3 && textField.text.Length > 0) //can't be smaller then 3
        {
            parseWin = 3;
            textField.text = parseWin.ToString();
        }

        int smallestNr = C4Grid.Height < C4Grid.Width ? C4Grid.Height : C4Grid.Width; //choose the smallest nr between the width and height of the board
        if (parseWin > smallestNr) //can't be bigger then grid size
        {
            parseWin = smallestNr;
            textField.text = parseWin.ToString();
        }

        GameManager.instance.winNr = parseWin;
    }
    
    #endregion
    
    //---------------------------------CAMERA--------------------------------

    #region Camera

    /// <summary>
    /// Setting up camera position and size to always be in the middle of the maze and always be able to see the whole maze
    /// </summary>
    public void SetUpCamera(float width, float height)
    {
        Camera cam = Camera.main;
        
        float camZoom = height >= width || height == width? 2 + height / 2 : 1 + width / 2;

        if (camZoom <= 4) camZoom = 4; // size can't be smaller then 4

        cam.orthographicSize = camZoom; //set the size
    }
    
    #endregion
    
}
