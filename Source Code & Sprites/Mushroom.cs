/*********************************************************
 * File: Mushroom.cs
 * Author: Gabby Strevay 
 * Purpose: Create a unity whack-a-mushroom game 
 * Due Date: Nov. 20, 2023
 * 
 * Contents of Code:
 * - Classes
 * - Functions
 * - Inheritance
 * - For Loop
 * - While Loop
 * - Switch-Case 
 * - If Statements
 * 
 * Resources:
 * 
 * https://www.youtube.com/watch?v=oqnr7THMbcU
 * this resource was the main resource used to help build the game; this tutorial
 * aided in the understanding of unity and making the backbone of this game!
 * 
 * https://gamedevbeginner.com/the-right-way-to-lerp-in-unity-with-examples/
 * this resouce was used in further understanding the lerp function in unity
 ************************************************************/
using System.Collections;
using UnityEngine;
using System;

public class Mushroom : MonoBehaviour
{
    //adding so we have usage of our sprites in the script
    [Header("Graphics")]
    [SerializeField] private Sprite mushroom;
    [SerializeField] private Sprite hardMushroom;
    [SerializeField] private Sprite mushroomHit;
    [SerializeField] private Sprite bomb;
    

    //reference to the GameManager script
    [Header("GameManager")]
    [SerializeField] private GameManager2 gameManager;

    //Vector2 describes X & Y posotion of 2D object
    private Vector2 startPosition = new Vector2(0f, -2.56f); //hide to start game; start -556/100
    private Vector2 endPosition = Vector2.zero; //popped up position aka zero since centered 
    private float showStretch = .5f; //how long it takes to show mushroom; .5 second
    private float stretch = 1f; //how long mushroom is visible; 1 second

    private SpriteRenderer spriteRenderer; //reference to SpriteRenderer so we can change it
    private bool hitMushroom = true; //mushroom hit or not hit parameter

    //private Animator animator; //reference to Animator so we can control the bomb animation

    private BoxCollider2D boxCollider2D; //reference to our boxcollider 
    private Vector2 boxOffset;
    private Vector2 boxSize;
    private Vector2 boxOffsetHidden;
    private Vector2 boxSizeHidden;

    //added enum to make sure we have all possiblities 
    public enum MushType { Normal, Hard, Bomb};
    private MushType mushType; //which one is it
    private float hardVersion = .17f; //how often do we want a hard version; 1/4 of the time
    private float bombRate = .25f;
    private int lives; //hard version has 2 lives
    private int mushIndex = 0; //know which mushroom they are on the board

    /*using this function to execute across multiple frames (ie coroutine) and start with 
    this movement; moving mushroom up and down*/
    private IEnumerator ShowHide(Vector2 start, Vector2 end)
    {
        transform.localPosition = start; //showing mushroom begins @ start; startPosition as defined

        /*showing the mushroom loop 
            lerp loop returns a value between two others at a point on a linear scale 
        Time.deltaTime: represents the time interval in sec it took from the
        last frame to the current frame */
        float timePassed = 0f; //start at 0 seconds
        for(timePassed=0; timePassed < showStretch; timePassed+=Time.deltaTime)
        {
            transform.localPosition = Vector2.Lerp(start, end, timePassed / showStretch);
            boxCollider2D.offset = Vector2.Lerp(boxOffsetHidden, boxOffset, timePassed / showStretch);
            boxCollider2D.size = Vector2.Lerp(boxSizeHidden, boxSize, timePassed / showStretch);
            yield return null; //return loop again in next frame
        }

        //mushroom is at endPosition; fully out/not hidden
        transform.localPosition = end; 
        boxCollider2D.offset = boxOffset;
        boxCollider2D.size = boxSize;

        yield return new WaitForSeconds(stretch); //wait 1 second in mushroom shown state

        /*hiding mushroom loop; almost exact same as show mushroom loop
        reverse lerp so it goes end to start to hide it accordingly */
        timePassed = 0f;
        while(timePassed < showStretch)
        {
            transform.localPosition = Vector2.Lerp(end, start, timePassed / showStretch);
            boxCollider2D.offset = Vector2.Lerp(boxOffset, boxOffsetHidden, timePassed / showStretch);
            boxCollider2D.size = Vector2.Lerp(boxSize, boxSizeHidden, timePassed / showStretch);

            timePassed = timePassed + Time.deltaTime;
            yield return null;
        }
        transform.localPosition = start; //mushroom is at startPosition ; fully hidden
        boxCollider2D.offset = boxOffsetHidden;
        boxCollider2D.size = boxSizeHidden;

        //at the end and didnt hit a mushroom, its missed
        if(hitMushroom)
        {
            hitMushroom = false;
            gameManager.Missed(mushIndex, mushType != MushType.Bomb); //penalty if not a bomb
        }
    }

