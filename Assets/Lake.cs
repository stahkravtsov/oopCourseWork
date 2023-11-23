using System;
using UnityEngine;
using UnityEngine.U2D;
using Random = System.Random;

public class Lake : MonoBehaviour
{
    private static readonly Random CustomRandom = new Random();

    private readonly Vector2[] _pointToRenderLake = new[]
    {
        new Vector2(-6f, -3f),
        new Vector2(-4.5f, 3f),

        new Vector2(-4.5f, 3f),
        new Vector2(4.5f, 4.5f),

        new Vector2(4.5f, -3f),
        new Vector2(6f, 3f),

        new Vector2(-4.5f, -4.5f),
        new Vector2(4.5f, -3f)
    };

    private readonly Vector2[] _tangentVector2 = new[]
    {
        Vector2.up, Vector2.right, Vector2.down, Vector2.left
    };

    [SerializeField] private SpriteShapeController _spriteShapeController;

    private EdgeCollider2D _edgeCollider;
    private bool _isFirstRandomGetting = true;
    private float _lakePerimeter;

    //Return random number from range
    public static int GetRandomInt(int min, int max)
    {
        if (min > max)
        {
            throw new Exception("Incorrect range of ship count");
        }

        return CustomRandom.Next(min, max);
    }

    //Render unique lake at Start of game, generated points are inside _pointToRenderLake
    public void RenderLake()
    {
        Spline spline = _spriteShapeController.spline;

        for (int i = 0; i < _spriteShapeController.spline.GetPointCount(); i++)
        {
            spline.RemovePointAt(i);

            spline.InsertPointAt(i,
                new Vector3(
                    _pointToRenderLake[i * 2].x + (float)CustomRandom.NextDouble() *
                    (_pointToRenderLake[i * 2 + 1].x - _pointToRenderLake[i * 2].x),
                    _pointToRenderLake[i * 2].y + (float)CustomRandom.NextDouble() *
                    (_pointToRenderLake[i * 2 + 1].y - _pointToRenderLake[i * 2].y)));

            spline.SetHeight(i, 0);

            spline.SetTangentMode(i, ShapeTangentMode.Continuous);

            spline.SetLeftTangent(i, _tangentVector2[i] * -1f);
            spline.SetRightTangent(i, _tangentVector2[i]);
        }

        _spriteShapeController.BakeCollider();
        _edgeCollider = _spriteShapeController.edgeCollider;
    }

    //Finding perimeter of Lake, adding distance between each pair of render points
    private void FindLakePerimeter()
    {
        Vector2[] points = _edgeCollider.points;

        for (int i = 0; i < points.Length - 1; i++)
        {
            _lakePerimeter += (points[i + 1] - points[i]).magnitude;
        }

        _lakePerimeter += (points[0] - points[^1]).magnitude;
    }

    //Getting random point at the edge of lake
    public Vector2 GetRandomPoint()
    {
        Vector2[] points = _edgeCollider.points;

        if (_isFirstRandomGetting)
        {
            FindLakePerimeter();
            _isFirstRandomGetting = false;
        }

        float padding = (float)(_lakePerimeter * CustomRandom.NextDouble());
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
}