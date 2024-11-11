using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    TMP_Text timerText;
    [SerializeField] GameState startingState = GameState.MENU;

    [SerializeField] GameObject camControl;
    [SerializeField] GameObject camFollow;

    [SerializeField] Transform restartPoint;
    [SerializeField] Image blackout;

    public delegate void TransitionDelegate();
    TransitionDelegate TransitionDel;

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

    private void Awake()
    {
        player = FindObjectOfType<SurfController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) { instance = this; }
        endScreen = FindObjectOfType<EndScreen>();
        mainMenu = FindObjectOfType<MainMenu>();
        tutorial = FindObjectOfType<TutorialManager>();


        timerText = ScoreManager.instance.timerText;

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
                    RumbleManager.instance.SetRumbleActive(false);
                    ScoreManager.instance.SetCanvasActive(false);
                    break;
                case GameState.INTRO:
                    RumbleManager.instance.SetRumbleActive(false);
                    tutorial.StartIntro();
                    break;
                case GameState.TUTORIAL:
                    foreach (SpotInstance1 location in ScoreManager.instance.GetLocations())
                    {
                        location.GetComponentInChildren<AudioSource>().Play();
                    }

                    CameraControl cam = camControl.GetComponentInChildren<CameraControl>();
                    Outliner outliner = camControl.AddComponent<Outliner>();
                    outliner.SetMats(cam.outlineMats[0], cam.outlineMats[1], cam.outlineMats[2], cam.outlineMats[3]);
                    //Debug.Log(player == null);
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
                    ScoreManager.instance.Set();
                    timerRunning = true;
                    timer = startTime;
                    break;
                case GameState.END:
                    AudioManager.instance.PlaySound("end boom", volume: 1f);
                    RumbleManager.instance.SetRumbleActive(false);
                    
                    foreach (SpotInstance1 location in ScoreManager.instance.GetLocations())
                    {
                        location.GetComponentInChildren<AudioSource>().Stop();
                    }

                    timerRunning = false;
                    endScreen.SetScreen(ScoreManager.instance.GetOverallScore(), ScoreManager.instance.GetLocations());
                    player.DisableInput(true);
                    break;

            }
        }
        stateUpdated = true;
    }

    public void PlayAgain()
    {
        foreach (SpotInstance1 location in ScoreManager.instance.GetLocations())
        {
            location.GetComponentInChildren<AudioSource>().Play();
        }
        player.transform.position = restartPoint.position;
        ScoreManager.instance.ClearScores();
        player.transform.localEulerAngles = new Vector3(0, 90, 0);
        UpdateState(GameState.GAME);
    }

    public void Transition(TransitionDelegate _delegate)
    {
        TransitionDel = _delegate;
        StartCoroutine(TransitionFade());
    }

    IEnumerator TransitionFade()
    {
        float alpha = 0;
        blackout.color = new Color(blackout.color.r, blackout.color.g, blackout.color.b, alpha);
        while (alpha < 1)
        {
            alpha += 1.5f * Time.deltaTime;
            blackout.color = new Color(blackout.color.r, blackout.color.g, blackout.color.b, alpha);
            yield return null;
        }
        blackout.color = new Color(blackout.color.r, blackout.color.g, blackout.color.b, 1);

        TransitionDel();

        while (alpha > 0)
        {
            alpha -= 1.5f * Time.deltaTime;
            blackout.color = new Color(blackout.color.r, blackout.color.g, blackout.color.b, alpha);
            yield return null;
        }
        blackout.color = new Color(blackout.color.r, blackout.color.g, blackout.color.b, 0);
    }

    public GameState GetGameState() { return gameState; }
}
