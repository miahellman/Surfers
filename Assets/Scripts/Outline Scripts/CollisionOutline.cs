using UnityEngine;
using UnityEngine.Rendering;

public class CollisionOutline : MonoBehaviour
{
    // Outline object
    private Renderer outlinedObject;

    // Materials for outline vs no outline
    public Material WriteObject;
    public Material ApplyOutline;

    // Triggered when the player collides with a selectable object
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collided object is tagged "Selectable"
        if (other.CompareTag("Selectable"))
        {
            outlinedObject = other.GetComponent<Renderer>();
            Debug.Log("Collided with selectable object");
        }
    }

    // Triggered when the player exits collision with a selectable object
    private void OnTriggerExit(Collider other)
    {
        // If exiting a selectable object, remove the outline
        if (other.CompareTag("Selectable"))
        {
            outlinedObject = null;
            Debug.Log("Exited selectable object");
        }
    }

    // Called after all rendering is complete, draws the outline
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // Setup CommandBuffer for rendering
        var commands = new CommandBuffer();
        int selectionBuffer = Shader.PropertyToID("_SelectionBuffer");
        commands.GetTemporaryRT(selectionBuffer, source.descriptor);

        // Render selection buffer
        commands.SetRenderTarget(selectionBuffer);
        commands.ClearRenderTarget(true, true, Color.clear);

        // Draw outline if an object is selected
        if (outlinedObject != null)
        {
            commands.DrawRenderer(outlinedObject, WriteObject);
        }

        // Apply everything and clean up in CommandBuffer
        commands.Blit(source, destination, ApplyOutline);
        commands.ReleaseTemporaryRT(selectionBuffer);

        // Execute and clean up CommandBuffer itself
        Graphics.ExecuteCommandBuffer(commands);
        commands.Dispose();
    }
}
