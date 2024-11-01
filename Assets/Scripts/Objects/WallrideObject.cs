using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallrideObject : Interactable
{
    [Header("Ridable Normals")]
    [SerializeField] public bool forward = false;
    [SerializeField] public bool backward = false;
    [SerializeField] public bool right = false;
    [SerializeField] public bool left = false;
    [SerializeField] public bool up = false;
    [SerializeField] public bool down = false;

    [HideInInspector] public List<Vector3> ridableNormals = new List<Vector3>();

    bool colorHighlighted = false;
    Color originColor;

    // Start is called before the first frame update
    void Start()
    {
        if (forward) { ridableNormals.Add(transform.forward); }
        if (backward) { ridableNormals.Add(-transform.forward); }
        if (right) { ridableNormals.Add(transform.right); }
        if (left) { ridableNormals.Add(-transform.right); }
        if (up) { ridableNormals.Add(transform.up); }
        if (down) { ridableNormals.Add(-transform.up); }
    }

    // Update is called once per frame
    void Update()
    {
        
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
                Debug.DrawRay(transform.position, dir * 5f, Color.blue);
            }
        }
    }

    public void HighlightColor(bool value)
    {
        //colorHighlighted = value;
        //if (colorHighlighted) { GetComponent<Renderer>().material.color = Color.red; }
        //else { GetComponent<Renderer>().material.color = originColor; }
    }

    public bool CheckValidNormal(Vector3 normal)
    {
        return ridableNormals.Contains(normal);
    }
}
