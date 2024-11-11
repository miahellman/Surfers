using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour
{
    [SerializeField] TMP_Text scoreText;
    [SerializeField] GameObject container;
    [SerializeField] TMP_Text[] spotTexts;
    [SerializeField] TMP_Text[] spotScores;

    [SerializeField] string playAgainButton;
    [SerializeField] string menuButton;

    bool inputActive = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.GetGameState() == GameManager.GameState.END)
        {
            if (!inputActive) { return; }
            if (Input.GetButtonDown(menuButton))
            {
                GameManager.TransitionDelegate transition = RestartScene;
                GameManager.instance.Transition(transition);
            }
            if(Input.GetButtonDown(playAgainButton))
            {
                GameManager.TransitionDelegate transition = GameManager.instance.PlayAgain;
                GameManager.instance.Transition(transition);
            }
        }
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SetScreen(int score, SpotInstance1[] spots)
    {
        StartCoroutine(WaitAndSetInputActive());
        container.SetActive(true);
        //scoreText.text = score.ToString();

        for (int i = 0; i < spots.Length; i++)
        {
            spotTexts[i].text = spots[i].spotName;
            spotScores[i].text = Mathf.FloorToInt(spots[i].highScore).ToString();
            //// whether 'player' will be the name tbd
            //if (spots[i].currentOwner == "Player")
            //{
            //    spotTexts[i].color = Color.white;
            //}
            //else
            //{
            //    spotTexts[i].color = Color.red;
            //}
        }
    }

    IEnumerator WaitAndSetInputActive()
    {
        inputActive = false;
        yield return new WaitForSeconds(1);
        inputActive = true;
    }

    public void Close()
    {
        container.SetActive(false);
    }
}
