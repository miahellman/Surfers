using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Outliner : MonoBehaviour
{
    //outline object
    public Renderer OutlinedObject;

    //materials for outline vs no outline
    public Material WriteObject;
    public Material ApplyOutline;

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
            print("test");
        }
        //apply everything and clean up in commandbuffer
        commands.Blit(source, destination, ApplyOutline);
        commands.ReleaseTemporaryRT(selectionBuffer);

        //execute and clean up commandbuffer itself
        Graphics.ExecuteCommandBuffer(commands);
        commands.Dispose();
    }

    public void SetOutlineObject(GameObject outlineObject)
    {
        OutlinedObject = outlineObject.GetComponent<Renderer>();
        //print(outlineObject.name);
    }

    public void ClearOutlineObject()
    {
        OutlinedObject = null;
        //print("outline object null");
    }
}
