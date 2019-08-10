using UnityEngine;

public class Balloon : MonoBehaviour
{
    [SerializeField]
    private AudioSource _cubeAppear;

    [SerializeField]
    private AudioSource _cubeDisappear;

    private Animator _anim;
    private bool _hasLostLife = false;
    private OnScreenVisibilityEventer _sprite;

    public void Reset()
    {
        if (_anim != null)
        {
            _anim.SetBool("IsPopped", false);
        }
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
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
