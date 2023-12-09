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
    [SerializeField] private AudioClip winSound;
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

    public void VisualizePlayerTurn(int playerId)
    {
        playerTurnTMP.text = $"P{playerId} turn";
    }

    public void TurnOnWinMenu(int playerID)
    {
        winTMP.text = $"PLAYER {playerID} WON!";
        bckgSoundSource.clip = winSound;
        bckgSoundSource.loop = false;
        bckgSoundSource.Play();
        winMenu.SetActive(true);
    }

    public void TurnOnDrawMenu()
    {
        winTMP.text = $"DRAW!";
        bckgSoundSource.clip = winSound;
        bckgSoundSource.loop = false;
        bckgSoundSource.Play();
        winMenu.SetActive(true);
    }

    /// <summary>
    /// played whenever hovering over a button
    /// </summary>
    public void PlayButtonSound()
    {
        soundEfxSource.volume = 0.3f;
        soundEfxSource.PlayOneShot(buttonSound);//the hover sound
    }

    /// <summary>
    /// Changes the overall volume of the game 
    /// </summary>
    /// <param name="vol"></param>
    public void SetVolume(float vol)
    {
        audioMixer.SetFloat("Volume", Mathf.Log10(vol) * 20);
    }
    
    //---------------------------------GRID VALUES--------------------------------

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
    
    //---------------------------------CAMERA--------------------------------

    /// <summary>
    /// Setting up camera position and size to always be in the middle of the maze and always be able to see the whole maze
    /// </summary>
    public void SetUpCamera(float width, float height)
    {
        Camera cam = Camera.main;
        
        float camZoom = height >= width || height == width? 2 + height / 2 : 0.5f + width / 2;

        if (camZoom <= 4) camZoom = 4; // size can't be smaller then 4

        cam.orthographicSize = camZoom; //set the size
    }
    
    //---------------------------------BUTTONS--------------------------------

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ResetSounds()
    {
        bckgSoundSource.clip = backgroundMusic;
        bckgSoundSource.loop = true;
        bckgSoundSource.Play();
    }
}
