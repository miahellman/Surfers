using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CollisionOutline : MonoBehaviour
{
    [Header("Object Outline & Shader Materials")]
    //outline object
    [SerializeField] Renderer OutlinedObject;
    //materials for outline vs no outline
    [SerializeField] Material WriteObject;
    [SerializeField] Material ApplyOutline;

    [Header("Player Transform & Raycasts")]
    [SerializeField] Transform playerTransform; // Reference to the player's transform
    [SerializeField] float raycastDistance = 2f; // Maximum distance for the raycast

    private void Update()
    {
        // Shoot a ray forward from the player's position
        Ray ray = new Ray(playerTransform.position, playerTransform.forward);
        RaycastHit hit;

        // Check if the ray hits an object within the specified distance
        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            if (hit.collider.CompareTag("Selectable"))
            {
                // Assign the hit object's renderer to outlinedObject if it has one
                OutlinedObject = hit.collider.GetComponent<Renderer>();
                // Object with "selectable" tag is hit
                // Place your code here to handle the selectable object, e.g., highlight, select, etc.
                Debug.Log("Selectable object hit: " + hit.collider.name);
            }
            
        }
        else
        {
            // Clear the outlinedObject if no object is hit
            OutlinedObject = null;
        }
        Debug.DrawRay(playerTransform.position, playerTransform.forward * raycastDistance, Color.blue);

    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //setup stuff
        var commands = new CommandBuffer();

        //create selection buffer
        int selectionBuffer = Shader.PropertyToID("_SelectionBuffer");
        commands.GetTemporaryRT(selectionBuffer, source.descriptor);
        //render selection buffer
        commands.SetRenderTarget(selectionBuffer);
        commands.ClearRenderTarget(true, true, Color.clear);
        //draws outline if object is selected
        if (OutlinedObject != null)
        {
            commands.DrawRenderer(OutlinedObject, WriteObject);
        }
        //apply everything and clean up in commandbuffer
        commands.Blit(source, destination, ApplyOutline);
        commands.ReleaseTemporaryRT(selectionBuffer);


        //execute and clean up commandbuffer itself
        Graphics.ExecuteCommandBuffer(commands);
        commands.Dispose();
    }
}