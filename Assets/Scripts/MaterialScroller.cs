using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialScroller : MonoBehaviour
{
    [SerializeField] Material _background = default;
    [SerializeField] float _speed = 0.01f;
    [SerializeField] float offset = 0;

    // Update is called once per frame
    void Update()
    {
        _background.SetTextureOffset("_MainTex", new Vector2(offset, 0));
        offset += _speed * Time.deltaTime;
    }
}
