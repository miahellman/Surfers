using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
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

    public void StartTutorial()
    {
        Fungus.Flowchart.BroadcastFungusMessage("start");
    }

    public void TriggerZone(string zoneName)
    {
        if (zoneName == "end")
        {
            tutorialComplete = true;
        }
        else
        {
            Fungus.Flowchart.BroadcastFungusMessage(zoneName);
        }
    }

    public void Unfreeze()
    {
        TimeControl.instance.SetFreeze(false, true, true);
    }

    public void SetPlayerActive(bool active, bool tutorial)
    {
        player.SetActive(active);
        dummyPlayer.SetActive(false);

        if (!tutorial)
        {
            foreach (TutorialZone zone in zones)
            {
                Destroy(zone);
            }
        }
    }
}
