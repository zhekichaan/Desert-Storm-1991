using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class DoorLightingControl : MonoBehaviour
{
    private bool isLit = false;
    private Door door;
    private Material runtimeDarkMaterial;


    private void Awake()
    {
        // Get components
        door = GetComponentInParent<Door>();

        runtimeDarkMaterial = new Material(GameResources.Instance.variableLitShader);
        runtimeDarkMaterial.SetFloat("Alpha_Slider", 0f);

        SetDoorDarkImmediate();

    }

    private void SetDoorDarkImmediate()
    {
        SpriteRenderer[] spriteRenderers = GetComponentsInParent<SpriteRenderer>();

        foreach (SpriteRenderer sr in spriteRenderers)
        {
            sr.material = runtimeDarkMaterial;
        }
    }

    /// <summary>
    /// Fade in door
    /// </summary>
    public void FadeInDoor(Door door)
    {
        // Create new material to fade in
        Material material = new Material(GameResources.Instance.variableLitShader);

        if (!isLit)
        {
            SpriteRenderer[] spriteRendererArray = GetComponentsInParent<SpriteRenderer>();

            foreach (SpriteRenderer spriteRenderer in spriteRendererArray)
            {
                StartCoroutine(FadeInDoorRoutine(spriteRenderer, material));
            }

            isLit = true;
        }
    }

    /// <summary>
    /// Fade in door coroutine
    /// </summary>
    private IEnumerator FadeInDoorRoutine(SpriteRenderer spriteRenderer, Material material)
    {
        spriteRenderer.material = material;

        for (float i = 0.05f; i <= 1f; i += Time.deltaTime / Settings.fadeInTime)
        {
            material.SetFloat("Alpha_Slider", i);
            yield return null;

        }

        spriteRenderer.material = GameResources.Instance.litMaterial;
    }

    // Fade door in if triggered
    private void OnTriggerEnter2D(Collider2D collision)
    {
        FadeInDoor(door);
    }
}
