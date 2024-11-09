using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tricks;

public class BoardGraphics : MonoBehaviour
{
    [SerializeField] float rotSpeed = 540;
    [SerializeField] float rotError = 20; // how many angles off still counts as a success?
    [SerializeField] float adjustSpeed = 100;

    bool flipping = false;
    float flipDir;
    float flipValue;

    float rotValue;
    float targetRot;
    float adjustDir;

    public enum FlipState { FLIPPING, ADJUSTING, IDLE, FAILED };
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
                //rotValue += adjustDir * adjustSpeed * Time.deltaTime;
                //transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, rotValue);

                //if (Mathf.Abs(transform.localEulerAngles.z - 0) < 2f)
                //{
                //    flipState = FlipState.IDLE;
                //}

                rotValue = Mathf.LerpAngle(rotValue, adjustDir * targetRot, adjustSpeed * Time.deltaTime);
                //rotValue += adjustDir * 100 * Time.deltaTime;
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, rotValue);
                if (Mathf.Abs(rotValue - targetRot) <= 5)
                {
                    flipState = FlipState.IDLE;
                    rotValue = 0;
                }
                break;
            case FlipState.IDLE:
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0);
                rotValue = 0;
                break;
            case FlipState.FAILED:
                //if (FindObjectOfType<SurfController>().IsGrounded())
                //{
                //    flipState = FlipState.ADJUSTING;
                //}    
                break;
        }
    }

    IEnumerator ResetRotation()
    {
        flipState = FlipState.ADJUSTING;
        while (transform.localEulerAngles.z % 360 > 0.5f)
        {
            rotValue += adjustDir * adjustSpeed * Time.deltaTime;
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, rotValue);
            yield return null;
        }
        flipState = FlipState.IDLE;
    }

    IEnumerator FailAndReset()
    {
        yield return new WaitForSeconds(0.1f);
        flipState = FlipState.ADJUSTING;
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
        if (flipState == FlipState.ADJUSTING || flipState == FlipState.FAILED) { return; }

        // stop flip
        if (flipping && !active)
        {
            adjustDir = -Mathf.Sign(flipValue);
            targetRot = Mathf.Round(rotValue / 360) * 360; // nearest multiple of 360
            if (CheckSuccess())
            {
                int rotations = Mathf.Abs(Mathf.RoundToInt(targetRot / 360));
                Trick trick = TrickManager.Kickflip;
                ScoreManager.instance.ScoreTrick(trick, trick.baseScore * rotations);
                AudioManager.instance.PlaySound("kickflip stop");
            }
            else
            {
                AudioManager.instance.PlaySound("kickflip miss");
                FindObjectOfType<SurfController>().MissFlip();
            }

            //if (CheckSuccess())
            //{
            //    flipState = FlipState.ADJUSTING;
            //}
            //else
            //{
            //    flipState = FlipState.FAILED;
            //    StartCoroutine(FailAndReset());
            //}
            flipState = FlipState.ADJUSTING;
            //StartCoroutine(ResetRotation());
            flipping = false;
        }
        // start flip
        else if (!flipping && active) 
        {
            AudioManager.instance.PlaySound("kickflip start");
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
