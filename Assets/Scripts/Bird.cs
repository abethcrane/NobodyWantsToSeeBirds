using System.Collections;
using UnityEngine;

public class Bird : MonoBehaviour
{
    [SerializeField]
    private AudioSource _cubeAppear;

    [SerializeField]
    private AudioSource _cubeDisappear;

    private Animator _anim;
    private bool _isVisibleToGrampa = true;
    private bool _hasLostLife = false;
    private OnScreenVisibilityEventer[] _sprites;
    private Renderer[] _spriteRenderers;
    private bool _checkForOffscreen = false;
    private bool _checkingOffscreen = false;

    public void Reset()
    {
        _isVisibleToGrampa = true;
        if (_anim != null)
        {
            _anim.SetBool("IsVisible", _isVisibleToGrampa);
        }
        _hasLostLife = false;
        _checkForOffscreen = false;
        _checkingOffscreen = false;
    }

    private void Start()
    {
        _anim = GetComponentInChildren<Animator>();
        _sprites = GetComponentsInChildren<OnScreenVisibilityEventer>();
        _spriteRenderers = GetComponentsInChildren<Renderer>();
        foreach (var sprite in _sprites)
        {
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

    private void OnMouseDown()
    {
        if (Main.Instance.IsGameActive)
        {
            if (_isVisibleToGrampa)
            {
                _cubeDisappear.Play();
                _isVisibleToGrampa = false;
                _anim.SetBool("IsVisible", false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        WhileInLineOfSight();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        WhileInLineOfSight();
    }

    private void WhileInLineOfSight()
    {
        if (_isVisibleToGrampa && !_hasLostLife)
        {
            Main.Instance.LoseLife();
            _hasLostLife = true;
        }
    }

    // This offscreen system became a little more complex than I would have liked, given we have multiple children sprites
    // It'd be nice to make this part of OnScreenVisiblityEventer, but, eh...
    // So basically when we become invisible we set the check to true, when we become visible we set it back to false
    // Every update, if the check is on then we trigger an actual check event and indicate that
    // The actual check waits 1 second, and then if the check is on it loops through all children eventers and if they're all off it marks us as invisible
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
            foreach (var sprite in _spriteRenderers)
            {
                if (sprite.isVisible)
                {
                    isVisible = true;
                    continue;
                }
            }

            if (!isVisible)
            {
                _checkForOffscreen = false;
                Main.Instance.OffScreen();
                gameObject.SetActive(false);
            }
        }
        _checkingOffscreen  = false;
    }
}
