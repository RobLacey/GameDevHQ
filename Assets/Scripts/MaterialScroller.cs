using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialScroller : MonoBehaviour
{
    [SerializeField] Material _background = default;
    [SerializeField] float _xSpeed = 0;
    [SerializeField] float _xOffset = 0;
    [SerializeField] float _ySpeed = 0;
    [SerializeField] float _yOffset = 0;

    void Update()
    {
        _background.SetTextureOffset("_MainTex", new Vector2(_xOffset, _yOffset));
        _xOffset += _xSpeed * Time.deltaTime;
        _yOffset += _ySpeed * Time.deltaTime;
    }
}
