using UnityEngine;

public class Bird : MonoBehaviour
{
    [SerializeField]
    private AudioSource _censor = null;

    [SerializeField]
    private AudioSource _uncensor = null;

    public int SortOrder = 1;

    private Animator _anim;
    private bool _isVisibleToGrampa = true;
    private bool _hasLostLife = false;
    private VisibilityManager _visibilityManager;
    private SpriteRenderer[] _sprites;

    public void Reset()
    {
        _isVisibleToGrampa = true;
        if (_anim != null)
        {
            _anim.SetBool("IsVisible", _isVisibleToGrampa);
        }
        _hasLostLife = false;

        // Set a unique sorting order on each bird so they don't flicker in and out behind each other
        _sprites = GetComponentsInChildren<SpriteRenderer>(true);
        Debug.Log("Setting sort order to " + SortOrder);
        foreach (var sprite in _sprites) {
            sprite.sortingOrder = SortOrder;
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
            Debug.Log("mouse down on " + gameObject.name);
            // We used to have an else here, where you'd play _uncensor
            // And set it to be visible to grampa again
            if (_isVisibleToGrampa)
            {
                Debug.Log("and it's visible to grampa " + gameObject.name);
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
        gameObject.SetActive(false);
	}
	
}
