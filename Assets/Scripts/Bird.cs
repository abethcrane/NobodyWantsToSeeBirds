using UnityEngine;

public class Bird : MonoBehaviour
{
    private Animator _anim;
    private bool _isVisible = true;
    private bool _hasLostLife = false;
    private OnScreenVisibilityEventer _sprite;

    public void Reset()
    {
        _isVisible = true;
        if (_anim != null)
        {
            _anim.SetBool("IsVisible", _isVisible);
        }
        _hasLostLife = false;
    }

    private void Start()
    {
        _anim = GetComponentInChildren<Animator>();
        _sprite = GetComponentInChildren<OnScreenVisibilityEventer>();
        _sprite.BecameInvisible += OnBecameInvisible;
    }

    private void OnMouseDown()
    {
        _isVisible = !_isVisible;
        _anim.SetBool("IsVisible", _isVisible);
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
        if (_isVisible && !_hasLostLife)
        {
            Main.Instance.LoseLife();
            _hasLostLife = true;
        }
    }

    private void OnBecameInvisible()
    {
        Main.Instance.OffScreen();
        gameObject.SetActive(false);
    }
}
