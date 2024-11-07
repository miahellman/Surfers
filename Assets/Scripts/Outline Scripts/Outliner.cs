using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Outliner : MonoBehaviour
{
    //outline object
    public Renderer activeObject;
    List<Renderer> outlineObjects = new List<Renderer>();

    //materials for outline vs no outline
    [SerializeField] Material writeObject;
    [SerializeField] Material applyOutline;
    [SerializeField] Material activeOutline;
    [SerializeField] Material testWrite;

    public Outliner(Material write, Material interactOutline, Material closeOutline, Material testWriteMat)
    {
        writeObject = write;
        applyOutline = interactOutline;
        activeOutline = closeOutline;
        testWrite = testWriteMat;
    }

    public void SetMats(Material write, Material interactOutline, Material closeOutline, Material testWriteMat)
    {
        writeObject = write;
        applyOutline = interactOutline;
        activeOutline = closeOutline;
        testWrite = testWriteMat;
        outlineObjects = new List<Renderer>();
    }

    private void Start()
    {
        outlineObjects = new List<Renderer>();
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //if (GameManager.instance.GetGameState() == GameManager.GameState.INTRO) { return; }
        //setup stuff
        var commands = new CommandBuffer();
        int selectionBuffer = Shader.PropertyToID("_SelectionBuffer");
        commands.GetTemporaryRT(selectionBuffer, source.descriptor);
        //render selection buffer
        commands.SetRenderTarget(selectionBuffer);
        commands.ClearRenderTarget(true, true, Color.clear);
        //draws outline if object is selected
        //if (OutlinedObject != null)
        //{
        //    commands.DrawRenderer(OutlinedObject, writeObject);
        //}
        if (activeObject != null)
        {
            commands.DrawRenderer(activeObject, writeObject);
            commands.Blit(source, destination, activeOutline);
        }
        else if (outlineObjects.Count > 0)
        {
            foreach (Renderer o in outlineObjects)
            {
                commands.DrawRenderer(o, writeObject);
            }
            commands.Blit(source, destination, applyOutline);
        }
        else
        {
            commands.Blit(source, destination, applyOutline);
        }

        //apply everything and clean up in commandbuffer
        commands.ReleaseTemporaryRT(selectionBuffer);

        //execute and clean up commandbuffer itself
        Graphics.ExecuteCommandBuffer(commands);
        commands.Dispose();
    }

    public void SetOutlineObject(GameObject outlineObject)
    {
        activeObject = outlineObject.GetComponent<Renderer>();
    }

    public void ClearOutlineObject()
    {
        activeObject = null;
    }

    public void SetObjects(List<Renderer> objects)
    {
        outlineObjects = objects;
    }

    public void ClearObjects()
    {
        outlineObjects.Clear();
    }
}
