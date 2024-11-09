using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LocationCard : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text scoreText;

    [SerializeField] Color32 ownedColor;
    [SerializeField] Color32 unownedColor;

    Image card;

    private void Start()
    {
        card = GetComponent<Image>();
    }

    public void SetCard(string name)
    {
        nameText.text = name;
        scoreText.text = "0";
    }

    public void UpdateCard(int score)
    {
        scoreText.text = score.ToString();
    }

    public void SetOwned(bool owned)
    {
        if (owned) { card.color = ownedColor; }
        else { card.color = unownedColor; }
    }
}
