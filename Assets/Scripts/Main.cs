using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public enum HealthDamage {
    BalloonCrashed,
    SawBird
}

public class Main : MonoBehaviour
{
    public const int NumBirdsPerQuoteLevelQuote = 50;
    private const float DefaultDecayRate = 0.75f;

    public event System.Action<HealthDamage> LostLife;
    public event System.Action GameOver;
	public event System.Action BirdSpawned;
	public event System.Action<bool> PauseToggled;

	public static Main Instance;

    public float SecondsBetweenSpawns = 3f; // Used only as a visual for debugging
    public float Speed = 3f;
    public bool IsRainbowBirdsEnabled;
    public bool AreBirdsUntappable;

    [SerializeField]
    private bool _useSillyRotationMode;

    [SerializeField]
    private TextMeshProUGUI _healthText;

    [SerializeField]
    private TextMeshProUGUI _scoreText;

    [SerializeField]
    private GameObject _tapToPlay;

    [SerializeField]
    private GameObject _pauseButton;

	[SerializeField]
    private GameObject _birdPrefab;

	[SerializeField]
	private GameObject _airBalloonPrefab;

	[SerializeField]
    private string[] _lives;

    [SerializeField]
    private GameObject _birdPool;

	[SerializeField]
	private GameObject _airBalloonPool;

	[SerializeField]
    private Transform _grandpaTransform;

	[SerializeField]
	private AnimationCurve _spawnSpeedIncrease;

	[SerializeField]
	private AnimationCurve _birdSpeedIncrease;

    [SerializeField]
    [RangeAttribute(0, 100)]
    private int _percentageBalloons = 8;

    private bool _isGameOver = false;
    private bool _isGamePaused = false;
    private float _timeSinceLastSpawn;
    private List<GameObject> _birds = new List<GameObject>();
    private int _numLives;
    private int _score = 0;
    private string _scoreName;
    private Camera _camera;
    private Animator _cameraAnim;
    private float _screenWidth;
    private float _screenHeight;
    private float _minFlyingPos = 0;
    private float _maxBirdYPos = 1;
    private float _maxBalloonYPos =  1;
    private float _birdHeight = 1.875f; // This /should/ be calculated from bird extents but *shrug*

    public bool IsGameActive => !_isGameOver && !_isGamePaused;
    public float SecondsOfGamePlay { get; private set; } = 0f;
    public float MinutesOfGamePlay => SecondsOfGamePlay / 60;

    private int _numBirdsSpawned = 0;
    private float[] _spawnSecondsPerLevel = new float[]{2f, 1f, 0.875f, 0.75f, 0.5f, 0.45f, 0.4f, 0.375f, 0.36f, 0.35f, 0.345f};


    private void Awake()
    {
        Instance = this;
        _camera = Camera.main;
        _cameraAnim = GetComponent<Animator>();
        UpdateScreenDimensions();

        _numLives = _lives.Length - 1;
        _timeSinceLastSpawn = 1000; // Arbitrarily large so we generate one on first update
		Screen.sleepTimeout = SleepTimeout.NeverSleep; // Stop screen dimming

        BirdSpawned += OnBirdSpawn;

        _scoreName = _scoreText.text.Split(':')[0] ?? "Score";
	}

    private void Update()
    {
        if (IsGameActive)
        {
            SecondsOfGamePlay += Time.deltaTime;

            if (_screenHeight != Screen.height || _screenWidth != Screen.width)
            {
                UpdateScreenDimensions();
            }

            _timeSinceLastSpawn += Time.deltaTime;

            // If we're overdue then we have a very reasonable chance of spawning something.
            // If we're not due yet, we have a quite low but not impossible chance
            if (_timeSinceLastSpawn > SecondsBetweenSpawns && Random.Range(0, 100) < 40
                || _timeSinceLastSpawn < SecondsBetweenSpawns && Random.Range(0, 100) < 0.05)
            {
                // Choose whether to spawn a bird or balloon
                if (Random.Range(0f, 100f) < _percentageBalloons)
                {
                    SpawnBalloon();
                }
                else
                {
                    SpawnBird();
                    
                }

                _timeSinceLastSpawn = 0f;

            }
        }
    }

    public void StartGame()
    {
        // Reload the scene, but that doesn't reload this script
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        _timeSinceLastSpawn = 0f;

        _numLives = _lives.Length - 1;
        _score = 0;

        SpawnBird();

        _isGameOver = false;
        _isGamePaused = false;
    }

    public void BirdWentOffScreen()
    {
        if (IsGameActive)
        {
            _score += 1;
            _scoreText.text = $"{_scoreName}: {_score}";
        }
    }

    public void SawBird()
    {
        LoseLife(HealthDamage.SawBird);
    }

    public void BalloonCrashed()
    {
        _cameraAnim.SetTrigger("Shake");
        LoseLife(HealthDamage.BalloonCrashed);
    }

    public void TogglePause()
    {
        _isGamePaused = !_isGamePaused;
        PauseToggled?.Invoke(_isGamePaused);

        // This blogpost indicated this would be the best thing to do: https://gamedevbeginner.com/the-right-way-to-pause-the-game-in-unity/
        // But it doesn't seem like time.timeScale is affected by this.
        // So I'm using it for pausing the bird's velocity but managing my own SecondsOfGamePlay 
        Time.timeScale = _isGamePaused ? 0 : 1f;
    }

