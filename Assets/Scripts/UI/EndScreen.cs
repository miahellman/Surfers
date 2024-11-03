using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EndScreen : MonoBehaviour
{
    [SerializeField] TMP_Text scoreText;
    [SerializeField] GameObject container;
    [SerializeField] TMP_Text[] spotTexts;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
}
