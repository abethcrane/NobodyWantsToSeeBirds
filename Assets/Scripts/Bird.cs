﻿
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

    [SerializeField]
    private Material[] _materials;

    public int SortOrder = 1;

    private Animator _anim;
    private bool _isVisibleToGrampa = true;
    private bool _hasLostLife = false;
    private VisibilityManager _visibilityManager;
    private bool _hasGoneOffScreen = false;

    public void Reset()
    {
        _isVisibleToGrampa = true;
        if (_anim != null)
        {
            _anim.SetBool("IsVisible", _isVisibleToGrampa);
        }
        _hasLostLife = false;
        _hasGoneOffScreen = false;

        // Set a unique sorting order on each bird so they don't flicker in and out behind each other
        foreach (var sprite in _sprites) {
            sprite.sortingOrder = SortOrder;
        }

        _sprites[0].gameObject.SetActive(true);
        _sprites[1].gameObject.SetActive(false);

        var materialNum = Main.Instance.IsRainbowBirdsEnabled ? Random.Range(0, _materials.Length - 1) : 0;

        _sprites[0].material = _materials[materialNum];
        _sprites[1].material = _materials[materialNum];
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
            if (_isVisibleToGrampa)
            {
                _isVisibleToGrampa = false;
                _anim.SetBool("IsVisible", false);
                _censor?.Play();
            }
            else if (Main.Instance.AreBirdsUntappable)
            {
                _isVisibleToGrampa = true;
                _anim.SetBool("IsVisible", true);
                _uncensor?.Play();
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
            Main.Instance.SawBird();
            _hasLostLife = true;
        }
    }

    private void OnOffScreen()
	{
        if (!_hasGoneOffScreen) {
            _hasGoneOffScreen = true;
            Main.Instance.BirdWentOffScreen();
            StartCoroutine("DisableGameObject");
        }
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
