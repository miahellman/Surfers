using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] float fovMultiplier = 2; // multiplier based on player's speed

    [Header("Outliner Materials")]
    [SerializeField] public Material[] outlineMats;

    Transform followPoint; // the one attached to the player
    Vector3 defaultFollowPointPos;

    float defaultFOV;
    float fovOffset;

    CameraFollowPoint followPointController;

    SurfController player;

    public static CameraControl instance;

    // Start is called before the first frame update
    void Start()
    {
        defaultFOV = GetComponent<Camera>().fieldOfView;
        followPointController = GetComponentInParent<CameraFollowPoint>();
        followPoint = followPointController.pointToFollow;
        defaultFollowPointPos = followPoint.localPosition;
        player = FindObjectOfType<SurfController>();

        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        //if (GameManager.instance.GetGameState() == GameManager.GameState.TUTORIAL) { return; }
        //fovOffset = followPointController.DistFromTarget() * fovMultiplier;
        fovOffset = (Mathf.Max(0, player.ForwardVelocity().magnitude) / 5) * fovMultiplier;
        GetComponent<Camera>().fieldOfView = defaultFOV + fovOffset;
    }

    // used for offsetting follow point from dead center on player
    // this does not need to be smoothed, because CameraFollowPoint will smooth the camera's movement to follow point
    // giving values of Vector3.zero will reset the position
    public void MovePointView(Vector3 offset, Vector3 rotOffset)
    {
        followPoint.localPosition = defaultFollowPointPos + offset;
        followPoint.localEulerAngles = rotOffset;
    }


}
