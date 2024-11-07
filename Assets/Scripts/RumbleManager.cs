using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RumbleManager : MonoBehaviour
{
    public static RumbleManager instance;

    Coroutine rumbleCo;
    Gamepad gamepad;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) { instance = this; }
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetButtonDown("Jump")) { SetRumbleActive(10, 10); }
        //if (Input.GetButtonUp("Jump")) { SetRumbleActive(false); }
    }

    public void SetRumbleActive(float lowFrequency, float highFrequency)
    {
        gamepad = Gamepad.current;
        if (gamepad == null) { return; }

        gamepad.SetMotorSpeeds(lowFrequency, highFrequency);
    }

    public void SetRumbleActive(bool active)
    {
        gamepad = Gamepad.current;
        if (gamepad == null) { return; }
        if (!active) { gamepad.SetMotorSpeeds(0, 0); }
    }

    public void RumbleForTime(float time, float lowFrequency, float highFrequency)
    {
        if (Gamepad.current == null) { return; }
        rumbleCo = StartCoroutine(RumbleTimer(time, lowFrequency, highFrequency));
    }

    IEnumerator RumbleTimer(float time, float lowFrequency, float highFrequency)
    {
        gamepad = Gamepad.current;
        gamepad.SetMotorSpeeds(lowFrequency, highFrequency);
        yield return new WaitForSeconds(time);
        gamepad.SetMotorSpeeds(0, 0);
    }
}