	public void ToggleAudio()
	{
		AudioListener.pause = !AudioListener.pause;
	}

    private void UpdateScreenDimensions()
    {
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;
        
        _minFlyingPos = _grandpaTransform.position.y + 2f; // For padding

        var topOfScreen = _camera.ViewportToWorldPoint(Vector2.one).y;
        _maxBirdYPos = topOfScreen - (_birdHeight * 0.5f);
        _maxBalloonYPos = topOfScreen + 0.1f;
    }

    private void SpawnBird()
    {
        GameObject birdObj = GetOrCreatePooledBird();
        birdObj.SetActive(true);
        float leftSideOfScreen = _camera.ViewportToWorldPoint(new Vector3(0, 0, 10)).x;
        var xPos = leftSideOfScreen - Random.Range(3f, 5f);
        var yPos = Random.Range(_minFlyingPos, _maxBirdYPos);
        // Needs to be far ennough left so that they bump each other out of the way before coming onto the screen
        birdObj.transform.position = new Vector3(xPos, yPos, birdObj.transform.position.z);
        Bird bird = birdObj.GetComponent<Bird>();
        bird.Reset();
        Fly fly = birdObj.GetComponent<Fly>();
		Speed = _birdSpeedIncrease.Evaluate(MinutesOfGamePlay);
		fly.Speed = Speed;
        fly.Rotation = _useSillyRotationMode ? Random.Range(-5f, 5f) : 0;

		BirdSpawned?.Invoke();
	}

	private void SpawnBalloon()
	{
        GameObject balloonObj = GetOrCreatePooledBalloon();
        balloonObj.SetActive(true);
        float leftSideOfScreen = _camera.ViewportToWorldPoint(new Vector3(0, 0, 10)).x;
        float xPos = leftSideOfScreen - 3;
        float yPos = Random.Range(_minFlyingPos, _maxBalloonYPos);

		balloonObj.transform.position = new Vector3(xPos, yPos, balloonObj.transform.position.z);
		balloonObj.transform.parent = _airBalloonPool.transform;
    }

    private GameObject GetOrCreatePooledBalloon()
    {
        GameObject newBalloon = GetPooledObject(typeof(Balloon), _airBalloonPool);

        if (newBalloon == null)
        {
            newBalloon = GameObject.Instantiate(_airBalloonPrefab);
            newBalloon.transform.parent = _airBalloonPool.transform;
        }
        else
        {
            Balloon balloon = newBalloon.GetComponent<Balloon>();
            balloon.Reset();
        }

        return newBalloon;
    }

    private GameObject GetOrCreatePooledBird()
    {
        GameObject newBird = GetPooledObject(typeof(Bird), _birdPool);

        if (newBird == null)
        {
            newBird = GameObject.Instantiate(_birdPrefab);
            newBird.transform.parent = _birdPool.transform;
            Bird bird = newBird.GetComponent<Bird>();
            bird.SortOrder = _birdPool.transform.childCount;
        }

        return newBird;
    }

    private GameObject GetPooledObject(System.Type t, GameObject parent)
    {
        foreach (var obj in parent.GetComponentsInChildren(t, true))
        {
            if (!obj.gameObject.activeSelf)
            {
                return obj.gameObject;
            }
        }

        return null;
    }


    private void LoseLife(HealthDamage reason)
    {
        _numLives -= 1;

        if (_numLives < 0)
        {
            return;
        }

        _healthText.text = string.Format("Lives: {0} {1}", _numLives, _lives[_numLives]);
        LostLife?.Invoke(reason);

        if (_numLives == 0)
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        _isGameOver = true;
        _tapToPlay.SetActive(true);
        _pauseButton.SetActive(false);
        GameOver?.Invoke();
    }

    private void OnBirdSpawn()
    {
        _numBirdsSpawned += 1;
        SecondsBetweenSpawns -= Time.deltaTime;
        int level = _numBirdsSpawned / NumBirdsPerQuoteLevelQuote;
        if (level >= _spawnSecondsPerLevel.Length)
        {
            level = _spawnSecondsPerLevel.Length - 1;
        }

        float start = _spawnSecondsPerLevel[level];
        float end = start * DefaultDecayRate;
        if (level < _spawnSecondsPerLevel.Length - 1)
        {
            end = _spawnSecondsPerLevel[level + 1];
        }
        SecondsBetweenSpawns = GetNextLerp(start, end, SecondsBetweenSpawns, NumBirdsPerQuoteLevelQuote);
    }

    private float GetNextLerp(float start, float end, float current, int num_steps)
    {
        float range = end - start;
        if (current < start && start < end)
        {
            return start;
        }
        else if (current > start && start > end)
        {
            return start;
        }
        else if (current > end && start < end)
        {
            return end;
        }
        else if (current < end && start > end)
        {
            return end;
        }
        else
        {
            float step = range / num_steps;
            return current + step;
        }
    }
}
