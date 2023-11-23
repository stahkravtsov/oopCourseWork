using System;
using System.Collections.Generic;
using System.Globalization;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.U2D;
using UnityEngine.UI;

public class BermudianTriangle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private const float DetectionLengthEpsilon = 0.25f;
    private const float SplineHeight = 0f;
    private const float DoubleClickDelay = 0.5f;
    private const int MinimalPointCount = 3;

    [SerializeField] private SpriteShapeController _spriteShapeController;
    [SerializeField] private GameObject _pointToInstantiate;
    [SerializeField] private ShipManager _shipManager;
    [SerializeField] private SettingsAndInfo _settingsAndInfo;

    private List<GameObject> _points = new List<GameObject>();

    private bool _isTouch;
    private bool _isDoubleClick;
    private int _idPointToEdit = -1;

    private int _clicked;
    private float _clickTime;

    private Vector3 _deltaBetweenClickAndSelect;

    //draw point on the vertices of the polygon
    private void Awake()
    {
        for (int i = 0; i < _spriteShapeController.spline.GetPointCount(); i++)
        {
            _spriteShapeController.spline.SetHeight(i, SplineHeight);
            _points.Add(Instantiate(_pointToInstantiate,
                IntoGlobalPosition(_spriteShapeController.spline.GetPosition(i)),
                Quaternion.identity, transform));
        }
    }

    //If the pointer is down, move the point with the mouse
    private void Update()
    {
        if (_isTouch && _idPointToEdit != -1)
        {
            Vector3 currentLocalPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentLocalPosition = IntoLocalPosition(new Vector3(currentLocalPosition.x, currentLocalPosition.y));

            try
            {
                if (IsPolygonConvex(_idPointToEdit, currentLocalPosition))
                {
                    _spriteShapeController.spline.SetPosition(_idPointToEdit,
                        currentLocalPosition - _deltaBetweenClickAndSelect);
                    _points[_idPointToEdit].transform
                        .SetLocalPositionAndRotation(currentLocalPosition - _deltaBetweenClickAndSelect,
                            Quaternion.identity);
                }
            }
            catch (Exception e)
            {
                _settingsAndInfo.ShowErrorMessage(e.Message);
            }
        }
    }

    //determines the enter into the Bermuda triangle zone
    private void OnTriggerEnter2D(Collider2D other)
    {
        _settingsAndInfo.AddTextToMagazine(other.gameObject.name + " enter");
        _shipManager.ToggleSignalPlaying(other.gameObject.name, true);
    }

    //determines the exit from the Bermuda triangle zone
    private void OnTriggerExit2D(Collider2D other)
    {
        _settingsAndInfo.AddTextToMagazine(other.gameObject.name + " exit");
        _shipManager.ToggleSignalPlaying(other.gameObject.name, false);
    }

    //Converts global position to local position
    private Vector3 IntoLocalPosition(Vector3 globalPosition)
    {
        return globalPosition - _spriteShapeController.transform.position;
    }

    //Converts local position to global position
    private Vector3 IntoGlobalPosition(Vector3 localPosition)
    {
        return localPosition + _spriteShapeController.transform.position;
    }

    //Detect point, which was selected
    private bool DetectPointSelection(Vector3 clickPosition)
    {
        for (int i = 0; i < _spriteShapeController.spline.GetPointCount(); i++)
        {
            Vector3 deltaPosition = clickPosition - IntoGlobalPosition(_spriteShapeController.spline.GetPosition(i));

            if (deltaPosition.magnitude < DetectionLengthEpsilon)
            {
                if (_isDoubleClick && _spriteShapeController.spline.GetPointCount() > MinimalPointCount)
                {
                    _spriteShapeController.spline.RemovePointAt(i);
                    Destroy(_points[i]);
                    _points.RemoveAt(i);
                    return true;
                }

                if (_spriteShapeController.spline.GetPointCount() == MinimalPointCount)
                {
                    throw new Exception("There is minimal point count, can't remove");
                }

                _isTouch = true;
                _idPointToEdit = i;
                _deltaBetweenClickAndSelect = deltaPosition;
                return true;
            }
        }

        return false;
    }

    //Detecting pointer down, points creation and creation
    public void OnPointerDown(PointerEventData eventData)
    {
        Vector3 clickPosition = Camera.main.ScreenToWorldPoint(eventData.pressPosition);
        clickPosition = new Vector3(clickPosition.x, clickPosition.y);

        DetectDoubleClick();
        try
        {
            if (!DetectPointSelection(clickPosition))
            {
                DetectPointsCreation(clickPosition);
            }
        }
        catch (Exception e)
        {
            _settingsAndInfo.ShowErrorMessage(e.Message);
        }
    }

    //Detecting pointer up
    public void OnPointerUp(PointerEventData eventData)
    {
        _isTouch = false;
        _idPointToEdit = -1;
    }

    //Determines whether a new point has been created
    private void DetectPointsCreation(Vector3 clickPosition)
    {
        for (int i = 0; i < _spriteShapeController.spline.GetPointCount(); i++)
        {
            Vector3 pointsProjection = Vector3.Project(
                IntoLocalPosition(clickPosition) - _spriteShapeController.spline.GetPosition(i),
                _spriteShapeController.spline.GetPosition((i + 1) % _spriteShapeController.spline.GetPointCount()) -
                _spriteShapeController.spline.GetPosition(i)) + _spriteShapeController.spline.GetPosition(i);

            Vector3 deltaPosition = clickPosition - IntoGlobalPosition(pointsProjection);

            if (deltaPosition.magnitude < DetectionLengthEpsilon)
            {
                _deltaBetweenClickAndSelect = deltaPosition;

                _spriteShapeController.spline.InsertPointAt(i + 1, IntoLocalPosition(clickPosition - deltaPosition));
                _spriteShapeController.spline.SetHeight(i + 1, SplineHeight);

                _points.Insert(i + 1,
                    Instantiate(_pointToInstantiate, IntoGlobalPosition(_spriteShapeController.spline.GetPosition(i)),
                        Quaternion.identity, transform));

                _isTouch = true;
                _idPointToEdit = i + 1;
                return;
            }
        }
    }

    //Detect double click
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

    //Define is polygon will be convex, if point with pointId will move into position newPointPosition
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
                throw new Exception("The polygon isn't convex");
            }

            signOfResult = sign;
        }

        return true;
    }
}