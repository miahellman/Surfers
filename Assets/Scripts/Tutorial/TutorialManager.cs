using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] AudioSource audio;
    [SerializeField] TutorialZone[] zones;
    [SerializeField] GameObject dummyPlayer;

    bool tutorialComplete = false;

    GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<SurfController>().gameObject;
        player.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartIntro()
    {
        Fungus.Flowchart.BroadcastFungusMessage("start");
    }

    public void TriggerZone(string zoneName)
    {
        if (zoneName == "end zone")
        {
            tutorialComplete = true;
            GameManager.instance.UpdateState(GameManager.GameState.GAME);
        }
        else
        {
            Fungus.Flowchart.BroadcastFungusMessage(zoneName);
            //TimeControl.instance.SetFreeze(true, true, true);
            TimeControl.instance.ChangeTime(0.06f, true);
            player.GetComponent<SurfController>().DisableInput(true);
        }
    }

    public void Unfreeze()
    {
        //TimeControl.instance.SetFreeze(false, true, true);
        TimeControl.instance.ResetTime(true);
        player.GetComponent<SurfController>().DisableInput(false);
    }

    public void SetPlayerActive(bool active, bool tutorial)
    {
        player.SetActive(active);
        dummyPlayer.SetActive(false);
        audio.Play();

        foreach (TutorialZone zone in zones)
        {
            Destroy(zone);
        }
    }
}
