using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class Main : MonoBehaviour
{
    public const int NumBirdsPerQuoteLevelQuote = 50;
    private const float DefaultDecayRate = 0.75f;

    public event System.Action LostLife;
    public event System.Action GameOver;
	public event System.Action BirdSpawned;

	public static Main Instance;
    public float SecondsBetweenSpawns = 3f; // Used only as a visual for debugging
    public float Velocity = 1f;

    [SerializeField]
    private TextMeshProUGUI _healthText;

    [SerializeField]
    private GameObject _tapToPlay;

    [SerializeField]
    private GameObject _pauseButton;

    [SerializeField]
    private TextMeshProUGUI _scoreText;

    [SerializeField]
    private GameObject _birdPrefab;

    [SerializeField]
    private string[] _lives;

    [SerializeField]
    private GameObject _birdPool;

    [SerializeField]
    private Transform _grandpaTransform;

	[SerializeField]
	private AnimationCurve _spawnSpeedIncrease;

	[SerializeField]
	private AnimationCurve _birdVelocityIncrease;

	private bool _isGameOver = false;
    private bool _isGamePaused = false;
    private float _timeSinceLastSpawn;
    private List<GameObject> _birds = new List<GameObject>();
    private int _numLives;
    private int _score = 0;
    private Camera _camera;
    private int _numBirdsSpawned = 0;
	private float _topOfScreen;
    float _screenWidth;
    float _screenHeight;

    public float TimeFactor => _isGamePaused ? 0f : 1f;

    public bool IsGameActive => !_isGameOver && !_isGamePaused;

    private void Awake()
    {
        Instance = this;
        _camera = Camera.main;

        _topOfScreen = _camera.ViewportToWorldPoint(new Vector3(0.5f, 1f, 10)).y;
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;

        _numLives = _lives.Length - 1;

		SpawnBird();

		// Stop screen dimming
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}

    private void Update()
    {
        if (IsGameActive)
        {
            if (_screenHeight != Screen.height || _screenWidth != Screen.width)
            {
                _screenWidth = Screen.width;
                _screenHeight = Screen.height;
                _topOfScreen = _camera.ViewportToWorldPoint(new Vector3(0.5f, 1f, 10)).y;
            }

            _timeSinceLastSpawn += Time.deltaTime;

            if (_timeSinceLastSpawn > SecondsBetweenSpawns)
            {
                SpawnBird();
                _timeSinceLastSpawn = 0f;
                SecondsBetweenSpawns = _spawnSpeedIncrease.Evaluate(Time.timeSinceLevelLoad / 60);
			}
        }
    }

    public void StartGame()
    {
        // Reload the scene, but that doesn't reload this script
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        foreach (GameObject bird in _birds)
        {
            GameObject.Destroy(bird);
        }
        _birds.Clear();

        _timeSinceLastSpawn = 0f;

        _numLives = _lives.Length - 1;
        _score = 0;
        SpawnBird();

        _isGameOver = false;
        _isGamePaused = false;
    }

    public void OffScreen()
    {
        if (IsGameActive)
        {
            _score += 1;
            _scoreText.text = "Score: " + _score.ToString();
        }
    }

    public void LoseLife()
    {
        _numLives -= 1;

        if (_numLives < 0)
        {
            return;
        }

        _healthText.text = string.Format("Lives: {0} {1}", _numLives, _lives[_numLives]);
        LostLife?.Invoke();

        if (_numLives == 0)
        {
            EndGame();
        }
    }

    public void TogglePause()
    {
        _isGamePaused = !_isGamePaused;
    }

	public void ToggleAudio()
	{
		AudioListener.pause = !AudioListener.pause;
	}

    private void SpawnBird()
    {
        GameObject birdObj = GetOrCreatePooledBird();
        if (birdObj == null)
        {
            return;
        }
        birdObj.SetActive(true);
        float leftSideOfScreen = _camera.ViewportToWorldPoint(new Vector3(0, 0, 10)).x;
        birdObj.transform.position = new Vector3(leftSideOfScreen, Random.Range(_grandpaTransform.position.y + 1.2f, _topOfScreen - 1f), -0.3f);
        Bird bird = birdObj.GetComponent<Bird>();
        bird.Reset();
        Fly fly = birdObj.GetComponent<Fly>();
		Velocity = _birdVelocityIncrease.Evaluate(Time.timeSinceLevelLoad / 60);
		fly.Velocity = new Vector3(Velocity, 0, 0);

        _numBirdsSpawned++;

		BirdSpawned?.Invoke();
	}

    private GameObject GetOrCreatePooledBird()
    {
        GameObject newPooledBird = GetPooledBird();

        if (newPooledBird == null)
        {
            newPooledBird = GameObject.Instantiate(_birdPrefab);
            newPooledBird.transform.parent = _birdPool.transform;
        }

        return newPooledBird;
    }

    private GameObject GetPooledBird()
    {
        foreach (Bird b in _birdPool.GetComponentsInChildren<Bird>(true))
        {
            if (!b.gameObject.activeSelf)
            {
                return b.gameObject;
            }
        }

        return null;
    }

    private void EndGame()
    {
        _isGameOver = true;
        _tapToPlay.SetActive(true);
        _pauseButton.SetActive(false);
        GameOver?.Invoke();
    }
}
