using UnityEngine;

public class Grampa : MonoBehaviour
{
    [SerializeField]
    private Animator _grampaAnimator;

    [SerializeField]
    private Animator _visionAnimator;

    private void Awake()
    {
        float rightScreenX = Camera.main.ScreenToWorldPoint(new Vector3(1, 0, -10)).x;
        Vector3 newPos = transform.parent.position;
        newPos.x = rightScreenX - 6;
        transform.parent.position = newPos;
    }

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
