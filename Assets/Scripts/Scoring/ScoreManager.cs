using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tricks;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] float maxDownTime = 3; // how much time you have to do another trick before set is over
    [SerializeField] float setBonusPercent = 0.15f;

    [Header("UI")]
    [SerializeField] TMP_Text trickNameText;
    [SerializeField] TMP_Text durTrickText;
    [SerializeField] TMP_Text overallScoreText;
    [SerializeField] float fadeRate = 0.75f;
    [Tooltip("Bottom to top (0 is newest trick)")][SerializeField] TMP_Text[] trickTexts;

    //List<TrickListItem> trickList = new List<TrickListItem>();

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
    int setScore; // current running set
    bool setActive = false;
    Trick activeTrick;
    float activeTrickTime;

    Coroutine resetCo;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        durTrickText.text = " ";
        overallScoreText.text = overallScore.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (activeTrick != null)
        {
            activeTrickTime += Time.deltaTime;

            durTrickText.text = string.Format("{0} {1:0.00}", activeTrick.trickName, activeTrickTime);
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
        activeTrick = type;
    }

    public void StopTrick()
    {
        if (activeTrick == null) { Debug.LogWarning("Attempting to stop trick; no trick active"); return; }
        ScoreTrick(activeTrick, Mathf.FloorToInt(activeTrick.baseScore * activeTrickTime));
        activeTrick = null;
        activeTrickTime = 0;
        durTrickText.text = " ";
    }

    public void ScoreTrick(Trick type, int value)
    {
        if (resetCo != null) { StopCoroutine(resetCo); }
        resetCo = StartCoroutine(ResetSetTimer());
        //trickNameText.text = type.trickName;
        setScore += value;
        overallScore += value;
        overallScoreText.text = overallScore.ToString();

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

    void ScoreSet()
    {
        int setBonus = Mathf.FloorToInt(setScore * setBonusPercent);
        overallScore += setBonus;
        overallScoreText.text = overallScore.ToString();
        setScore = 0;
    }
}
