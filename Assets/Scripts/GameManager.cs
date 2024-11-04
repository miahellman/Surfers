using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] TMP_Text timerText;

    public enum GameState { MENU, TUTORIAL, GAME, PAUSED, GAME_OVER };
    GameState gameState;

    public static GameManager instance;

    [SerializeField] float startTime = 180; // seconds
    float timer;
    bool timerRunning = false;

    bool stateUpdated = false;

    EndScreen endScreen;
    MainMenu mainMenu;
    SurfController player;
    TutorialManager tutorial;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        endScreen = FindObjectOfType<EndScreen>();
        player = FindObjectOfType<SurfController>();
        mainMenu = FindObjectOfType<MainMenu>();
        tutorial = FindObjectOfType<TutorialManager>();

        UpdateState(GameState.MENU);
    }

    // Update is called once per frame
    void Update()
    {
        switch (gameState)
        {
            case GameState.GAME:
                if (timerRunning)
                {
                    if (timer <= 0)
                    {
                        UpdateState(GameState.GAME_OVER);
                        timerText.text = "0:00";
                    }
                    else
                    {
                        timer -= Time.deltaTime;
                        float minutes = Mathf.Floor(timer / 60);
                        float seconds = timer % 60;
                        timerText.text = string.Format("{0}:{1:00}", minutes, seconds);
                    }
                }
                break;
        }
    }

    public void UpdateState(GameState newState)
    {
        gameState = newState;
        stateUpdated = false;

        if (!stateUpdated)
        {
            switch(gameState)
            {
                case GameState.MENU:
                    ScoreManager.instance.SetCanvasActive(false);
                    break;
                case GameState.TUTORIAL:
                    tutorial.StartTutorial();
                    mainMenu.enabled = false;
                    break;
                case GameState.GAME:
                    ScoreManager.instance.SetCanvasActive(true);
                    timerRunning = true;
                    timer = startTime;
                    break;
                case GameState.GAME_OVER:
                    timerRunning = false;
                    endScreen.SetScreen(ScoreManager.instance.GetOverallScore(), ScoreManager.instance.GetSpots());
                    player.DisableInput();
                    break;

            }
        }
        stateUpdated = true;
    }

    public GameState GetGameState() { return gameState; }
}
