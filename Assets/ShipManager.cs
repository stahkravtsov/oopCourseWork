using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ShipManager : MonoBehaviour
{
    private const int MinDelayBetweenShipSpawn = 100;
    private const int MaxDelayBetweenShipSpawn = 500;

    [SerializeField] private Ship _shipTemplate;
    [SerializeField] private Lake _lake;
    [SerializeField] private SettingsAndInfo _settingsAndInfo;

    private readonly List<Ship> _ships = new List<Ship>();

    private bool _areShipsSpawning;

    private void Awake()
    {
        _lake.RenderLake();
        SpawnAllRequiredShip();
    }

    private void Update()
    {
        ContinueShipMoving();
    }

    //Toggle signal playing when ship crosses Bermudian Triangle, take shipName and state to set isPlaying
    public void ToggleSignalPlaying(string shipName, bool isPlaying)
    {
        _ships.Find(ship => ship.gameObject.name == shipName).ToggleSignalPlaying(isPlaying);
    }

    //Spawning ships at the start and each time one ship ended moving inside lake
    private async void SpawnAllRequiredShip()
    {
        if (_areShipsSpawning)
        {
            return;
        }

        _areShipsSpawning = true;
        int countShipInLake = 0;

        try
        {
            countShipInLake = Lake.GetRandomInt(_settingsAndInfo.MinShipCount, _settingsAndInfo.MaxShipCount);
        }
        catch (Exception e)
        {
            _settingsAndInfo.ShowErrorMessage(e.Message);
        }

        for (int i = _ships.Count; i < countShipInLake; i++)
        {
            await UniTask.Delay(Lake.GetRandomInt(MinDelayBetweenShipSpawn, MaxDelayBetweenShipSpawn));
            SpawnShip();
        }

        _areShipsSpawning = false;
    }

    //Spawn ship, and begin moving
    private void SpawnShip()
    {
        _ships.Add(Instantiate(_shipTemplate, _lake.transform, false));
        _ships[^1].InitializeShip(_lake.GetRandomPoint(), _lake.GetRandomPoint());

        bool canMove = true;

        for (int i = 0; i < _ships.Count - 1; i++)
        {
            bool doesLineIntersect = DoesShipTrajectoryIntersect(^1, i);

            if (doesLineIntersect)
            {
                canMove = false;
            }
        }

        _ships[^1].MoveShip(canMove);
        _ships[^1].removeFromListEvent.AddListener(RemoveShipFromList);
    }

    //Remove ship from list
    private void RemoveShipFromList(Ship ship)
    {
        _ships.Remove(ship);
        SpawnAllRequiredShip();
    }

    //Analyzes all trajectories of ships, determines their intersection, and the ship that is most profitable to stop
    private void ContinueShipMoving()
    {
        for (int i = 0; i < _ships.Count; i++)
        {
            bool isMoving = true;

            for (int j = 0; j < _ships.Count; j++)
            {
                if (i == j)
                {
                    continue;
                }

                bool doesShipIntersect = DoesShipTrajectoryIntersect(i, j);

                if (doesShipIntersect)
                {
                    isMoving = isMoving && !DoesNeedToStop(i, j);
                }
            }

            _ships[i].MoveShip(isMoving);
        }
    }

    //Determines whether the trajectories of ships with indexes in the list current and counter intersect
    private bool DoesShipTrajectoryIntersect(Index current, Index counter)
    {
        if (!GetIntersectPoints(out Vector3[,] intersection, current, counter))
        {
            return false;
        }

        GetTotalTimeToInAndOut(out float minCurrentShip, out float maxCurrentShip, current, intersection);
        GetIntersectPoints(out intersection, counter, current);
        GetTotalTimeToInAndOut(out float minPrevShip, out float maxPrevShip, counter, intersection);

        if (minCurrentShip > maxPrevShip || minPrevShip > maxCurrentShip || maxCurrentShip < 0 || maxPrevShip < 0)
        {
            return false;
        }

        return true;
    }

    //Determines points od intersection of ships with indexes in the list current and counter
    private bool GetIntersectPoints(out Vector3[,] intersection, Index current, Index counter)
    {
        intersection = new Vector3[2, 2];

        bool llLineIntersect = Line.SegmentIntersection(out intersection[0, 0], _ships[current].LeftTrajectory,
            _ships[counter].LeftTrajectory);
        bool lrLineIntersect = Line.SegmentIntersection(out intersection[0, 1], _ships[current].LeftTrajectory,
            _ships[counter].RightTrajectory);
        bool rlLineIntersect = Line.SegmentIntersection(out intersection[1, 0], _ships[current].RightTrajectory,
            _ships[counter].LeftTrajectory);
        bool rrLineIntersect = Line.SegmentIntersection(out intersection[1, 1], _ships[current].RightTrajectory,
            _ships[counter].RightTrajectory);

        return llLineIntersect || lrLineIntersect || rlLineIntersect || rrLineIntersect;
    }

    //Determines time to in and time to out from area surrounded by intersection points
    private void GetTotalTimeToInAndOut(out float min, out float max, Index current, Vector3[,] intersection)
    {
        float lMin = Math.Min(
            GetDistanceToPointConsideringDirection(_ships[current].SizeOffset[0, 0], intersection[0, 0], current),
            GetDistanceToPointConsideringDirection(_ships[current].SizeOffset[0, 0], intersection[0, 1], current));

        float lMax = Math.Max(
            GetDistanceToPointConsideringDirection(_ships[current].SizeOffset[1, 0], intersection[0, 0], current),
            GetDistanceToPointConsideringDirection(_ships[current].SizeOffset[1, 0], intersection[0, 1], current));

        float rMin = Math.Min(
            GetDistanceToPointConsideringDirection(_ships[current].SizeOffset[0, 1], intersection[1, 0], current),
            GetDistanceToPointConsideringDirection(_ships[current].SizeOffset[0, 1], intersection[1, 1], current));

        float rMax = Math.Max(
            GetDistanceToPointConsideringDirection(_ships[current].SizeOffset[1, 1], intersection[1, 0], current),
            GetDistanceToPointConsideringDirection(_ships[current].SizeOffset[1, 1], intersection[1, 1], current));

        max = Math.Max(lMax, rMax) / _ships[current].Speed;
        min = Math.Min(lMin, rMin) / _ships[current].Speed;
    }

    //Determines distance to point considering direction of move
    private float GetDistanceToPointConsideringDirection(Vector3 currentPointOffset, Vector3 intersectionPoint,
        Index current)
    {
        Vector3 pointWay =
            intersectionPoint - (_ships[current].gameObject.transform.localPosition + currentPointOffset);
        float magnitude = (pointWay).magnitude;
        Vector3 way = (_ships[current].Trajectory.endPoint - _ships[current].Trajectory.startPoint);

        if (!(Math.Sign(way.x) == Math.Sign(pointWay.x) && Math.Sign(way.y) == Math.Sign(pointWay.y)))
        {
            magnitude *= -1f;
        }

        return magnitude;
    }

    //Determines does current ship need to stop
    private bool DoesNeedToStop(Index current, Index counter)
    {
        GetIntersectPoints(out Vector3[,] intersection, current, counter);
        GetTotalTimeToInAndOut(out float minCurrentShip, out float maxCurrentShip, current, intersection);

        GetIntersectPoints(out intersection, counter, current);
        GetTotalTimeToInAndOut(out float minPrevShip, out float maxPrevShip, counter, intersection);

        bool r1 = maxPrevShip - minCurrentShip < maxCurrentShip - minPrevShip;
        bool r2 = _ships[counter].IsMoving || (!_ships[counter].IsMoving && minPrevShip < 0 && maxPrevShip > 0);
        return r1 && r2;
    }
}