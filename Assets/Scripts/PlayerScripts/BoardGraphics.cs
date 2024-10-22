using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGraphics : MonoBehaviour
{
    [SerializeField] float rotSpeed = 540;
    [SerializeField] float rotError = 20; // how many angles off still counts as a success?

    bool flipping = false;
    float flipDir;
    float flipValue;

    float rotValue;
    float targetRot;
    float adjustDir;

    Animator anim;

    public enum FlipState { FLIPPING, ADJUSTING, IDLE };
    FlipState flipState = FlipState.IDLE;

    // Start is called before the first frame update
    void Start()
    {
        //anim = GetComponent<Animator>();
        rotValue = transform.localEulerAngles.z;
    }

    // Update is called once per frame
    void Update()
    {
        switch (flipState)
        {
            case FlipState.FLIPPING:
                rotValue += -Mathf.Sign(flipValue) * rotSpeed * Time.deltaTime;
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, rotValue);
                break;
            case FlipState.ADJUSTING:
                rotValue = Mathf.LerpAngle(rotValue, adjustDir * targetRot, 50 * Time.deltaTime);
                //rotValue += adjustDir * 100 * Time.deltaTime;
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, rotValue);
                if (rotValue == targetRot)
                {
                    flipState = FlipState.IDLE;
                }
                break;
        }
    }

    IEnumerator ResetRotation()
    {
        while (transform.localEulerAngles.z % 180 > 0)
        {
            yield return null;
        }
    }

    public void SetValue(float value)
    {
        flipValue = value;

        int intValue;
        if (value == 0) { intValue = 0; }
        else { intValue = Mathf.FloorToInt(Mathf.Sign(flipValue)); }
        //anim.SetInteger("FlipValue", intValue);
    }

    public void SetFlipActive(bool active)
    {
        // stop flip
        if (flipping && !active)
        {
            adjustDir = -Mathf.Sign(flipValue);
            flipState = FlipState.ADJUSTING;
            targetRot = Mathf.Round(rotValue / 360) * 360; // nearest multiple of 360
            CheckSuccess();
            flipping = false;
        }
        else if (!flipping && active) // start flip
        {
            flipState = FlipState.FLIPPING;
            flipping = true;
        }
    }

    public bool CheckSuccess()
    {
        bool a = transform.localEulerAngles.z >= 360 - rotError && transform.localEulerAngles.z <= 360;
        bool b = transform.localEulerAngles.z >= 0 && transform.localEulerAngles.z <= rotError;
        if (a || b)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public FlipState State()
    {
        return flipState;
    }

    //public void Flip(float value)
    //{
    //    anim.SetTrigger("Flip");
    //    print("test2");
    //    //flipping = true;
    //    //flipDir = Mathf.Sign(value);

    //    //anim.SetFloat("FlipDir", flipDir);
    //    //anim.SetBool("Flipping", flipping);
    //}

    //public void StopFlip()
    //{
    //    flipping = false;
    //    anim.SetBool("Flipping", flipping);
    //}
}
