using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] TMP_Text timerText;
    [SerializeField] GameState startingState = GameState.MENU;

    [SerializeField] GameObject camControl;
    [SerializeField] GameObject camFollow;

    [SerializeField] Transform restartPoint;

    public enum GameState { MENU, INTRO, TUTORIAL, GAME, PAUSED, END };
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
        if (instance == null) { instance = this; }
        endScreen = FindObjectOfType<EndScreen>();
        player = FindObjectOfType<SurfController>();
        mainMenu = FindObjectOfType<MainMenu>();
        tutorial = FindObjectOfType<TutorialManager>();

        UpdateState(startingState);
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
                        UpdateState(GameState.END);
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
                case GameState.INTRO:
                    tutorial.StartIntro();
                    break;
                case GameState.TUTORIAL:
                    CameraControl cam = camControl.GetComponentInChildren<CameraControl>();
                    Outliner outliner = cam.gameObject.AddComponent<Outliner>();
                    outliner.SetMats(cam.outlineMats[0], cam.outlineMats[1], cam.outlineMats[2], cam.outlineMats[3]);
                    player.outliner = outliner;

                    camControl.SetActive(true);
                    camFollow.SetActive(true);

                    mainMenu.CloseMenu();
                    mainMenu.enabled = false;
                    break;
                case GameState.GAME:
                    endScreen.Close();
                    player.DisableInput(false);
                    ScoreManager.instance.SetCanvasActive(true);
                    timerRunning = true;
                    timer = startTime;
                    break;
                case GameState.END:
                    timerRunning = false;
                    endScreen.SetScreen(ScoreManager.instance.GetOverallScore(), ScoreManager.instance.GetSpots());
                    player.DisableInput(true);
                    break;

            }
        }
        stateUpdated = true;
    }

    public void PlayAgain()
    {
        player.transform.position = restartPoint.position;
        ScoreManager.instance.ClearScores();
        UpdateState(GameState.GAME);
    }

    public GameState GetGameState() { return gameState; }
}
