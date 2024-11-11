using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class UpgradedCamera : MonoBehaviour
{

    //Let's get the player and their controller so that we can track their state and do some cool conditional stuff.
    public Transform playerTransform;
    SurfController controller;

    Transform zoomTransform;

    [Header("Wallride Behavior")]
    public float wallrideWallOffset = 8;
    public float wallrideCameraOffsetZ = 8;

    [Header("Grinding Behavior")]
    public float grindingCameraOffsetY = 0;
    public float grindingCameraOffsetZ = 4;
    public float grindingCameraOffsetX = 4;

    [Header("Airborne Behavior")]
    public float airborneCameraOffsetZ = 10;
    public float airborneCameraOffsetY = 0;
    public float airborneBehaviorGroundDistThreshold = 25;
    [SerializeField] LayerMask floorRaycastLayers;


    [Header("Grounded Behavior")]
    public float groundedCameraOffsetZ = 8;
    public float groundedCameraOffsetY = 8;




    [Header("Main Camera Focusing")]
    //The idea here is that we're going to have the camera focus on a spot slightly ahead of the player, which is determined by a few of the following factors!
    //This is the most straightforward one. We sound out a ray with this length, and the camera focuses on that ray's endpoint if the ray hits nothing
    public float focusRayLength = 8;
    public float camFocusLerpSpeed = .8f;

    float currentCamOffsetZ = 0;
    float currentCamOffsetY = 0;
    float currentCamOffsetX = 0;

    //The ray is coded to bounce and reflect off of walls and slanted floors, and the camera prioritize focusing on those reflected points instead.
    //This is how long the subsequent raycasts will be, when we reflect the original ray off of walls and ramps.
    //(we reflect it off ramps so that the focus point will be above the player, which is good!)
    public float afterBounceLength = 6;

    //This is just a placeholder until we have an actual focus point
    Vector3 focusPoint = Vector3.zero;

    //What do we want the rays to bounce off of?
    public LayerMask camReflectionLayerMask;


    public float angularVelocityMultiplier = 4f;


    Vector3 customUpDirection;


    // Start is called before the first frame update
    void Start()
    {
        zoomTransform = transform.GetChild(0);

        controller = playerTransform.GetComponent<SurfController>();
    }

    // Update is called once per frame
    void Update()
    {

        //Debug.Log(groundedCheck());


        switch (controller.GetMovementState())
        {
            case SurfController.MovementState.STANDARD:

                if (groundedCheck())
                {
                    groundedBehavior();
                }
                else
                {

                    airborneBehavior();
                }

                break;

            case SurfController.MovementState.WALLRIDE:

                wallrideBehavior();
                break;

            case SurfController.MovementState.GRIND:
                grindBehavior();
                break;





        }



        RaycastHit hit;

        //First we should get the focus point ahead of the player.
        Vector3 camFocusPoint = getReflectionRayEndpoint(playerTransform.position, playerTransform.forward, out hit, focusRayLength, camReflectionLayerMask);


        //We should also make sure the camera doesn't clip through the floor.
        RaycastHit floorHit;
        bool floorCast = Physics.Raycast(transform.position, -(Vector3.Scale(transform.forward, Vector3.one - Vector3.up)).normalized, out floorHit, currentCamOffsetZ + 1, camReflectionLayerMask);

        if (floorCast)
        {
            Debug.Log("ARE WE IN THE FLOOR!?!");

            Debug.Log(floorHit.collider.name);
        }



        //if (Vector2.Distance( new Vector2( playerTransform.position.x, playerTransform.position.x),  new Vector2( transform.position.x, transform.position.x) ) < currentCamOffsetZ )
        //{
            //transform.position = playerTransform.position + customUpDirection * currentCamOffsetY;
            Vector3 targetCamPos = playerTransform.position + customUpDirection * currentCamOffsetY;

            transform.position = Vector3.Lerp( transform.position, targetCamPos, .1f );


        //}



        //transform.position = playerTransform.position + playerTransform.right * currentCamOffsetX;

        zoomTransform.localPosition = -Vector3.forward * currentCamOffsetZ;

        if (camFocusPoint != playerTransform.position + playerTransform.forward * focusRayLength || floorCast || camFocusPoint.y < playerTransform.position.y || controller.GetMovementState() != SurfController.MovementState.STANDARD)
        {
            if (floorCast)
            {
                zoomTransform.localPosition = -Vector3.forward * floorHit.distance;
            }

            transform.LookAt(camFocusPoint, customUpDirection);


        }
        else
        {
            transform.LookAt(new Vector3(camFocusPoint.x, transform.position.y, camFocusPoint.z));

        }

        //And then we're also going to rotate the camera slightly more depending on angular velocity.

        //Debug.Log("Angular Velocity distance multiplier: " + ((Vector3.Distance(transform.position, zoomTransform.position) / cameraOffsetZ) - 1) );

        zoomTransform.localRotation = Quaternion.Euler( new Vector3( 0, playerTransform.GetComponent<Rigidbody>().angularVelocity.y * angularVelocityMultiplier * (Vector3.Distance( transform.position, zoomTransform.position)/currentCamOffsetZ )-1, 0 ) );

    }

    void groundedBehavior()
    {
        currentCamOffsetY = groundedCameraOffsetY;
        currentCamOffsetZ = groundedCameraOffsetZ;
        currentCamOffsetX = 0;

        customUpDirection = Vector3.up;
    }


    void airborneBehavior()
    {
        currentCamOffsetY = airborneCameraOffsetY;
        currentCamOffsetZ = airborneCameraOffsetZ;
        currentCamOffsetX = 0;

        customUpDirection = Vector3.up;
    }

    void wallrideBehavior()
    {

        //this is where we will need to offset the camera based on where the riden wall is.

        RaycastHit hit;

        if (Physics.Raycast(playerTransform.position, -playerTransform.up, out hit, airborneBehaviorGroundDistThreshold, floorRaycastLayers))
        {
            customUpDirection = hit.normal;
        }
        else
        {
            customUpDirection = transform.up;
        }
        currentCamOffsetY = wallrideWallOffset;
        currentCamOffsetZ = wallrideCameraOffsetZ;
        currentCamOffsetX = 0;

    }

    void grindBehavior()
    {

        currentCamOffsetY = grindingCameraOffsetY;
        currentCamOffsetZ = grindingCameraOffsetZ;
        currentCamOffsetX = grindingCameraOffsetX;
    }


    bool groundedCheck()
    {


        //return Physics.Raycast( playerTransform.position, -playerTransform.up, out hit, controller.groundedDist ) )

        RaycastHit hit;

        if (controller.IsGrounded() || Physics.Raycast(playerTransform.position, -playerTransform.up, out hit, airborneBehaviorGroundDistThreshold, floorRaycastLayers))
        {
            return true;
        }

        return false;
    }






    private void OnDrawGizmos()
    {

        RaycastHit hit;

        Vector3 camFocusPoint = getReflectionRayEndpoint(playerTransform.position, playerTransform.forward, out hit, focusRayLength, camReflectionLayerMask);

        Gizmos.DrawSphere(camFocusPoint, 0.5f);



    }


    Vector3 getReflectionRayEndpoint(Vector3 rayStartPoint, Vector3 rayDirection, out RaycastHit hit, float rayLength, LayerMask layerMask)
    {

        Debug.DrawRay(rayStartPoint, rayDirection * rayLength, Color.red);

        Vector3 endPoint = rayStartPoint + rayDirection * rayLength;


        RaycastHit endingHit;




        //We're going to make it so that the ray keeps bouncing off walls until it can't reach any more walls. 
        int maxBounces = 4;
        int bounces = 0;
        while (Physics.Raycast(rayStartPoint, rayDirection, out endingHit, rayLength, layerMask) && bounces < maxBounces)
        {
            Debug.DrawRay(rayStartPoint, rayDirection * endingHit.distance, Color.red);

            //Debug.Log("The ray bounced!");

            bounces++;

            rayStartPoint = endingHit.point;

            rayDirection = Vector3.Reflect(rayDirection, endingHit.normal);

            rayLength = afterBounceLength;

            endPoint = rayStartPoint + rayDirection * rayLength;

            Debug.DrawRay(rayStartPoint, rayDirection * rayLength, Color.red);


        }

        hit = endingHit;




        return endPoint;


    }



}

