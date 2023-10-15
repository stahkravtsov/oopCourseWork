using System;
using UnityEngine;

public class Line
{
    public Vector2 startPoint;
    public Vector2 endPoint;

    private float _k;
    private float _b;

    public Line(Vector2 start, Vector2 end)
    {
        startPoint = start;
        endPoint = end;

        _k = (end.y - start.y) / (end.x - start.x);
        _b = start.y - start.x * _k;
    }

    public bool IsLinesIntersection(Line secondLine)
    {
        return false;
    }

    public double GetYByX(double x)
    {
        return _k * x + _b;
    }

    public float GatAngle()
    {
        Vector2 delta = startPoint - endPoint;
        float angle = 180 / Mathf.PI * Mathf.Atan(_k) + 90 + (delta.x < 0 ? 180 : 0);
        return angle;
    }
    
    
    public static bool FasterLineSegmentIntersection(Line first, Line second)
    {
        Vector2 a = first.endPoint - first.startPoint;
        Vector2 b = second.startPoint - second.endPoint;
        Vector2 c = first.startPoint - second.startPoint;

        float alphaNumerator = b.y * c.x - b.x * c.y;
        float alphaDenominator = a.y * b.x - a.x * b.y;
        float betaNumerator = a.x * c.y - a.y * c.x;
        float betaDenominator = a.y * b.x - a.x * b.y;

        bool doIntersect = true;

        if (alphaDenominator == 0 || betaDenominator == 0)
        {
            doIntersect = false;
        }
        else
        {
            if (alphaDenominator > 0)
            {
                if (alphaNumerator < 0 || alphaNumerator > alphaDenominator)
                {
                    doIntersect = false;
                }
            }
            else if (alphaNumerator > 0 || alphaNumerator < alphaDenominator)
            {
                doIntersect = false;
            }

            if (doIntersect && betaDenominator > 0)
            {
                if (betaNumerator < 0 || betaNumerator > betaDenominator)
                {
                    doIntersect = false;
                }
            }
            else if (betaNumerator > 0 || betaNumerator < betaDenominator)
            {
                doIntersect = false;
            }
        }

        return doIntersect;
    }
}