using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialZone : MonoBehaviour
{
    [SerializeField] string zoneName;

    TutorialManager manager;

    bool entered = false;

    // Start is called before the first frame update
    void Start()
    {
        manager = GetComponentInParent<TutorialManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<SurfController>() != null && !entered)
        {
            manager.TriggerZone(zoneName + " zone");
            entered = true;
        }
    }
}
