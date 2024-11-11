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
    void Awake()
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

    public void RumblePulse(float pulseLength, float pulseGap, Vector2 lowFrequency, Vector2 highFrequency)
    {
        StartCoroutine(Pulse(pulseLength, pulseGap, lowFrequency, highFrequency));
    }

    IEnumerator Pulse(float pulseLength, float gap, Vector2 lowFrequency, Vector2 highFrequency)
    {
        gamepad = Gamepad.current;
        gamepad.SetMotorSpeeds(lowFrequency.x, highFrequency.x);
        yield return new WaitForSeconds(pulseLength);
        gamepad.SetMotorSpeeds(0, 0);

        yield return new WaitForSeconds(gap);

        gamepad = Gamepad.current;
        gamepad.SetMotorSpeeds(lowFrequency.y, highFrequency.y);
        yield return new WaitForSeconds(pulseLength);
        gamepad.SetMotorSpeeds(0, 0);
    }

    IEnumerator RumbleTimer(float time, float lowFrequency, float highFrequency)
    {
        gamepad = Gamepad.current;
        gamepad.SetMotorSpeeds(lowFrequency, highFrequency);
        yield return new WaitForSeconds(time);
        gamepad.SetMotorSpeeds(0, 0);
    }
}
