using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using Random = System.Random;

public class SheepManager : MonoBehaviour
{
    private const int MinDelayBetweenShipSpawn = 100;
    private const int MaxDelayBetweenShipSpawn = 500;

    private readonly Vector2[] _areas = new[]
    {
        new Vector2(-6f, -1.5f),
        new Vector2(-4.5f, 1.5f),

        new Vector2(-4.5f, 1.5f),
        new Vector2(4.5f, 3f),

        new Vector2(4.5f, -1.5f),
        new Vector2(6f, 1.5f),

        new Vector2(-4.5f, -3f),
        new Vector2(4.5f, -1.5f)
    };

    private readonly Vector2[] _tangentVector2 = new[]
    {
        Vector2.up, Vector2.right, Vector2.down, Vector2.left
    };

    private readonly Random _random = new Random();

    private EdgeCollider2D _edgeCollider;
    [SerializeField] private SpriteShapeController _spriteShapeController;
    [SerializeField] private Ship _ship;
    [SerializeField] private int MinCountShipInLake;
    [SerializeField] private int MaxCountShipInLake;

    private readonly List<Ship> _ships = new List<Ship>();
    
    private bool _areShipsSpawning;
    private bool _isFirstRandom = true;
    private int _uniqueShipNumber;
    private int _countShipInLake;
    private float _lakePerimeter;

    private void Awake()
    {
        RenderLake();
    }

    private void Start()
    {
        SpawnAllRequiredShip();
    }

    private void RenderLake()
    {
        Spline spline = _spriteShapeController.spline;

        for (int i = 0; i < _spriteShapeController.spline.GetPointCount(); i++)
        {
            spline.RemovePointAt(i);

            spline.InsertPointAt(i,
                new Vector3(_areas[i * 2].x + (float)_random.NextDouble() * (_areas[i * 2 + 1].x - _areas[i * 2].x),
                    _areas[i * 2].y + (float)_random.NextDouble() * (_areas[i * 2 + 1].y - _areas[i * 2].y)));

            spline.SetHeight(i, 0);

            spline.SetTangentMode(i, ShapeTangentMode.Continuous);

            spline.SetLeftTangent(i, _tangentVector2[i] * -1f);
            spline.SetRightTangent(i, _tangentVector2[i]);
        }

        _spriteShapeController.BakeCollider();
        _edgeCollider = _spriteShapeController.edgeCollider;
    }

    private void FindLakePerimeter()
    {
        Vector2[] points = _edgeCollider.points;

        for (int i = 0; i < points.Length - 1; i++)
        {
            _lakePerimeter += (points[i + 1] - points[i]).magnitude;
        }

        _lakePerimeter += (points[0] - points[^1]).magnitude;
    }

    private async void SpawnAllRequiredShip()
    {
        if (_areShipsSpawning)
        {
            return;
        }

        _areShipsSpawning = true;

        _countShipInLake = _random.Next(MinCountShipInLake, MaxCountShipInLake);

        for (int i = _ships.Count; i < _countShipInLake; i++)
        {
            await UniTask.Delay(_random.Next(MinDelayBetweenShipSpawn, MaxDelayBetweenShipSpawn));
            SpawnShip();
        }

        _areShipsSpawning = false;
    }

    private void SpawnShip()
    {
        _ships.Add(Instantiate(_ship, transform, false));
        _ships[^1].InitializeShip(GetRandomPoint(), GetRandomPoint());
        _ships[^1].gameObject.name += _uniqueShipNumber++;

        Ship currentShip = _ships[^1];

        _ships[^1].MoveShip(true);
        _ships[^1].RemoveFromListEvent.AddListener(RemoveShipFromList);
    }

    private Vector2 GetRandomPoint()
    {
        Vector2[] points = _edgeCollider.points;

        if (_isFirstRandom)
        {
            FindLakePerimeter();
            _isFirstRandom = false;
        }

        float padding = (float)(_lakePerimeter * _random.NextDouble());
        float currentLength = 0f;
        int i = 0;

        do
        {
            padding -= currentLength;
            currentLength = (points[(i + 1) % points.Length] - points[i % points.Length]).magnitude;
            i++;
        } while (padding - currentLength > 0f);

        Vector2 vector2 = points[i % points.Length] - points[(i - 1) % points.Length];

        return points[(i - 1) % points.Length] + vector2 * (padding / vector2.magnitude);
    }

    private void RemoveShipFromList(Ship ship)
    {
        _ships.Remove(ship);
        SpawnAllRequiredShip();
    }
}