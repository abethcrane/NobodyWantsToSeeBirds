using System;
using UnityEngine;

// https://kylewbanks.com/blog/create-fullscreen-background-image-in-unity2d-with-spriterenderer

// oooh https://answers.unity.com/questions/394372/calculating-distance-of-fov-to-pixel-size-an-objec.html
public class FullscreenSprite : MonoBehaviour
{
    public enum AnchorPoint {Top, Bottom, Center}

    [SerializeField]
    private AnchorPoint _anchorPos;

    SpriteRenderer _spriteRenderer;
    Camera _camera;
    float _screenWidth;
    float _screenHeight;

    void Awake()
    {   
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _camera = Camera.main;
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;
        UpdateScreenSize();
    }

    void Update()
    {
        if (_screenHeight != Screen.height || _screenWidth != Screen.width)
        {
            _screenWidth = Screen.width;
            _screenHeight = Screen.height;
            UpdateScreenSize();
        }
    }

    void UpdateScreenSize()
    {
        //float spriteHeight = spriteRenderer.sprite.bounds.size.y;
        float spriteWidth = _spriteRenderer.sprite.bounds.size.x;
        float distance = transform.position.z - _camera.transform.position.z;
        float screenHeight = 2 * Mathf.Tan(_camera.fieldOfView * Mathf.Deg2Rad / 2) * distance;
        float screenWidth = screenHeight * _camera.aspect;
        transform.localScale = new Vector3(screenWidth / spriteWidth, screenWidth / spriteWidth, 1);

        float viewPortValue = 1f;
        if (_anchorPos == AnchorPoint.Center)
        {
            viewPortValue = 0.5f;
        }
        else if (_anchorPos == AnchorPoint.Bottom)
        {
            viewPortValue = 0f;
        }

        transform.position = _camera.ViewportToWorldPoint(new Vector3(0.5f, viewPortValue, 10));

        Debug.Log(gameObject.name + " - " + transform.position);

        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }
}