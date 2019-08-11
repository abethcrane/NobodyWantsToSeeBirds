using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Balloon : MonoBehaviour
{
    [SerializeField]
    private Transform _child;

    private Animator _anim;
    private bool _hasLostLife = false;
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
        _hasLostLife = false;

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
        if (Main.Instance.IsGameActive && !_hasLostLife)
        {
            Main.Instance.LoseLife();
            _hasLostLife = true;
            _anim.SetBool("IsPopped", true);
        }
    }

    private void OnBecameInvisible()
    {
        gameObject.SetActive(false);
    }
}
