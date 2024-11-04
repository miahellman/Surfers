using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeControl : MonoBehaviour
{
    public static TimeControl instance;

    float slowRate = 1f;

    float defaultTimeScale;
    float defaultFixedDeltaTime;

    bool frozen = false; // seems good to keep track of this

    SurfController player;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        player = FindObjectOfType<SurfController>();
        defaultTimeScale = Time.timeScale;
        defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    void Update()
    {
        Time.timeScale = Mathf.Clamp(Time.timeScale, 0, defaultTimeScale);
        Time.fixedDeltaTime = Mathf.Clamp(Time.fixedDeltaTime, 0, defaultFixedDeltaTime);

        //if (Input.GetKeyDown(KeyCode.J))
        //{
        //    if (frozen) { SetFreeze(false, true); }
        //    else { SetFreeze(true, true); }
        //}
    }

    public void ChangeTime(float newScale, bool playerOnly)
    {
        if (playerOnly)
        {
            FindObjectOfType<SurfController>().SetLocalTimeScale(newScale);
        }
        else
        {
            Time.timeScale = newScale;
            Time.fixedDeltaTime = defaultFixedDeltaTime * newScale;
        }
    }

    public void ResetTime(bool playerOnly)
    {
        if (playerOnly)
        {
            FindObjectOfType<SurfController>().SetLocalTimeScale(1);
        }
        else
        {
            Time.timeScale = defaultTimeScale;
            Time.fixedDeltaTime = defaultFixedDeltaTime;
        }
    }

    // instant immediately freezes/unfreezes
    public void SetFreeze(bool _frozen, bool instant, bool playerOnly)
    {
        frozen = _frozen;
        if (instant)
        {
            if (frozen)
            {
                if (playerOnly) { player.enabled = false; player.GetComponent<Rigidbody>().isKinematic = true; }
                else { Time.timeScale = 0; }
            }
            else
            {
                if (playerOnly) { player.enabled = true; player.GetComponent<Rigidbody>().isKinematic = false; }
                else { Time.timeScale = defaultTimeScale; } 
            }
        }
        else
        {
            // not working right :(
            //if (frozen) { StartCoroutine(SlowFreezeTimer()); }
            //else { StartCoroutine(UnfreezeTimer()); }
        }
    }


    // because the time scale is changing, so is Time.deltaTime, causing the speed rate to get smaller and smaller, never actually reaching 0
    IEnumerator SlowFreezeTimer()
    {
        while (Time.timeScale > 0)
        {
            Time.timeScale -= slowRate * Time.deltaTime;
            Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;
            yield return null;
        }
        Time.timeScale = 0;
        Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;
    }

    IEnumerator UnfreezeTimer()
    {
        while (Time.timeScale < defaultTimeScale)
        {
            print(Time.timeScale);
            if (Time.timeScale == 0) { Time.timeScale = 0.01f; }
            else { Time.timeScale += slowRate * Time.deltaTime; }
            //Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;
            yield return null;
        }
        Time.timeScale = defaultTimeScale;
        Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;
    }
}
