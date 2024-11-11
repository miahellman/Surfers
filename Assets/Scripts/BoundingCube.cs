using Fungus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class BoundingCube : MonoBehaviour
{

    public Transform player;

    public float gizmoCubeOpacity;

    public float distFromBounds;

    public float beginFadeDist = 20;

    public Image screenShroud;

    public float screenFadeProgress = 0;

    public Material fadeMaterial;


    // Start is called before the first frame update
    void Start()
    {
        if (player == null)
        {
            player = GameObject.Find("Player").transform;
        
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.GetGameState() == GameManager.GameState.MENU || GameManager.instance.GetGameState() == GameManager.GameState.INTRO) { return; }

        if (!CheckBounds())
        {
            Debug.Log("Player should warp now!");

            WarpPlayer();

            distFromBounds = 0;
        }
        else {

            float currentDist = GetDistFromBounds();

            if (distFromBounds < currentDist)
            {
                distFromBounds = Mathf.Lerp(distFromBounds, currentDist, .01f );
            }
            else
            {
                distFromBounds = currentDist;
            }

             
  
        }


        screenFadeProgress = Mathf.Pow( Mathf.InverseLerp(beginFadeDist,0, distFromBounds), .5f );


        //screenShroud.color = new Color(1, 1, 1, screenFadeProgress);

        fadeMaterial.SetFloat("_AnimProgress", screenFadeProgress);

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


    public float GetDistFromBounds()
    {

        //float smallestXDist = Mathf.Min(  ,  );

        //Vector3 testVector3 = new Vector3(1, 2, 3);

        //float testFloat = testVector3[1]; 

        float closestDist = 888888;

        Vector3 upperCubePos = transform.position + transform.lossyScale * .5f;
        Vector3 lowerCubePos = transform.position - transform.lossyScale * .5f;



        for (int i = 0; i < 3; i++)
        {
            float upperWallDist = Mathf.Abs(player.position[i] - upperCubePos[i]);
            float lowerWallDist = Mathf.Abs(player.position[i] - lowerCubePos[i]);

            float minDist = Mathf.Min(upperWallDist, lowerWallDist );

            if (minDist < closestDist)
            {
                closestDist = minDist;
            }

        }



        return closestDist;
    }

    public void WarpPlayer() {

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
