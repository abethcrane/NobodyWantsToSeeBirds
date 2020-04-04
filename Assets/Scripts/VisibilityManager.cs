using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibilityManager : MonoBehaviour
{   
    public event Action ObjectOffScreen;

    private Camera _camera;
    private float _objectLeftSideOffset = 0f;
    private void Start()
    {
        _camera = Camera.main;
        var collider = GetComponentInChildren<Collider2D>();

        // We need to cache it here because it might be inactive when they leave the screen (and return 0 haha)
        // In this game it's okay because sizes don't change
        _objectLeftSideOffset = -collider.bounds.extents.x;
    }

    // No need to check if the game is paused
    private void FixedUpdate()
    {
        var rightEdgeOfScreen = _camera.ViewportToWorldPoint(new Vector2(1, 1)).x;
        
        // Wait until the whole object has gone off screen
        var leftEdgeOfObj = gameObject.transform.position.x + _objectLeftSideOffset;

        if (leftEdgeOfObj > rightEdgeOfScreen)
        {
            ObjectOffScreen?.Invoke();
        }
    }
}
