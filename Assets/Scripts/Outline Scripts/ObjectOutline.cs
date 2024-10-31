using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ObjectOutline : MonoBehaviour
{
	//outline object
	public Renderer OutlinedObject;

	//materials for outline vs no outline
	public Material WriteObject;
	public Material ApplyOutline;

	void Update()
	{
		//check if we clicked on a selectable object
		if (Input.GetMouseButtonDown(0))
		{
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			bool hitSelectable = Physics.Raycast(ray, out var hit) && hit.transform.CompareTag("Selectable");
			//if hit, get the renderer of hit object, else no outline at all
			if (hitSelectable) {
				OutlinedObject = hit.transform.GetComponent<Renderer>();
			} else {
				OutlinedObject = null;
			}
		}
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		//setup stuff
		var commands = new CommandBuffer();
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
