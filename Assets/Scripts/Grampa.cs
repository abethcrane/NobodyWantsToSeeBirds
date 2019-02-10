using UnityEngine;

public class Grampa : MonoBehaviour
{
    [SerializeField]
    private Animator _grampaAnimator;

    [SerializeField]
    private Animator _visionAnimator;

    private void Start()
    {
        Main.Instance.LostLife += OnLostLife;
    }

    private void OnLostLife()
	{
        _grampaAnimator.SetTrigger("LostLife");
        _visionAnimator.SetTrigger("Pulse");
    }
}
