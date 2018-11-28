using UnityEngine;

public class Grampa : MonoBehaviour
{
    [SerializeField]
    private Animator _grampaAnimator;

    [SerializeField]
    private Animator _visionAnimator;

    private void Awake()
    {
        Vector3 rightBottomScreen = Camera.main.ScreenToWorldPoint(new Vector3(1, 1, -10));
        Vector3 newPos = transform.parent.position;
        newPos.x = rightBottomScreen.x - 6;
        newPos.y = -1*rightBottomScreen.y + 6;
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
