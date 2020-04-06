
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
    [SerializeField]
    private AudioSource _censor = null;

    [SerializeField]
    private AudioSource _uncensor = null;

    [SerializeField]
    private SpriteRenderer[] _sprites;

    public int SortOrder = 1;

    private Animator _anim;
    private bool _isVisibleToGrampa = true;
    private bool _hasLostLife = false;
    private VisibilityManager _visibilityManager;

    public void Reset()
    {
        _isVisibleToGrampa = true;
        if (_anim != null)
        {
            _anim.SetBool("IsVisible", _isVisibleToGrampa);
        }
        _hasLostLife = false;

        // Set a unique sorting order on each bird so they don't flicker in and out behind each other
        foreach (var sprite in _sprites) {
            sprite.sortingOrder = SortOrder;
        }

        _sprites[0].gameObject.SetActive(true);
        _sprites[1].gameObject.SetActive(false);

        if (_anim != null)
        {
            _anim.speed = Random.Range(0.8f, 1f);
        }
    }

    private void Start()
    {
        _anim = GetComponentInChildren<Animator>();
        _visibilityManager = GetComponent<VisibilityManager>();
        _visibilityManager.ObjectOffScreen += OnOffScreen;
    }

    private void OnMouseDown()
    {
        if (Main.Instance.IsGameActive)
        {
            // We used to have an else here, where you'd play _uncensor
            // And set it to be visible to grampa again
            if (_isVisibleToGrampa)
            {
                _isVisibleToGrampa = false;
                _anim.SetBool("IsVisible", false);
                _censor?.Play();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("VisionCone"))
        {
            WhileInLineOfSight();
        }
    }

    private void WhileInLineOfSight()
    {
        if (_isVisibleToGrampa && !_hasLostLife)
        {
            Main.Instance.LoseLife();
            _hasLostLife = true;
        }
    }

    private void OnOffScreen()
	{
        Main.Instance.OffScreen();

        StartCoroutine("DisableGameObject");
    }

    private IEnumerator DisableGameObject()
    {
        // This is very silly, https://forum.unity.com/threads/animator-default-values-can-change-through-the-animations-itself.509668/
        // Turns out disabling the game object mid animation was causing the double wing issue - just writing defaults messing things up I suspect.
        _anim.SetTrigger("Disable");
        yield return new WaitForSeconds(.1f);

        gameObject.SetActive(false);
	}    
	
}
