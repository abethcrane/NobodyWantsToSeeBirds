using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource _holdMusic;

    [SerializeField]
    private AudioSource _themeMusicIntro;

    [SerializeField]
    private AudioSource _themeMusicLoop;

    private float[] _pitchPerLevel = new float[] { 0.98f, 1.04f, 1.07f, 1f, 1.125f, 1.15f, 1.175f, 1.2f, 1.22f, 1.24f, 1.26f };
    private int _level = 0;

    private void Awake()
    {
        _themeMusicIntro.Play();
        _themeMusicLoop.PlayDelayed(_themeMusicIntro.clip.length);
		_themeMusicLoop.pitch = _pitchPerLevel[0];

	}

    private void Start()
    {
        Main.Instance.GameOver += OnGameOver;
        Main.Instance.BirdSpawned += OnBirdSpawned;
    }

    private void OnBirdSpawned(int newLevel)
    {
        _level = newLevel;
        float start = _pitchPerLevel[_level];
        float end = start;
        if (_level + 1 < _pitchPerLevel.Length)
        {
            end = _pitchPerLevel[_level + 1];
        }

        _themeMusicLoop.pitch = Helpers.GetNextLerp(start, end, _themeMusicLoop.pitch, Main.NumBirdsPerQuoteLevelQuote);
    }

    private void OnGameOver()
    {
        StartCoroutine(CrossFade(_themeMusicLoop, _holdMusic));
    }

    private static IEnumerator CrossFade(AudioSource oldMusic, AudioSource newMusic, float FadeTime=2f)
    {
        float startVolume = oldMusic.volume;
        float endVolume = newMusic.volume;
        newMusic.volume = 0;
        newMusic.Play();

        while (oldMusic.volume > 0 || newMusic.volume < endVolume)
        {
            if (oldMusic.volume > 0)
            {
                oldMusic.volume -= startVolume * Time.deltaTime / FadeTime;
            }

            if (newMusic.volume < endVolume)
            {
                newMusic.volume += endVolume * Time.deltaTime / FadeTime;
            }

            yield return null;
        }

        oldMusic.Stop();
        oldMusic.volume = startVolume;
    }
}
