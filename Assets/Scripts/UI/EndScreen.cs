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

    [SerializeField] string playAgainButton;
    [SerializeField] string menuButton;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.GetGameState() == GameManager.GameState.END)
        {
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
        container.SetActive(true);
        scoreText.text = score.ToString();

        for (int i = 0; i < spots.Length; i++)
        {
            spotTexts[i].text = spots[i].spotName;
            
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

    public void Close()
    {
        container.SetActive(false);
    }
}
