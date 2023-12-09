using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenGen : MonoBehaviour
{
    #region Variables

    [Header("Token Properties")]
    [SerializeField] private GameObject tokenObj;
    [SerializeField] private AudioSource tokenSound;
    [SerializeField] private AudioClip dropSound;
    [SerializeField] private AudioClip winningTokenSound;
    private GameObject tokenGhost;

    [Header("Object Pooling")]
    private List<GameObject> pooledTokens = new List<GameObject>();
    private int amountToPool = 42;
    
    [Header("Scripts")]
    [SerializeField]private C4Grid c4GridScript;
    
    #endregion


    private void Awake()
    {
        for (int i = 0; i < amountToPool; i++)
        {
            GameObject obj = Instantiate(tokenObj, Vector3.zero, Quaternion.identity, transform);
            obj.SetActive(false);
            pooledTokens.Add(obj);
        }
    }

    //--------------------------------------TOKEN GHOST BEHAVIOUR-----------------------------------
    
    #region Token Ghost Behaviour

    /// <summary>
    /// Token ghost will visualize to the player where the token will spawn (from the top of the board)
    /// </summary>
    public IEnumerator CreateTokenGhost()
    { 
        yield return new WaitUntil(() => GameManager.instance.IsWithinGridBounds());
       tokenGhost = ReturnPooledObject();
       tokenGhost.SetActive(true);

       VisualizeTokenGhost();
    }
    
    /// <summary>
    /// Visualize the show token to match the current player
    /// </summary>
    public void VisualizeTokenGhost()
    {
        tokenGhost.SetActive(true);
        Animator tokenAnim = tokenGhost.GetComponent<Animator>();
        tokenAnim.SetInteger("id", GameManager.instance.PlayerID);
        SpriteRenderer tokenSprite = tokenGhost.GetComponent<SpriteRenderer>();
        Color tokenCol = Color.white;
        tokenCol = new Color(tokenCol.r, tokenCol.g, tokenCol.b, 0.4f); // lower the opacity
        tokenSprite.color = tokenCol;
    }

    /// <summary>
    /// Changes the position of the show token
    /// </summary>
    /// <param name="xPos"> the desired x pos of the show token </param>
    public void ChangeTokenGhostPos(int xPos)
    {
        if(tokenGhost == null) return;
        
        Vector3 desiredPos = new Vector3(c4GridScript.StartX + xPos, c4GridScript.StartY + C4Grid.Height);
        
        if(tokenGhost.transform.position != desiredPos)
            tokenGhost.transform.position = desiredPos;
    }
    
    #endregion
    
    //--------------------------------------TOKEN BEHAVIOUR-----------------------------------
    
    #region Token Behaviour

    /// <summary>
    /// Pools a token out and makes the drop in the desired position
    /// </summary>
    /// <param name="xPos"> the desired x position </param>
    /// <param name="yPos"> the desired y position </param>
    /// <returns></returns>
    public GameObject InstantiateToken(int xPos, int yPos)
    {
        GameObject tokenObjClone = ReturnPooledObject();
        tokenObjClone.transform.position = new Vector2(c4GridScript.StartX + xPos, c4GridScript.StartY + C4Grid.Height);
        tokenObjClone.SetActive(true);
        
        //Visualize to match the player
        Animator tokenAnim = tokenObjClone.GetComponent<Animator>();
        tokenAnim.SetInteger("id", GameManager.instance.PlayerID);
        SpriteRenderer tokenSprite = tokenObjClone.GetComponent<SpriteRenderer>();
        tokenSprite.color = Color.white;

        //Visualize the drop
        StartCoroutine(VisualizeTokenDrop(tokenObjClone, xPos, yPos));

        return tokenObjClone;
    }

    /// <summary>
    /// As long as there are free spaces in a column drop the token
    /// </summary>
    /// <param name="tokenObject"> The token object to drop </param>
    /// <param name="xPos"> The column in which to drop the token </param>
    /// <param name="yPos"> The row in which the token drops </param>
    private IEnumerator VisualizeTokenDrop(GameObject tokenObject, int xPos, int yPos)
    {
        tokenGhost.SetActive(false);//hide token ghost
        
        //gather the free spaces of a column in a list
        List<Vector2> freeSpaces = new List<Vector2>();
        
        for (int i = C4Grid.Height - 1; i >= 0 ; i--)
        {
            if (c4GridScript.CellInfo[xPos, i] == 0)
            {
                Vector2 freeSpace = new Vector2(c4GridScript.StartX + xPos, c4GridScript.StartY + i);
                
                freeSpaces.Add(freeSpace);
            }
        }

        tokenSound.clip = dropSound;
        tokenSound.volume = 0.5f;
        
        //make the drop
        for (int i = 0; i < freeSpaces.Count; i++)
        {
            yield return new WaitForSeconds(0.2f);  
            
            tokenObject.transform.position = freeSpaces[i];
            
            //Play Sound on drop
            if (Math.Abs(freeSpaces[i].y - (c4GridScript.StartY + yPos)) < 0.5f) //checking if reached the bottom to play different sound
            {
                tokenSound.pitch = 0.8f;
                tokenSound.Play();
            }
            else
            {
                tokenSound.pitch = 1f;
                tokenSound.Play();
            }
        }
        
        //The end of the drop

        GameManager.instance.TokenLanded(xPos, yPos);
    }

    public void PlayEndGameTokensSound(float pitch)
    {
        tokenSound.pitch = pitch;
        tokenSound.volume = 0.15f;
        tokenSound.clip = winningTokenSound;
        tokenSound.Play();
    }
    
    #endregion
    
    //--------------------------------------POOLED TOKENS-----------------------------------
    
    #region Pooled Objects

    /// <summary>
    /// Returns an inactive token object
    /// </summary>
    private GameObject ReturnPooledObject()
    {
        for (int i = 0; i < pooledTokens.Count; i++)
        {
            if (!pooledTokens[i].activeSelf)
            {
                return pooledTokens[i];
            }
        }
        return null;
    }

    public List<GameObject> ReturnListOfPlacedTokens()
    {
        List<GameObject> placedTokens = new List<GameObject>();

        for (int i = 1; i < pooledTokens.Count; i++)
        {
            if (pooledTokens[i].activeSelf)
            {
                placedTokens.Add(pooledTokens[i]);
            }
        }

        return placedTokens;
    }
    public void DeactivateAllTokens()
    {
        for (int i = 0; i < pooledTokens.Count; i++)
        {
            pooledTokens[i].SetActive(false);
        }
    }
    
    #endregion
}
