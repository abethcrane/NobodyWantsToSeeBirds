using UnityEngine;

public class Grampa : MonoBehaviour
{
    [SerializeField]
    private Animator _grampaAnimator;

    [SerializeField]
    private Animator _visionAnimator;

    [SerializeField]
    private AudioClip[] _lostLifeGrumbles;

    [SerializeField]
    private AudioSource _grumblesSource;

    private int _numLivesLost = 0;

    private void Start()
    {
        Main.Instance.LostLife += OnLostLife;
    }

    private void OnLostLife()
	{
        _grampaAnimator.SetTrigger("LostLife");
        _visionAnimator.SetTrigger("Pulse");

        if (_numLivesLost < _lostLifeGrumbles.Length)
        {
            _grumblesSource.clip = _lostLifeGrumbles[_numLivesLost];
            _grumblesSource.Play();
        }

        _numLivesLost++;
    }
}
