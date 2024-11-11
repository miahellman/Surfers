using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardOrientator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.GetGameState() != GameManager.GameState.GAME || GameManager.instance.GetGameState() != GameManager.GameState.TUTORIAL) { return; }
        transform.rotation = Camera.main.transform.rotation;
    }
}
