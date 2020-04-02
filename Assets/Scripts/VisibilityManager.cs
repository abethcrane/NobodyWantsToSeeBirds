using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This offscreen system became a little more complex than I would have liked, given we can have multiple children sprites
// So basically when a child sprite becomes invisible invisible we set the checkOffscreen to true
// When a child sprite becomes visible,  wet set the checkOffscreen back to false
// Every update(), if the check is treu then we queue up a more thorough check and indicate that with _checkingoffscren
// The actual check waits 1 second, and then if the check flag is still on it loops through all children
// And only if they're all invisible does it trigger its BecameInvisible event

public class VisibilityManager : MonoBehaviour
{   
    public event Action ObjectOffScreen;
    private VisibilityEventer[] _sprites;
    private List<Renderer> _renderers = new List<Renderer>();
    private bool _checkForOffscreen = false;
    private bool _checkingOffscreen = false;

    private void Start()
    {
        _checkForOffscreen = false;
        _checkingOffscreen = false;
        _sprites = GetComponentsInChildren<VisibilityEventer>();
        foreach (var sprite in _sprites)
        {
            _renderers.Add(sprite.GetComponent<Renderer>());
            sprite.BecameInvisible += OnBecameInvisible;
            sprite.BecameVisible += OnBecameVisible;
        }
    }

    private void Update()
    {
        if (_checkForOffscreen && !_checkingOffscreen)
        {
            _checkingOffscreen = true;
            StartCoroutine(CheckOffscreen());
        }
    }

    private void OnBecameInvisible()
    {
        _checkForOffscreen = true;
    }

    private void OnBecameVisible()
    {
        _checkForOffscreen = false;
    }

    private IEnumerator CheckOffscreen() 
    {
        // Wait a second before we check if the children are all invisible so that we're not too trigger happy
        yield return new WaitForSeconds(1);

        if (_checkForOffscreen)
        {
            var isVisible = false;
            foreach (var renderer in _renderers)
            {
                if (renderer.isVisible)
                {
                    isVisible = true;
                    continue;
                }
            }

            if (!isVisible)
            {
                _checkForOffscreen = false;
                ObjectOffScreen?.Invoke();
            }
        }
        _checkingOffscreen  = false;
    }
}
