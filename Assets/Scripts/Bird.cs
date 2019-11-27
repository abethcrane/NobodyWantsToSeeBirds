﻿using UnityEngine;

public class Bird : MonoBehaviour
{
    [SerializeField]
    private AudioSource _cubeAppear;

    [SerializeField]
    private AudioSource _cubeDisappear;

    private Animator _anim;
    private bool _isVisibleToGrampa = true;
    private bool _hasLostLife = false;
    private OnScreenVisibilityEventer _sprite;

    public void Reset()
    {
        _isVisibleToGrampa = true;
        if (_anim != null)
        {
            _anim.SetBool("IsVisible", _isVisibleToGrampa);
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

    private void OnBecameInvisible()
    {
        Main.Instance.OffScreen();
        gameObject.SetActive(false);
    }
}
