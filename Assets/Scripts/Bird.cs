using UnityEngine;

public class Bird : MonoBehaviour
{
    [SerializeField]
    private AudioSource _censor = null;

    [SerializeField]
    private AudioSource _uncensor = null;

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
                _censor?.Play();
                _isVisibleToGrampa = false;
                _anim.SetBool("IsVisible", false);
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
        gameObject.SetActive(false);
	}
	
}
