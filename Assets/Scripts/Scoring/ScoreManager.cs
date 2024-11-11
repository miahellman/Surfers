using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tricks;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] float maxDownTime = 3; // how much time you have to do another trick before set is over
    [SerializeField] SpotInstance1[] locations;
    [SerializeField] float[] multiplierLevels;
    [SerializeField] int[] multiplierThresholds;

    SpotInstance1 currentLocation;
    int locationIndex;

    [Header("UI")]
    [SerializeField] GameObject canvas;
    [SerializeField] public TMP_Text timerText;
    [SerializeField] TMP_Text locationNameText;
    [SerializeField] TMP_Text durTrickText;
    [SerializeField] TMP_Text setScoreText;
    [SerializeField] TMP_Text locationScoreText;
    [SerializeField] Image multiplierRingFill;
    [SerializeField] TMP_Text multiplierText;
    [SerializeField] TMP_Text wallrideLeftText;
    [SerializeField] TMP_Text wallrideRightText;
    [SerializeField] float multiplierFillSpeed = 0.75f;
    [SerializeField] LocationCard[] locationCards;
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

    bool transitioningLevels = false;

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
        locationNameText.text = "";

        if (locations.Length > 0)
        {
            for (int i = 0; i < locationCards.Length; i++)
            {
                locationCards[i].SetCard(locations[i].spotName);
            }
        }
        //overallScoreText.text = overallScore.ToString();
    }

    public void Set()
    {
        durTrickText.text = "";
        wallrideLeftText.text = "";
        wallrideRightText.text = "";
        locationScoreText.text = currentLocationScore.ToString();
        setScoreText.text = setScore.ToString();
        locationNameText.text = "";

        if (locations.Length > 0)
        {
            for (int i = 0; i < locationCards.Length; i++)
            {
                locationCards[i].SetCard(locations[i].spotName);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (activeTrick != null)
        {
            activeTrickTime += Time.deltaTime;

            activeText.text = string.Format("{0} {1:0}", activeTrick.trickName, activeTrick.baseScore* activeTrickTime);
            //activeText.text = string.Format("{0} {1:0.00}", activeTrick.trickName, activeTrickTime);
        }

        for (int i = 0; i < trickList.Length; i++)
        {
            if (trickList[i] != null)
            {
                trickTexts[i].text = trickList[i].name;
                //trickList[i].alpha -= fadeRate * Time.deltaTime;

                Color color = trickTexts[i].color;
                //trickTexts[i].color = new Color(color.r, color.g, color.b, trickList[i].alpha);
            }
            else
            {
                trickTexts[i].text = "";
                Color color = trickTexts[i].color;
                //trickTexts[i].color = new Color(color.r, color.g, color.b, 0);
            }
        }

        if (transitioningLevels) { return; }
        multiplierRingFill.fillAmount = Mathf.MoveTowards(multiplierRingFill.fillAmount, targetRingFill, multiplierFillSpeed * Time.deltaTime);
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
        RumbleManager.instance.RumbleForTime(0.2f, 0, 1);

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
                StartCoroutine(NextRingLevel());
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

    IEnumerator NextRingLevel()
    {
        transitioningLevels = true;
        multiplierRingFill.fillClockwise = false;
        float fill = 1;
        while (fill > 0)
        {
            fill -= 1.25f * Time.deltaTime;
            multiplierRingFill.fillAmount = fill;
            yield return null;
        }
        multiplierRingFill.fillAmount = 0;
        multiplierIndex++;
        multiplierText.text = multiplierLevels[multiplierIndex].ToString() + "x";
        setTricks = 0;
        multiplierRingFill.fillClockwise = true;
        targetRingFill = 0;
        transitioningLevels = false;
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
        Set();
    }

    public void ForceStopSet()
    {
        if (resetCo != null) { StopCoroutine(resetCo); }
        setActive = false;
        ScoreSet();
    }

    void ScoreSet()
    {
        int setBonus = Mathf.FloorToInt(setScore * (1 - multiplierLevels[multiplierIndex]));
        Debug.Log(setBonus + setScore);
        currentLocationScore += setBonus + setScore;
        overallScore += setBonus;
        locationScoreText.text = currentLocationScore.ToString();
        setScoreText.text = "0";
        //overallScoreText.text = overallScore.ToString();
        setScore = 0;
        setTricks = 0;
        multiplierIndex = 0;
        multiplierText.text = multiplierLevels[multiplierIndex].ToString() + "x";
        targetRingFill = 0;
    }

    public void SetCurrentLocation(SpotInstance1 location)
    {
        currentLocation = location;
        locationNameText.text = location.spotName.ToUpper();
        locationNameText.GetComponent<Animation>().Play();
        AudioManager.instance.PlaySound("location", volume: 0.8f);
        for (int i = 0; i < locations.Length; i++)
        {
            if (locations[i] == location)
            {
                locationIndex = i;
            }
        }
    }

    public int ScoreLocation()
    {
        ForceStopSet();
        if (currentLocation != null)
        {
            if (currentLocation.highScore < currentLocationScore) { locationCards[locationIndex].UpdateCard(currentLocationScore); }
            currentLocation.ProcessNewScore(currentLocationScore, "Player");
            locationNameText.text = "";

            //locationCards[locationIndex].SetOwned(currentLocation.currentOwner == "Player");
            currentLocationScore = 0;
            locationScoreText.text = "0";
            currentLocation = null;
        } 
        else
        {
            Debug.LogWarning("Current location is null, most likely overlapping locations");
        }
        
        return currentLocationScore;
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
        multiplierIndex = 0;
        multiplierText.text = multiplierLevels[multiplierIndex].ToString() + "x";
        targetRingFill = 0;
    }

    public int GetOverallScore()
    {
        return overallScore;
    }

    public SpotInstance1[] GetLocations()
    {
        return locations;
    }
}