    private void Awake()
    {
        //obtain references to components needed; the Spriterenderer and Animator
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        //animator = GetComponent<Animator>();

        boxCollider2D = GetComponent<BoxCollider2D>(); //obtain our variable pertaining the collider
        //what the collider values should be
        boxOffset = boxCollider2D.offset; //original value
        boxSize = boxCollider2D.size; //original value
        boxOffsetHidden = new Vector2(boxOffset.x, -startPosition.y / 2f); //hidden; 1/2 start pos in y; base of sprites
        boxSizeHidden = new Vector2(boxSize.x, 0f);
    }

    private void OnMouseDown()
    {
        if (hitMushroom)
        {
            switch (mushType)
            {
                case MushType.Normal:
                    spriteRenderer.sprite = mushroomHit; //once mushroom is clicked, change to hit image
                    gameManager.AddScore(mushIndex); //add to score
                    StopAllCoroutines(); //stop the animation
                    StartCoroutine(QuickHide()); //hide it for the moment
                    hitMushroom = false; //stop ability for hitting mushroom
                    break;
                case MushType.Hard:
                    if(lives ==2)
                    {
                        spriteRenderer.sprite = mushroom;
                        lives--;
                    }
                    else
                    {
                        spriteRenderer.sprite = mushroomHit;
                        gameManager.AddScore(mushIndex); //add to score
                        StopAllCoroutines(); //stop the animation
                        StartCoroutine(QuickHide()); //hide it for the moment
                        hitMushroom = false; //stop ability for hitting mushroom                     
                    }
                    break;
                case MushType.Bomb:
                    gameManager.GameOver(1); //set game to OVER
                    break;
                default:
                    break;
            }
        }
    }
    

    public void Hide()
    {
        transform.localPosition = startPosition; //hides mushroom
        boxCollider2D.offset = boxOffsetHidden; //set the offset
        boxCollider2D.size = boxSizeHidden; // set the box in the hidden postion 
    }
    //documentation for QuickHide()
    private IEnumerator QuickHide()
    {
        yield return new WaitForSeconds(.25f); //wait for .25 seconds
        //check to make sure new spawned mushroom hasnt happened before being hit
        if (!hitMushroom)
        {
            Hide(); //sets location of startPosition
        }
    }

    private void CreateNext()
    {
        float randomNum = UnityEngine.Random.Range(0f, 1f); //rand number btwn 0-1
        if(randomNum < bombRate)
        {
            mushType = MushType.Bomb; //setting MushType to Bomb
            spriteRenderer.sprite = bomb;
            //animator.enabled = true; //animator setting is turned on 
        }
        //randomNum = UnityEngine.Random.Range( 0f, 1f); 
        else if (randomNum < hardVersion)
        {
            mushType = MushType.Hard; //setting MushType to Hard
            spriteRenderer.sprite = hardMushroom;
            lives = 2; //2 lives 
        }
        else //normal mushroom!
        {
            //animator.enabled = false;
            mushType = MushType.Normal; //setting MushType to Normal
            spriteRenderer.sprite = mushroom;
            lives = 1; //1 life
        }
        hitMushroom = true; //hittable mushrooms
    }

    //level changes how often bombs & hardVersion moles occur 
    private void Level(int level)
    {
        bombRate = Mathf.Min((level+1) * .017f, .5f); //at end bombs max occur 50% of time  
        hardVersion = Mathf.Min((level+5) * .025f, 1f); //all are hard at end
        float stretchMin = Mathf.Max(1 - level * .2f, .04f, 1f); 
        float stretchMax = Mathf.Max(2 - level * .2f, .04f, 2f);
        stretch = UnityEngine.Random.Range(stretchMin, stretchMax); //will start as 1 second down to .2 second
    }

    //used by game manager to know which mushroom is which
    public void SetIndex(int index)
    {
        mushIndex = index;
    }

    //used to freeze the game on finish
    public void StopGame()
    {
        hitMushroom = false; //cant hit 
        StopAllCoroutines(); //stop animations 
    }
    // Activate is called before the first frame update
    public void Activate(int level)
    {
        Level(level); //start at level 1; look at GameManager script 
        CreateNext();
        StartCoroutine(ShowHide(startPosition, endPosition));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
