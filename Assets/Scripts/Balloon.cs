
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Balloon : MonoBehaviour
{
    [SerializeField]
    private Transform _child = null;

    [SerializeField]
    private AudioSource _audioSourcePop = null;

    [SerializeField]
    private AudioSource _audioSourceHiss = null;

    [SerializeField]
    private AudioSource _audioSourceThump = null;

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

        _audioSourcePop.Stop();
        _audioSourceHiss.Stop();
        _audioSourceThump.Stop();
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
            OnPop();
        }
    }


    // When it hits the ground we make an explosion and that's what loses us a life
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_isPopped && !_hasLostLife)
        { 
            OnCrash();
        }
    }

    private void OnPop()
    {
        _anim.SetBool("IsPopped", true);
        _isPopped = true;

        // Turn gravity on
        GetComponent<Rigidbody2D>().gravityScale = 20f;

        // Play a popping sound and a hissing sound (at the same time)
        _audioSourcePop.Play();
        _audioSourceHiss.Play();
    }

    private void OnCrash()
    {
        // Play an explosion sound
        Main.Instance.Explosion();
        _audioSourceThump.Play();
        _hasLostLife = true;
        StartCoroutine ("DisableWhenNotPlaying");
    }

    private void OnPauseToggled(bool isPaused) {
        // If it's paused, set speed to 0, else to 1
        // This only affects popped - fly can proceed as normal
        _anim.SetFloat("SpeedMultiplier", isPaused ? 0f : 1f);

        // also pause the sounds
        if (isPaused)
        {
            _audioSourcePop.Pause();
            _audioSourceHiss.Pause();
            _audioSourceThump.Pause();
        }
        else
        {
            _audioSourcePop.UnPause();
            _audioSourceHiss.UnPause();
            _audioSourceThump.UnPause();
        }
    }

    private void OnOffScreen()
    {
        StartCoroutine ("DisableWhenNotPlaying");
    }

    private IEnumerator DisableWhenNotPlaying()
    {
        yield return new WaitWhile (() => _audioSourceThump.isPlaying);
        gameObject.SetActive(false);
    }
}
