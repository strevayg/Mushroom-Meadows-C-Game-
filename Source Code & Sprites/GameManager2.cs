/*********************************************************
 * File: GameManager2.cs
 * Author: Gabby Strevay 
 * Purpose: Create a unity whack-a-mushroom game 
 * Due Date: Nov. 20, 2023
 * 
 * Contents of Code:
 * - Classes
 * - Functions
 * - Inheritance
 * - File I/O
 * - Exceptions
 * - If Statements
 * - For Loop
 * 
 * Resources:
 * https://www.youtube.com/watch?v=oqnr7THMbcU
 * this resource was the main resource used to help build the game; this tutorial
 * aided in the understanding of unity and making the backbone of this game!
 * https://www.youtube.com/watch?v=6bVcLSZWqK8
 * this resource helped me create a text file from unity
 * https://stackoverflow.com/questions/11663978/check-if-file-exist-with-try-catch 
 * this resouce helped me check if a text file existed and if not catch the exception
 * and create the text file
 ************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; //input and output (file IO)
using System;

public class GameManager2 : MonoBehaviour
{
    [SerializeField] private List<Mushroom> mushrooms; //reference to the mushrooms 

    [Header("UI objects")]
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject outOfTimeText;
    [SerializeField] private GameObject bombText;
    //referencing the TextMeshPro component bc want to update as we play
    [SerializeField] private TMPro.TextMeshProUGUI timeText;
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;

    private float gameTime = 20f; //game lasts 20 seconds
    private float timeLeft;

    //HashSet search for new mushroom to populate will make game faster
    private HashSet<Mushroom> currentMushrooms = new HashSet<Mushroom>();

    private int score;
    private bool currentlyPlaying = false; //set to not playing to start

    //public so playButton can see it
    public void StartGame()
    {
        playButton.SetActive(false); //hide playButton
        outOfTimeText.SetActive(false); //hide out of time text
        bombText.SetActive(false); //hide bomb text
        gameUI.SetActive(true); //display the score and time remaining

        //for loop hides the visible mushrooms
        for(int i = 0; i < mushrooms.Count; i++)
        {
            mushrooms[i].Hide();
            mushrooms[i].SetIndex(i); //setting which mushroom is which from other script
        }
        currentMushrooms.Clear(); //clear the game board for new game
        timeLeft = gameTime; //start with 20 seconds
        score = 0;
        scoreText.text = "0"; //show 0 to start
        currentlyPlaying = true;
    }

    public void GameOver(int type)
    {
        if(type == 0)
        {
            outOfTimeText.SetActive(true); //ran out of time text displays
        }
        else
        {
            bombText.SetActive(true); //display bomb text 
        }

        foreach(Mushroom mushroom in mushrooms)
        {
            mushroom.StopGame(); //hides each mushroom thats visible
        }
        currentlyPlaying = false; //stop game
        playButton.SetActive(true); //show play button

        //make text file containing the high score!
        string path = Application.dataPath + "/mushroomScores.txt";//file path
        try
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(); //function in System.IO; need new instance of this 
            }
        }
        catch(FileNotFoundException)
        {
            File.WriteAllText(path, "!!********High Scores********!!\n\n");
        }
        string scoreData = "Your score on " + System.DateTime.Now + " was " + score + "\n";
        File.AppendAllText(path, scoreData);
    }

    public void AddScore(int mushroomIndex)
    {
        score = score + 1; //add one for each hit
        scoreText.text = $"{score}"; //score text is updated
        timeLeft = timeLeft + 1; //add 1 second
        currentMushrooms.Remove(mushrooms[mushroomIndex]); //remove the mush/bomb
    }

    public void Missed(int mushroomIndex, bool isMush)
    {
        if(isMush)
        {
            timeLeft = timeLeft - 2; //missed a mushroom so -2 on time
        }
        currentMushrooms.Remove(mushrooms[mushroomIndex]); //remove the mush/bomb  
    }

   
    // Start is called before the first frame update
    public void Start()
    {
         
    }

    // Update is called once per frame
    void Update()
    {
        if(currentlyPlaying)
        {
            timeLeft = timeLeft - Time.deltaTime; //update the time
            if(timeLeft <= 0) //if above makes it go to zero or below, game over
            {
                timeLeft = 0;
                GameOver(0);
            }
            //minutes:seconds; D2 ensures theres always 2 digits for seconds
            timeText.text = $"{(int)timeLeft / 60}:{(int)timeLeft % 60:D2}";

            if(currentMushrooms.Count <= (score / 10)) //every 10 mole hit, new appear
            {
                int index = UnityEngine.Random.Range(0, mushrooms.Count); //choose random mushroom
                if(!currentMushrooms.Contains(mushrooms[index]))
                {
                    currentMushrooms.Add(mushrooms[index]);
                    mushrooms[index].Activate((score + 10) / 10); //level is score/10 here; go up a lvl for every 10 hit
                }
            }
        }
    }
}
