using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

public class BermudianTriangle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private const float DetectionLengthEpsilon = 0.15f;
    private const float SplineHeight = 0f;
    private const float DoubleClickDelay = 0.5f;

    [SerializeField] private SpriteShapeController _spriteShapeController;
    [SerializeField] private GameObject pointToInstantiate;

    private List<GameObject> _points = new List<GameObject>();
    
    private bool _isTouch;
    private bool _isDoubleClick;
    private int _idPointToEdit = -1;

    private int _clicked;
    private float _clickTime;

    private Vector3 _deltaBetweenClickAndSelect;

    private void Awake()
    {
        for (int i = 0; i < _spriteShapeController.spline.GetPointCount(); i++)
        {
            _spriteShapeController.spline.SetHeight(i, SplineHeight);
            _points.Add(Instantiate(pointToInstantiate, IntoGlobal(_spriteShapeController.spline.GetPosition(i)),
                Quaternion.identity, transform));
        }
    }

    private void Update()
    {
        if (_isTouch && _idPointToEdit != -1)
        {
            Vector3 currentLocalPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentLocalPosition = IntoLocal(new Vector3(currentLocalPosition.x, currentLocalPosition.y));

            if (IsPolygonConvex(_idPointToEdit, currentLocalPosition))
            {
                _spriteShapeController.spline.SetPosition(_idPointToEdit, currentLocalPosition - _deltaBetweenClickAndSelect);
                _points[_idPointToEdit].transform
                    .SetLocalPositionAndRotation(currentLocalPosition - _deltaBetweenClickAndSelect,
                        Quaternion.identity);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Enter Time: {Time.unscaledTime}");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"Exit Time: {Time.unscaledTime}");
    }

    private Vector3 IntoLocal(Vector3 globalPosition)
    {
        return globalPosition - _spriteShapeController.transform.position;
    }

    private Vector3 IntoGlobal(Vector3 localPosition)
    {
        return localPosition + _spriteShapeController.transform.position;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector3 clickPosition = Camera.main.ScreenToWorldPoint(eventData.pressPosition);
        clickPosition = new Vector3(clickPosition.x, clickPosition.y);

        Debug.Log("Pointer Clicked at position " + clickPosition);

        DetectDoubleClick();
        
        if (!DetectPointSelection(clickPosition))
        {
            DetectPointsCreation(clickPosition);
        }
    }

    private bool DetectPointSelection(Vector3 clickPosition)
    {
        for (int i = 0; i < _spriteShapeController.spline.GetPointCount(); i++)
        {
            Vector3 deltaPosition = clickPosition - IntoGlobal(_spriteShapeController.spline.GetPosition(i));

            if (deltaPosition.magnitude < DetectionLengthEpsilon)
            {
                if (_isDoubleClick && _spriteShapeController.spline.GetPointCount() > 3)
                {
                    _spriteShapeController.spline.RemovePointAt(i);
                    Destroy(_points[i]);
                    _points.RemoveAt(i);
                    return true;
                }
                
                _isTouch = true;
                _idPointToEdit = i;
                _deltaBetweenClickAndSelect = deltaPosition;
                Debug.Log($"Choose point to edit with id {i}");
                return true;
            }
        }

        return false;
    }

    private bool DetectPointsCreation(Vector3 clickPosition)
    {
        for (int i = 0; i < _spriteShapeController.spline.GetPointCount(); i++)
        {
            Vector3 pointsProjection = Vector3.Project(
                IntoLocal(clickPosition) - _spriteShapeController.spline.GetPosition(i),
                _spriteShapeController.spline.GetPosition((i + 1) % _spriteShapeController.spline.GetPointCount()) -
                _spriteShapeController.spline.GetPosition(i)) + _spriteShapeController.spline.GetPosition(i);

            Vector3 deltaPosition = clickPosition - IntoGlobal(pointsProjection);
            Debug.Log($"DetectPointsCreation in line {i} with magnitude {deltaPosition.magnitude}");

            if (deltaPosition.magnitude < DetectionLengthEpsilon)
            {
                _deltaBetweenClickAndSelect = deltaPosition;

                _spriteShapeController.spline.InsertPointAt(i + 1, IntoLocal(clickPosition - deltaPosition));
                _spriteShapeController.spline.SetHeight(i + 1, SplineHeight);

                _points.Insert(i + 1,
                    Instantiate(pointToInstantiate, IntoGlobal(_spriteShapeController.spline.GetPosition(i)),
                        Quaternion.identity, transform));
                
                _isTouch = true;
                _idPointToEdit = i + 1;
                Debug.Log($"Create point to edit with id {i + 1}");
                return true;
            }
        }

        return false;
    }

    private void DetectDoubleClick()
    {
        _clicked++;

        if (_clicked == 1)
        {
            _clickTime = Time.time;
        }

        if (_clicked > 1 && Time.time - _clickTime < DoubleClickDelay)
        {
            _clicked = 0;
            _clickTime = 0;
            _isDoubleClick = true;
            return;
        }
        
        if (_clicked > 2 || Time.time - _clickTime > 1)
        {
            _clicked = 0;
        }
        
        _isDoubleClick = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("OnPointerUp");

        _isTouch = false;
        _idPointToEdit = -1;
    }

    private bool IsPolygonConvex(int pointId, Vector2 newPointPosition)
    {
        int totalPointCount = _spriteShapeController.spline.GetPointCount();
        pointId += totalPointCount;

        Vector2[] points = new Vector2[totalPointCount];

        for (int i = 0; i < totalPointCount; i++)
        {
            points[i] = _spriteShapeController.spline.GetPosition(i);
        }

        points[pointId % totalPointCount] = newPointPosition;

        float signOfResult = 0f;

        for (int i = 0; i < totalPointCount; i++)
        {
            float dx1 = points[(i + 1) % totalPointCount].x - points[i].x;
            float dy1 = points[(i + 1) % totalPointCount].y - points[i].y;

            float dx2 = points[(i + 2) % totalPointCount].x - points[(i + 1) % totalPointCount].x;
            float dy2 = points[(i + 2) % totalPointCount].y - points[(i + 1) % totalPointCount].y;

            float z = dx1 * dy2 - dx2 * dy1;

            float sign = Mathf.Sign(z);

            if (i != 0 && Math.Abs(signOfResult - sign) > Mathf.Epsilon)
            {
                return false;
            }

            signOfResult = sign;
        }

        return true;
    }
}