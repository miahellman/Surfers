using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tricks;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] float maxDownTime = 3; // how much time you have to do another trick before set is over
    [SerializeField] SpotInstance1[] spots;
    [SerializeField] float[] multiplierLevels;
    [SerializeField] int[] multiplierThresholds;

    [Header("UI")]
    [SerializeField] GameObject canvas;
    [SerializeField] TMP_Text durTrickText;
    [SerializeField] TMP_Text setScoreText;
    [SerializeField] TMP_Text locationScoreText;
    [SerializeField] Image multiplierRingFill;
    [SerializeField] TMP_Text multiplierText;
    [SerializeField] TMP_Text wallrideLeftText;
    [SerializeField] TMP_Text wallrideRightText;
    [SerializeField] float fillSpeed = 0.75f;
    //[SerializeField] TMP_Text overallScoreText;
    [SerializeField] float fadeRate = 0.75f;
    [Tooltip("Bottom to top (0 is newest trick)")][SerializeField] TMP_Text[] trickTexts;

    TrickListItem[] trickList = { null, null, null };

    public class TrickListItem
    {
        public string name;
        public float alpha;

        public TrickListItem(string trickName, float defaultAlpha)
        {
            name = trickName;
            alpha = defaultAlpha;
        }   
    }

    public static ScoreManager instance;

    int overallScore;
    int highScore;
    int currentLocationScore; // score for location player is currently in
    int setScore; // current running set
    int setTricks; // number of tricks done in this set, used for multiplier
    bool setActive = false;
    Trick activeTrick;
    float activeTrickTime;
    int multiplierIndex = 0;
    float targetRingFill;
    TMP_Text activeText;

    Coroutine resetCo;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null) { instance = this; }
        durTrickText.text = "";
        wallrideLeftText.text = "";
        wallrideRightText.text = "";
        locationScoreText.text = currentLocationScore.ToString();
        setScoreText.text = setScore.ToString();
        //overallScoreText.text = overallScore.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (activeTrick != null)
        {
            activeTrickTime += Time.deltaTime;

            activeText.text = string.Format("{0} {1:0.00}", activeTrick.trickName, activeTrickTime);
        }

        for (int i = 0; i < trickList.Length; i++)
        {
            if (trickList[i] != null)
            {
                trickTexts[i].text = trickList[i].name;
                //trickList[i].alpha -= fadeRate * Time.deltaTime;

                Color color = trickTexts[i].color;
                trickTexts[i].color = new Color(color.r, color.g, color.b, trickList[i].alpha);
            }
            else
            {
                Color color = trickTexts[i].color;
                trickTexts[i].color = new Color(color.r, color.g, color.b, 0);
            }
        }

        multiplierRingFill.fillAmount = Mathf.MoveTowards(multiplierRingFill.fillAmount, targetRingFill, fillSpeed * Time.deltaTime);
    }

    public void StartTrick(Trick type)
    {
        if (!type.durationTrick) { Debug.LogWarning(type.trickName + " is not a duration trick. Use ScoreTrick() instead."); }
        if (!setActive) { setActive = true; }
        if (resetCo != null) { StopCoroutine(resetCo); }
        if (activeTrick != null)
        {
            // stop current trick, start new one
            StopTrick();
        }
        activeText = durTrickText;
        activeTrick = type;
    }

    public void StartTrick(Trick type, bool left)
    {
        if (!type.durationTrick) { Debug.LogWarning(type.trickName + " is not a duration trick. Use ScoreTrick() instead."); }
        if (resetCo != null) { StopCoroutine(resetCo); }
        if (activeTrick != null)
        {
            // stop current trick, start new one
            StopTrick();
        }
        if (left) { activeText = wallrideLeftText; }
        else { activeText = wallrideRightText; }
        activeTrick = type;
    }

    public void StopTrick()
    {
        if (activeTrick == null) { Debug.LogWarning("Attempting to stop trick; no trick active"); return; }
        ScoreTrick(activeTrick, Mathf.FloorToInt(activeTrick.baseScore * activeTrickTime));
        activeTrick = null;
        activeTrickTime = 0;
        activeText.text = "";
        activeText = null;
    }

    public void ScoreTrick(Trick type, int value)
    {
        if (resetCo != null) { StopCoroutine(resetCo); }
        resetCo = StartCoroutine(ResetSetTimer());
        //trickNameText.text = type.trickName;
        StartCoroutine(AddToNumber(setScore, setScore += value));
        setScore += value;
        overallScore += value;
        //overallScoreText.text = overallScore.ToString();
        setTricks++;

        //print(multiplierIndex.ToString() + ", " + (multiplierThresholds.Length - 1).ToString());
        if (multiplierThresholds.Length - 1 > multiplierIndex)
        {
            int nextThreshold = multiplierThresholds[multiplierIndex + 1];
            if (setTricks >= nextThreshold)
            {
                multiplierIndex++;
                setTricks = 0;
                multiplierText.text = multiplierLevels[multiplierIndex].ToString() + "x";
            }
            targetRingFill = (float)setTricks / (float)multiplierThresholds[multiplierIndex + 1];
        }
        else if (multiplierIndex == multiplierThresholds.Length - 1)
        {
            multiplierText.text = multiplierLevels[multiplierIndex].ToString() + "x";
            targetRingFill = (float)setTricks / (float)multiplierThresholds[multiplierIndex];
        }

        TrickListItem newItem = new TrickListItem(type.trickName, 1);

        if (trickList[0] != null)
        {
            for (int i = trickTexts.Length; i > 0 ; i--)
            {
                // move items up
                TrickListItem previousItem = trickList[i - 1];
                if (i < trickTexts.Length) { trickList[i] = previousItem; }
            }
            trickList[0] = newItem;
            trickTexts[0].GetComponent<Animation>().Play();
        }
        else
        {
            trickList[0] = newItem;
            trickTexts[0].GetComponent<Animation>().Play();
        }
    }

    IEnumerator ResetSetTimer()
    {
        yield return new WaitForSeconds(maxDownTime);
        setActive = false;
        ScoreSet();
    }

    public void SetCanvasActive(bool active)
    {
        canvas.SetActive(active);
    }

    public void ForceStopSet()
    {
        StopCoroutine(resetCo);
        setActive = false;
        ScoreSet();
    }

    void ScoreSet()
    {
        int setBonus = Mathf.FloorToInt(setScore * (1 - multiplierLevels[multiplierIndex]));
        currentLocationScore += setBonus + setScore;
        overallScore += setBonus;
        locationScoreText.text = currentLocationScore.ToString();
        setScoreText.text = "0";
        //overallScoreText.text = overallScore.ToString();
        setScore = 0;
        setTricks = 0;
        multiplierIndex = 0;
        targetRingFill = 0;
        multiplierText.text = multiplierLevels[multiplierIndex].ToString() + "x";
    }

    IEnumerator AddToNumber(int startScore, int newScore)
    {
        int displayScore = startScore;
        while (displayScore < newScore)
        {
            displayScore += 10;
            displayScore = Mathf.Min(displayScore, newScore);
            setScoreText.text = displayScore.ToString();
            yield return new WaitForSecondsRealtime(0.06f);
        }
    }

    public void ClearScores()
    {
        overallScore = 0;
        setScore = 0;
        currentLocationScore = 0;
    }

    public int GetOverallScore()
    {
        return overallScore;
    }

    public SpotInstance1[] GetSpots()
    {
        return spots;
    }
}
