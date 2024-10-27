using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tricks;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] float maxDownTime = 3; // how much time you have to do another trick before set is over
    [SerializeField] float setMultiplier = 1.15f;

    [SerializeField] TMP_Text trickNameText;

    public static ScoreManager instance;

    int overallScore;
    int highScore;
    int setScore; // current running set
    bool setActive = false;
    Trick activeTrick;

    Coroutine resetCo;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartTrick(Trick type)
    {
        if (!setActive) { setActive = true; }
        if (resetCo != null) { StopCoroutine(resetCo); }
        activeTrick = type;
    }

    public void StopTrick()
    {
        StartCoroutine(ResetSetTimer());
        activeTrick = null;
    }

    public void ScoreTrick(Trick type, int value)
    {
        if (resetCo != null) { StopCoroutine(resetCo); }
        StartCoroutine(ResetSetTimer());
        trickNameText.text = type.trickName;
        setScore += value;
    }

    IEnumerator ResetSetTimer()
    {
        yield return new WaitForSeconds(maxDownTime);
        setActive = false;
        ScoreSet();
    }

    void ScoreSet()
    {
        setScore = Mathf.FloorToInt(setScore * setMultiplier);
        overallScore = setScore;
        setScore = 0;
    }
}
