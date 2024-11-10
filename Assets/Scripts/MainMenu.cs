using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField] RectTransform topBar;
    [SerializeField] RectTransform bottomBar;
    [SerializeField] Image[] imagesToFade;

    [SerializeField] string startButtonName = "Jump";
    [SerializeField] string quitButtonName = "Wallride";

    [SerializeField] Camera menuCam;

    TMP_Text[] texts;

    bool started = false;

    // Start is called before the first frame update
    void Start()
    {
        texts = GetComponentsInChildren<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown(startButtonName) && !started)
        {
            TransitionToTutorial();
        }

        if (Input.GetButtonDown(quitButtonName))
        {
            Application.Quit();
        }
    }

    public void TransitionToTutorial()
    {
        StartCoroutine(MoveBar());
        started = true;
    }

    IEnumerator MoveBar()
    {
        float alpha = 1;
        while (alpha > 0)
        {
            alpha -= 1.5f * Time.deltaTime;
            foreach (TMP_Text text in texts)
            {
                Color color = text.color;
                text.color = new Color(color.r, color.g, color.b, alpha);
            }
            foreach (Image image in imagesToFade)
            {
                Color color = image.color;
                image.color = new Color(color.r, color.g, color.b, alpha);
            }
            yield return null;
        }

        float yValue = topBar.anchoredPosition.y;
        float moveRate = 250;
        while (yValue < 676)
        {
            //yValue = Mathf.Lerp(yValue, 680, 150 * Time.deltaTime);
            //topBar.anchoredPosition = new Vector2(topBar.anchoredPosition.x, yValue);
            //bottomBar.anchoredPosition = new Vector3(bottomBar.anchoredPosition.x, -yValue);
            moveRate -= 110 * Time.deltaTime;
            moveRate = Mathf.Max(0, moveRate);
            yValue += moveRate * Time.deltaTime;
            topBar.anchoredPosition = new Vector2(topBar.anchoredPosition.x, yValue);
            bottomBar.anchoredPosition = new Vector2(bottomBar.anchoredPosition.x, -yValue);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        GameManager.instance.UpdateState(GameManager.GameState.INTRO);
    }

    public void CloseMenu()
    {
        menuCam.gameObject.SetActive(false);
    }
}
