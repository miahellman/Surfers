using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrindObject : Interactable
{
    [Header("Grind Directions")]
    [SerializeField] public bool forward = false;
    [SerializeField] public bool backward = false;
    [SerializeField] public bool right = false;
    [SerializeField] public bool left = false;
    [SerializeField] public bool up = false;
    [SerializeField] public bool down = false;

    [HideInInspector] public List<Vector3> grindDirections = new List<Vector3>();

    bool colorHighlighted = false;
    Color originColor;

    // Start is called before the first frame update
    void Start()
    {
        originColor = GetComponent<Renderer>().material.color;

        if (forward) { grindDirections.Add(transform.forward); }
        if (backward) { grindDirections.Add(-transform.forward); }
        if (right) { grindDirections.Add(transform.right); }
        if (left) { grindDirections.Add(-transform.right); }
        if (up) { grindDirections.Add(transform.up); }
        if (down) { grindDirections.Add(-transform.up); }
    }

    private void OnDrawGizmosSelected()
    {
        List<Vector3> tempDir = new List<Vector3>();
        if (forward) { tempDir.Add(transform.forward); }
        if (backward) { tempDir.Add(-transform.forward); }
        if (right) { tempDir.Add(transform.right); }
        if (left) { tempDir.Add(-transform.right); }
        if (up) { tempDir.Add(transform.up); }
        if (down) { tempDir.Add(-transform.up); }

        if (tempDir.Count > 0)
        {
            foreach (Vector3 dir in tempDir)
            {
                Debug.DrawRay(transform.position, dir * 10f, Color.blue);
            }
        }
    }

    public void HighlightColor(bool value)
    {
        colorHighlighted = value;
        if (colorHighlighted) { GetComponent<Renderer>().material.color = Color.red; }
        else { GetComponent<Renderer>().material.color = originColor; }
    }
}
