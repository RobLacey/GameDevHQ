using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GlobalVars", menuName = "Global Variables")]

public class GlobalVariables : ScriptableObject
{
    [SerializeField] float _topBounds = default;
    [SerializeField] float _bottomBounds = default;
    [SerializeField] float _rightBounds = default;
    [SerializeField] float _LeftBounds = default;

    public float TopBounds { get { return _topBounds; } }
    public float BottomBounds { get { return _bottomBounds; } }
    public float RightBounds { get { return _rightBounds; } }
    public float LeftBounds { get { return _LeftBounds; } }
}
