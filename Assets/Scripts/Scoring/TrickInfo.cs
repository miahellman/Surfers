using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TrickInfo : MonoBehaviour
{
    [SerializeField] public string trickName;
    [SerializeField] public bool duration; // is this a trick that scores based on duration?

    bool trickActive = false;
    float trickTime = 0.0f;

    private void Update()
    {
        if (trickActive) { trickTime += Time.deltaTime; }
        else { trickTime = 0.0f; }
    }

    public virtual void StartTrick() { trickActive = true; }
    public virtual void StopTrick() { trickActive = false; }
    public abstract int ScoreTrick();
}
