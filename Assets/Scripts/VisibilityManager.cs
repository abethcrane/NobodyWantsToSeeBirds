using System;
using UnityEngine;

public class VisibilityManager : MonoBehaviour
{   
    public event Action ObjectOffScreen;

    private Camera _camera;
    private float _objectWidthOffset = 0f;
    private float _objectHeightOffset = 0f;
    private void Start()
    {
        _camera = Camera.main;
        var collider = GetComponentInChildren<Collider2D>();

        // We calculate these so that we can wait  until the whole object has gone off screen, not just the center.
        // We need to cache it here because it might be inactive when they leave the screen (and return 0 haha)
        // In this game it's okay because sizes don't change
        _objectWidthOffset = collider.bounds.extents.x;
        _objectHeightOffset = collider.bounds.extents.y;
    }

    // No need to check if the game is paused
    private void FixedUpdate()
    {
        // Wait until the whole object has gone off screen
        var rightEdgeOfScreen = _camera.ViewportToWorldPoint(new Vector2(1, 1)).x;        
        var leftEdgeOfObj = gameObject.transform.position.x - _objectWidthOffset;

        if (leftEdgeOfObj > rightEdgeOfScreen)
        {
            ObjectOffScreen?.Invoke();
        }

        
        // Wait until the whole object has gone off screen
        var bottomEdgeOfScreen = _camera.ViewportToWorldPoint(new Vector2(1, 0)).y;
        var topofObject = gameObject.transform.position.y + _objectHeightOffset;

        if (topofObject < bottomEdgeOfScreen)
        {
            ObjectOffScreen?.Invoke();
        }

        // Wait until the whole object has gone off screen
        var topEdgeOfScreen = _camera.ViewportToWorldPoint(new Vector2(1, 1)).y;        
        var bottomOfObject = gameObject.transform.position.y - _objectHeightOffset;

        if (bottomOfObject > topEdgeOfScreen)
        {
            ObjectOffScreen?.Invoke();
        }
    }
}
