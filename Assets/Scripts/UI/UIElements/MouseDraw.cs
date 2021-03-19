using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UIElements
{
    public class MouseDraw : MonoBehaviour
    {
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private float _lineWidth = 0.1f;
        [SerializeField] private float _lineHeight = 0.1f;
        [SerializeField] private Color _startColor = Color.blue;
        [SerializeField] private Color _endColor = Color.yellow;
        
        private Camera _camera;

        //Property that controls the size of the array that draws the points of the line
        private int PointCounter
        {
            get => _lineRenderer.positionCount;
            set => _lineRenderer.positionCount = value;
        }

        //Sets up your Line Renderer to have no points and your starting settings
        private void Awake()
        {
            _camera = Camera.main;
            _lineRenderer.startWidth = _lineWidth;
            _lineRenderer.endWidth = _lineHeight;
            _lineRenderer.startColor = _startColor;
            _lineRenderer.endColor = _endColor;
            PointCounter = 0;
        }

        private void Update()
        {
            //Draws a point when the left mouse button is pressed
            if (Input.GetMouseButtonDown(0))
            {
                DrawLine();
            }

            // Resets line when the right button is pressed
            if (Input.GetMouseButtonUp(1))
            {
                PointCounter = 0;
            }
        }

        private void DrawLine()
        {
            PointCounter++; //Adds one to the points array
            
            //This is the crucial line. mousePos always has a z position of 0. For the conversion to world space you need a
            //positive value as the near clip plane of the camera will stop it rendering 
            Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f);
            
            //Add the new point to the array (hence the -1) and convert to a world point.
            _lineRenderer.SetPosition(PointCounter - 1, _camera.ScreenToWorldPoint(mousePos));
        }
    }
}