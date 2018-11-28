using UnityEngine;

// https://kylewbanks.com/blog/create-fullscreen-background-image-in-unity2d-with-spriterenderer
public class FullscreenSprite : MonoBehaviour
{
    void Awake()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        float cameraHeight = Camera.main.orthographicSize * 2;
        Vector2 cameraSize = new Vector2(Camera.main.aspect * cameraHeight, cameraHeight);
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

        Vector2 scale = transform.localScale;
        if (cameraSize.x >= cameraSize.y)
        { // Landscape (or equal)
            scale *= cameraSize.x / spriteSize.x;
        }
        else
        { // Portrait
            scale *= cameraSize.y / spriteSize.y;
        }

        Camera camera = Camera.main;
        float bottomOfScreen = camera.ViewportToWorldPoint(new Vector3(0, 0, 10)).y;

        transform.position = new Vector2(0, bottomOfScreen + 5);
        transform.localScale = scale;
    }
}