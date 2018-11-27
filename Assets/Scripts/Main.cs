using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    public event System.Action LostLife;

    public static Main Instance;
    public float SecondsBetweenSpawns = 3f;
    public float Velocity = 1f;

    [SerializeField]
    private TextMeshProUGUI _healthText;

    [SerializeField]
    private GameObject _gameOverText;

    [SerializeField]
    private TextMeshProUGUI _scoreText;

    [SerializeField]
    private GameObject _birdPrefab;

    [SerializeField]
    private string[] _lives;

    [SerializeField]
    private GameObject _birdPool;

    private bool _isGameOver = false;
    private float _timeSinceLastSpawn;
    private List<GameObject> _birds = new List<GameObject>();
    private int _numLives;
    private int _score = 0;
    private float _leftScreenX;

    private void Awake()
    {
        Instance = this;
        _timeSinceLastSpawn = 0f;
        _numLives = _lives.Length;
        _leftScreenX = -1 * Camera.main.ScreenToWorldPoint(new Vector3(0, 0, -10)).x;
        SpawnBird();
    }

	private void Update()
    {
        if (!_isGameOver)
        {
            _timeSinceLastSpawn += Time.deltaTime;

            if (_timeSinceLastSpawn > SecondsBetweenSpawns)
            {
                SpawnBird();
                _timeSinceLastSpawn = 0f;
                SecondsBetweenSpawns -= (SecondsBetweenSpawns / 10f * 1 / Time.timeSinceLevelLoad);
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                int scene = SceneManager.GetActiveScene().buildIndex;
                SceneManager.LoadScene(scene, LoadSceneMode.Single);
            }
        }
	}

    public void OffScreen()
    {
        if (!_isGameOver)
        {
            _score += 1;
            _scoreText.text = "Score: " + _score.ToString();
        }
    }

    public void LoseLife()
    {
        _numLives -= 1;

        _healthText.text = "Health: " + _lives[_numLives];
        LostLife?.Invoke();

        if (_numLives == 0)
        {
            GameOver();
        }
    }


    private void SpawnBird()
    {
        GameObject birdObj = GetPooledBird();
        if (birdObj == null)
        {
            return;
        }
        birdObj.SetActive(true);
        birdObj.transform.position = new Vector3(_leftScreenX, Random.Range(-1f, 5f), -0.1f);
        Debug.Log(birdObj.transform.position);
        Bird bird = birdObj.GetComponent<Bird>();
        bird.Reset();
        Fly fly = birdObj.GetComponent<Fly>();
        fly.Velocity = new Vector3(Velocity, 0, 0);
        Velocity += (1f / Mathf.Max(1, Time.timeSinceLevelLoad));
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


    private void GameOver()
    {
        _isGameOver = true;
        _gameOverText.SetActive(true);
        foreach (GameObject bird in _birds)
        {
            GameObject.Destroy(bird);
        }
        _birds.Clear();
    }
}
