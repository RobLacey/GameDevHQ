using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject _3 = default;
    [SerializeField] GameObject _2 = default;
    [SerializeField] GameObject _1 = default;
    [SerializeField] GameObject _go = default;

    //Variables
    SpawnManager _spawnManager;
    public bool _isGameOver { get; set; }

    private void Awake()
    {
        _spawnManager = FindObjectOfType<SpawnManager>();
    }

    private void Start()
    {
        StartCoroutine(Countdown());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
            Debug.Log("Quit");
        }

        if (Input.GetKeyDown(KeyCode.R) && _isGameOver)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (Input.GetKeyDown(KeyCode.M) && _isGameOver)
        {
            SceneManager.LoadScene(0);
        }
    }

    private IEnumerator Countdown()
    {
        _3.SetActive(true);
        yield return new WaitForSeconds(1);
        _3.SetActive(false);
        _2.SetActive(true);
        yield return new WaitForSeconds(1);
        _2.SetActive(false);
        _1.SetActive(true);
        yield return new WaitForSeconds(1);
        _1.SetActive(false);
        _go.SetActive(true);
        yield return new WaitForSeconds(1);
        _go.SetActive(false);
        _spawnManager.StartSpawning();
    }

}
