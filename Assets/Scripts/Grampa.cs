using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Grampa : MonoBehaviour
{
    [SerializeField]
    private Animator _grampaAnimator;

    [SerializeField]
    private Animator _visionAnimator;

    [SerializeField]
    private AudioClip[] _lostLifeGrumbles;

    private int _numLivesLost = 0;
    private AudioSource _audioSource;

    private void Start()
    {
        _audioSource = gameObject.GetComponent<AudioSource>();
        Main.Instance.LostLife += OnLostLife;
    }

    private void OnLostLife()
	{
        _grampaAnimator.SetTrigger("LostLife");
        _visionAnimator.SetTrigger("Pulse");

        if (_numLivesLost < _lostLifeGrumbles.Length)
        {
            _audioSource.clip = _lostLifeGrumbles[_numLivesLost];
            _audioSource.Play();
        }

        _numLivesLost++;
    }
}
