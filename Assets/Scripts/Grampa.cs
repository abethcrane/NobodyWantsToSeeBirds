using UnityEngine;

public class Grampa : MonoBehaviour
{
    [SerializeField]
    private Animator _grampaAnimator;

    [SerializeField]
    private Animator _visionAnimator;

    private void Awake()
    {
        Camera camera = Camera.main;
        Vector3 rightBottomScreen = camera.ViewportToWorldPoint(new Vector3(1, 0, 10));
        Vector3 newPos = transform.parent.position;
        // Good magic numbers in order to get it all visible on the screen
        newPos.x = rightBottomScreen.x - 7;
        newPos.y = rightBottomScreen.y + 4.5f;
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
