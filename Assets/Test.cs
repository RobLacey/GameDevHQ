using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{

    enum Difficultly
    {
        Easy, Medium, Hard
    }

    [SerializeField] private Difficultly myDifficulty;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            switch (myDifficulty)
            {
                case Difficultly.Easy:
                    Debug.Log("I'm in easy");
                    break;
                case Difficultly.Medium:
                    Debug.Log("I'm in Medium");
                    break;
                case Difficultly.Hard:
                    Debug.Log("I'm in Hard");
                    break;
                default:
                    Debug.Log("Invalid");
                    break;
            }
        }
    }
}
