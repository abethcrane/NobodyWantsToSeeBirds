using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Balloon : MonoBehaviour
{
    [SerializeField]
    private Transform _child = null;

    private Animator _anim;
    private bool _isPopped = false;
    private bool _hasLostLife = false;
    private VisibilityManager _visibilityManager;

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
        _hasLostLife = false;

        Fly fly = GetComponent<Fly>();
        if (fly != null)
        {
            fly.Speed = 2f;
        }


        GetComponent<Rigidbody2D>().gravityScale = 0f;
    }

    private void Start()
    {
        _anim = GetComponentInChildren<Animator>();

        _visibilityManager = GetComponent<VisibilityManager>();
        _visibilityManager.ObjectOffScreen += OnOffScreen;

        Main.Instance.PauseToggled += OnPauseToggled;
    }

    private void OnMouseDown()
    {
        if (Main.Instance.IsGameActive && !_isPopped)
        {
            _anim.SetBool("IsPopped", true);
            _isPopped = true;
            // Turn gravity on
            GetComponent<Rigidbody2D>().gravityScale = 1f;
            // Play a popping sound
            // After that, play a falling sound            
        }
    }

    private void OnOffScreen()
    {
        gameObject.SetActive(false);
    }

    // When it hits the ground we make an explosion and that's what loses us a life
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_isPopped && !_hasLostLife)
        {
            // Play an explosion sound
            Main.Instance.Explosion();
            _hasLostLife = true;
            gameObject.SetActive(false);
        }
    }

    private void OnPauseToggled(bool isPaused) {
        //_anim.enabled = !isPaused;
        // If it's paused, set speed to 0, else to 1
        // This only affects popped - fly can proceed as normal
        _anim.SetFloat("SpeedMultiplier", isPaused ? 0f : 1f);
        // also pause the sounds
    }
}
