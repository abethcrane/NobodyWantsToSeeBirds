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
    [Range(0,2)]
    private float _maxHeight = 1f;

    [SerializeField]
    [Range(0,1)]
    [Tooltip("The percentage along the image to prioritize if we need to start zooming in")]
    private float _horizontalAnchorPoint = 0.5f;

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
                
        switch (_anchorPos)
        {
            case AnchorPoint.Center:
                _viewPortValue = 0.5f;
                break;
            case AnchorPoint.Bottom:
                _viewPortValue = 0f;
                break;
            case AnchorPoint.Top:
                _viewPortValue = 1f;
                break;            
        }

        ResizeSpriteToScreenOrtho();
    }

    void Update()
    {
        if (_screenHeight != Screen.height || _screenWidth != Screen.width)
        {
            _screenWidth = Screen.width;
            _screenHeight = Screen.height;
            ResizeSpriteToScreenOrtho();
        }
    }
    void ResizeSpriteToScreenOrtho()
    {
        // Calculate the scale we need to make the sprite as wide as the screen
        float spriteBoundsWidth = _spriteRenderer.sprite.bounds.size.x;
        float worldScreenHeight = _camera.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight / _screenHeight * _screenWidth;
        float newScale = worldScreenWidth / spriteBoundsWidth;

        // Adjust the new scale to take into account our desired min and max height
        // This might make the sprite wider/narrower than the screen - that's okay
        // It's actually the point - it might be better to crop in rather than have most of the screen empty.
        float spriteBoundsHeight = _spriteRenderer.sprite.bounds.size.y;       
        float newSpriteHeight = spriteBoundsHeight * newScale;
        float newSpritePercentOfScreenHeight = newSpriteHeight / worldScreenHeight;

        newSpritePercentOfScreenHeight = Mathf.Clamp(newSpritePercentOfScreenHeight, _minHeight, _maxHeight);
        newSpriteHeight = newSpritePercentOfScreenHeight * worldScreenHeight;
        newScale =  newSpriteHeight / spriteBoundsHeight;

        // Apply the new scale (evenly to x and y to maintain aspect ratio)
        transform.localScale = new Vector3(newScale, newScale, 1);

        // Now reposition the sprite!
        // By default we center it horizontally, and position the vertical based on the enum
        var newPos = _camera.ViewportToWorldPoint(new Vector2(0.5f, _viewPortValue)); 

        // Recalculate now that we've scaled
        var newSpriteWidth = spriteBoundsWidth * newScale;

        // But if we're zooming in on the image to fit the desired min/max height, we use the _horizontalAnchorPoint
        if (newSpriteWidth > worldScreenWidth)
        {
            // If we're zooming in to e.g. 80%, we want to keep our _horizontalAnchorPoint where it is
            // So if it was 0.75, keep the 75% mark of the image at 75% along the screen
            var stickingPoint = _camera.ViewportToWorldPoint(new Vector2(_horizontalAnchorPoint, _viewPortValue)).x;
            var newLeftEdgeOfSprite = stickingPoint - (newSpriteWidth * _horizontalAnchorPoint);
            newPos.x = newLeftEdgeOfSprite + (newSpriteWidth / 2);
        }

        transform.position = new Vector3(newPos.x, newPos.y, _initialZPos);
    }

    void ResizeSpriteToScreenPerspective()
    {
        float spriteHeight = _spriteRenderer.sprite.bounds.size.y;
        float spriteWidth = _spriteRenderer.sprite.bounds.size.x;
        float distance = transform.position.z - _camera.transform.position.z;
        float screenHeight = 2 * Mathf.Tan(_camera.fieldOfView * Mathf.Deg2Rad / 2) * distance;
        float screenWidth = screenHeight * _camera.aspect;
        transform.localScale = new Vector3(screenWidth / spriteWidth, screenWidth / spriteWidth, 1);
    }
 }