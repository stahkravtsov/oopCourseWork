using System;
using UnityEngine;

public class Line
{
    public Vector2 startPoint { get; }
    public Vector2 endPoint { get; }

    public Line(Vector2 start, Vector2 end)
    {
        startPoint = start;
        endPoint = end;
    }

    //Return the angle between start and end point
    public float GatAngle()
    {
        Vector2 delta = startPoint - endPoint;
        float angle = 180 / Mathf.PI * Mathf.Atan((endPoint.y - startPoint.y) / (endPoint.x - startPoint.x)) + 90 +
                      (delta.x < 0 ? 180 : 0);
        return angle;
    }

    //Determines segments intersection, return true and intersect point in args Intersection, otherwise false 
    public static bool SegmentIntersection(out Vector3 intersection, Line currentShipTrajectory,
        Line prevShipTrajectory)
    {
        return SegmentIntersection(out intersection,
            currentShipTrajectory.startPoint,
            currentShipTrajectory.endPoint - currentShipTrajectory.startPoint,
            prevShipTrajectory.startPoint,
            prevShipTrajectory.endPoint - prevShipTrajectory.startPoint);
    }

    //Determines segments intersection, return true and intersect point in args Intersection, otherwise false,
    //take start point and direction vector of each segment
    private static bool SegmentIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1,
        Vector3 linePoint2, Vector3 lineVec2)
    {
        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parrallel
        if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);

            bool val1 = DoesPointBelongsSegment(intersection, linePoint1, linePoint1 + lineVec1);
            bool val2 = DoesPointBelongsSegment(intersection, linePoint2, linePoint2 + lineVec2);

            return val1 && val2;
        }

        intersection = Vector3.zero;
        return false;
    }

    //Determines does point belongs segment, takes point, start and end segment points
    private static bool DoesPointBelongsSegment(Vector3 point, Vector3 aEnd, Vector3 bEnd)
    {
        float minX = Math.Min(aEnd.x, bEnd.x);
        float maxX = Math.Max(aEnd.x, bEnd.x);

        float minY = Math.Min(aEnd.y, bEnd.y);
        float maxY = Math.Max(aEnd.y, bEnd.y);

        return !(point.x > maxX) && !(point.x < minX) && !(point.y > maxY) && !(point.y < minY);
    }
}