using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Balloon : MonoBehaviour
{
    [SerializeField]
    private Transform _child = null;

    private Animator _anim;
    private bool _isPopped = false;
    private OnScreenVisibilityEventer _sprite;

    public void Reset()
    {
        if (_anim != null)
        {
            _anim.SetBool("IsPopped", false);
        }
        transform.localPosition = Vector3.zero;
        transform.rotation = Quaternion.identity;
        _child.transform.localPosition = Vector3.zero;
        _isPopped = false;

        Fly fly = GetComponent<Fly>();
        if (fly != null)
        {
            fly.Velocity = new Vector3(2, 0, 0);
        }
    }

    private void Start()
    {
        _anim = GetComponentInChildren<Animator>();
        _sprite = GetComponentInChildren<OnScreenVisibilityEventer>();
        _sprite.BecameInvisible += OnBecameInvisible;
    }

    private void OnMouseDown()
    {
        if (Main.Instance.IsGameActive && !_isPopped)
        {
            _anim.SetBool("IsPopped", true);
            _isPopped = true;
        }
    }

    private void OnBecameInvisible()
    {
        gameObject.SetActive(false);
    }

    // When it hits the ground we make an explosion and that's what loses us a life
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isPopped)
        {
            Debug.LogWarning("LOSING balloon life to "+ collision.name);
            Main.Instance.LoseLife();
        }
    }
}
