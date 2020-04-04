using System;
using UnityEngine;

// https://kylewbanks.com/blog/create-fullscreen-background-image-in-unity2d-with-spriterenderer

// oooh https://answers.unity.com/questions/394372/calculating-distance-of-fov-to-pixel-size-an-objec.html
public class FullscreenSprite : MonoBehaviour
{
    public enum AnchorPoint {Top, Bottom, Center}

    [SerializeField]
    private AnchorPoint _anchorPos;

    [SerializeField]
    [Range(0,1)]
    private float _minHeight = 0.5f;

    [SerializeField]
    [Range(0,1)]
    private float _maxHeight = 1f;

    private SpriteRenderer _spriteRenderer;
    private Camera _camera;
    private float _screenWidth;
    private float _screenHeight;
    private float _initialZPos;
    private float _viewPortValue = 1f;

    void Awake()
    {
         _initialZPos = transform.position.z;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _camera = Camera.main;
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;
                
        if (_anchorPos == AnchorPoint.Center)
        {
            _viewPortValue = 0.5f;
        }
        else if (_anchorPos == AnchorPoint.Bottom)
        {
            _viewPortValue = 0f;
        }

        ResizeSpriteToScreenOrtho();
        RepositionSprite();
    }

    void Update()
    {
        if (_screenHeight != Screen.height || _screenWidth != Screen.width)
        {
            _screenWidth = Screen.width;
            _screenHeight = Screen.height;
            ResizeSpriteToScreenOrtho();
            RepositionSprite();
        }
    }
    void ResizeSpriteToScreenOrtho()
    {
        // Calculate the scale we need to make the sprite as wide as the screen
        float spriteBoundsWidth = _spriteRenderer.sprite.bounds.size.x;
        float worldScreenHeight = Camera.main.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight / _screenHeight * _screenWidth;
        float newScale = worldScreenWidth / spriteBoundsWidth;

        // Adjust the new scale to take into account our desired min and max height
        // This might make the sprite wider/narrower than the screen - that's okay
        // It's actually the point - it might be better to crop in rather than have most of the screen empty.
        float spriteBoundsHeight = _spriteRenderer.sprite.bounds.size.y;       
        float newSpriteHeight = spriteBoundsHeight * newScale;
        float newSpritePercentOfScreenHeight = newSpriteHeight / worldScreenHeight;

        Debug.Log(string.Format("Pre clamp: height: {0}, percent of screen: {1}, scale:{2}", newSpriteHeight, newSpritePercentOfScreenHeight, newScale));

        newSpritePercentOfScreenHeight = Mathf.Clamp(newSpritePercentOfScreenHeight, _minHeight, _maxHeight);
        newSpriteHeight = newSpritePercentOfScreenHeight * worldScreenHeight;
        newScale =  newSpriteHeight / spriteBoundsHeight;

        Debug.Log(string.Format("Post clamp: height: {0}, percent of screen: {1}, scale:{2}", newSpriteHeight, newSpritePercentOfScreenHeight, newScale));

        // Apply the new scale (evenly to x and y to maintain aspect ratio)
        transform.localScale = new Vector3(newScale, newScale, 1);
    }

    void ResizeSpriteToScreenPerspective()
    {
        //float spriteHeight = spriteRenderer.sprite.bounds.size.y;
        float spriteWidth = _spriteRenderer.sprite.bounds.size.x;
        float distance = transform.position.z - _camera.transform.position.z;
        float screenHeight = 2 * Mathf.Tan(_camera.fieldOfView * Mathf.Deg2Rad / 2) * distance;
        float screenWidth = screenHeight * _camera.aspect;
        transform.localScale = new Vector3(screenWidth / spriteWidth, screenWidth / spriteWidth, 1);
    }

    void RepositionSprite()
    {
        transform.position = _camera.ViewportToWorldPoint(new Vector3(0.5f, _viewPortValue, 10));

        Debug.Log(gameObject.name + " - " + transform.position);

        transform.position = new Vector3(transform.position.x, transform.position.y, _initialZPos);
    }
}