using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource _holdMusic;

    [SerializeField]
    private AudioSource _themeMusicIntro;

    [SerializeField]
    private AudioSource _themeMusicLoop;

    private void Awake()
    {
        _themeMusicIntro.Play();
        _themeMusicLoop.PlayDelayed(_themeMusicIntro.clip.length);
    }

    private void Start()
    {
        Main.Instance.GameOver += OnGameOver;
    }

    private void OnGameOver()
    {
        _themeMusicLoop.Stop();
        _holdMusic.Play();
    }
}
