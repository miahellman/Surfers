using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpotInstance1 : MonoBehaviour
{

    public String spotName = "Test Zone!";

    public float spotRadius = 10;


    public float highScore = 0;

    public float startingScore = 100;

    public float localSkillCieling = 900;
    public float localSkillFloor = 400;

    public string currentOwner = "No One!";

    public TMP_Text textToEdit = null;

    public Transform playerTransform = null;

    public string[] localBoarderNames = {
        "Charless Pratt",
        "Outboarder",
        "Rosie",
        "Poseidon",
        "Jocelyn"

    };


    public float scoreChallengingDelay = 2;

    public float scoreChallengingTimer = 0;





    // Start is called before the first frame update
    void Start()
    {
        SetupInitialScore();

        if (playerTransform == null)
        {
            playerTransform = transform;
        }
    }

    // Update is called once per frame
    void Update()
    {

        //If the current highscore is more than the skill cieling of the other boarders, then their score shouldn't get challenged anymore.
        if (highScore < localSkillCieling && ((transform.position - playerTransform.position).magnitude > spotRadius) || playerTransform == transform)
        {
            bool locallyOwned = false;
            //First we check if the current owner isn't one of the boarders
            for (int i = 0; i < localBoarderNames.Length; i++)
            {
                if (currentOwner == localBoarderNames[i])
                {
                    locallyOwned = true;
                }



            }


            if (!locallyOwned)
            {
                if (scoreChallengingTimer < scoreChallengingDelay)
                {
                    scoreChallengingTimer += Time.deltaTime;
                }
                else
                {
                    scoreChallengingTimer = 0;

                    ProcessNewScore(RandomScore(), RandomBoarder());

                }


            }
        }


    }


    void SetupInitialScore()
    {
        ProcessNewScore(startingScore, RandomBoarder());

    }


    public void ProcessNewScore(float scoreToProcess, string scoreOwner)
    {

        if (scoreToProcess > highScore)
        {
            highScore = scoreToProcess;

            currentOwner = scoreOwner;

            UpdateScoreText();
        }


    }

    float RandomScore()
    {

        return UnityEngine.Random.Range(localSkillFloor, localSkillCieling);

    }
    string RandomBoarder()
    {

        return localBoarderNames[(int)Mathf.Floor(UnityEngine.Random.value * localBoarderNames.Length)];

    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, spotRadius);
    }


    void UpdateScoreText()
    {


        if (textToEdit)
        {
            textToEdit.text = highScore.ToString();
            Debug.Log("SCORE UPDATED for " + spotName);
        }
        else
        {
            Debug.Log("SCORE UPDATED...No score text to edit for " + spotName);

        }



    }
}
