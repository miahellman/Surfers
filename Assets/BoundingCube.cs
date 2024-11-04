using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BoundingCube : MonoBehaviour
{

    public Transform player;

    public float gizmoCubeOpacity;

    // Start is called before the first frame update
    void Start()
    {
        if (player == null)
        {
            player = GameObject.Find("Player").transform;
        
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(!CheckBounds())
        {
            Debug.Log("Player should warp now!");

            WarpPlayer();


        }
    }

    bool CheckBounds() {

        if (player == null) {
            player = GameObject.Find("Player").transform;
        }

        Vector3 upperCubePos = transform.position + transform.lossyScale * .5f;
        Vector3 lowerCubePos = transform.position - transform.lossyScale * .5f;

        Vector3 clampedPlayerPos = Vector3.Min( Vector3.Max( lowerCubePos, player.position), upperCubePos );

        if (player.position == clampedPlayerPos)
        {

            return true;
        }



        return false;
    
    }

    void WarpPlayer() {

        SpotInstance1[] spotsToWarpTo = FindObjectsOfType<SpotInstance1>();

        
        Vector3 closestPosition = Vector3.one * 88888888888888;
        float closestDist = Vector3.Distance(closestPosition, player.position);


        for (int i = 0; i < spotsToWarpTo.Length; i++)
        {
            Vector3 positionToCheck = spotsToWarpTo[i].transform.position;
            float distToCheck = Vector3.Distance(positionToCheck, player.position);
            if ( distToCheck < closestDist )
            {
                closestDist = distToCheck;
                closestPosition = positionToCheck;
            }
        }
        Debug.Log("Closest Dist:" + closestDist );
        Debug.Log("Closest Pos:" + closestPosition );
        Rigidbody playerRB = player.GetComponent<Rigidbody>(); 
        if (playerRB != null) { 
            playerRB.position = closestPosition;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1,0,0, gizmoCubeOpacity);



        if (CheckBounds()) {
            Gizmos.color = new Color(0, 1, 0, gizmoCubeOpacity);
        }
        Gizmos.DrawCube( transform.position, transform.lossyScale );   
    }

}
