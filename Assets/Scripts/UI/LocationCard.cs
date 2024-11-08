using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LocationCard : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text scoreText;

    public void SetCard(string name)
    {
        nameText.text = name;
    }

    public void UpdateCard(int score)
    {
        scoreText.text = score.ToString();
    }
}
