﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject _3;
    [SerializeField] GameObject _2;
    [SerializeField] GameObject _1;
    [SerializeField] GameObject _go;

    SpawnManager _spawnManager;
    public bool _isGameOver { get; set; }

    private void Start()
    {
        _spawnManager = FindObjectOfType<SpawnManager>();
        StartCoroutine(Countdown());
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && _isGameOver)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (Input.GetKeyDown(KeyCode.M) && _isGameOver)
        {
            SceneManager.LoadScene(0);
        }
    }
}
