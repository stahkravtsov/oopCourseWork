using UnityEngine;
using UnityEngine.Events;

public class Ship : MonoBehaviour
{
    private static int _uniqueShipNumber;

    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private LineRenderer _lineTemplate;
    [SerializeField] private GameObject _parent;
    [SerializeField] private SettingsAndInfo _settingsAndInfo;

    public UnityEvent<Ship> removeFromListEvent = new UnityEvent<Ship>();
    public float Speed => _settingsAndInfo.ShipSpeed;
    public bool IsMoving { get; private set; }
    public Line Trajectory { get; private set; }
    public Line LeftTrajectory { get; private set; }
    public Line RightTrajectory { get; private set; }
    public Vector3[,] SizeOffset { get; } = new Vector3[2, 2];

    private LineRenderer _visualizationTrajectory;

    //Moves the ship forward and destroys it as it moves to the end
    private void Update()
    {
        if (!IsMoving)
        {
            return;
        }

        transform.localPosition = Vector2.MoveTowards(transform.localPosition, Trajectory.endPoint,
            Speed * Time.deltaTime);

        if (transform.localPosition.Equals(Trajectory.endPoint))
        {
            removeFromListEvent?.Invoke(this);
            Destroy(this);
        }
    }

    //Destroy the ship 
    public void OnDestroy()
    {
        removeFromListEvent = null;
        Destroy(_visualizationTrajectory.gameObject);
        Destroy(gameObject);
    }

    //Return integer unique number representing count of ships
    private static int GetUniqueNumber()
    {
        return _uniqueShipNumber++;
    }

    //Starting or stop playing a signal
    public void ToggleSignalPlaying(bool isPlaying)
    {
        if (isPlaying)
        {
            _audioSource.Play();
        }
        else
        {
            _audioSource.Stop();
        }
    }

    //Initializing ships, and set up they properties
    public void InitializeShip(Vector2 start, Vector2 end)
    {
        Trajectory = new Line(start, end);
        SetUpShipTransform();
        DrawTrajectory();

        Vector2 localScale = transform.localScale;
        Vector2 rightOffset = Vector2.Perpendicular(end - start).normalized * localScale * 0.5f;
        Vector2 frontOffset = (end - start).normalized * localScale;

        RightTrajectory = new Line(start + rightOffset, end + rightOffset);
        LeftTrajectory = new Line(start - rightOffset, end - rightOffset);

        SizeOffset[0, 0] = frontOffset - rightOffset;
        SizeOffset[0, 1] = frontOffset + rightOffset;
        SizeOffset[1, 0] = -frontOffset - rightOffset;
        SizeOffset[1, 1] = -frontOffset + rightOffset;

        gameObject.name = GetUniqueNumber().ToString();
    }

    //Setting move state
    public void MoveShip(bool isMoving)
    {
        IsMoving = isMoving;
    }

    //Set up ship position and quaternion
    private void SetUpShipTransform()
    {
        transform.localPosition = Trajectory.startPoint;
        transform.localRotation = Quaternion.AngleAxis(Trajectory.GatAngle(), Vector3.forward);
    }

    //draw trajectory
    private void DrawTrajectory()
    {
        _visualizationTrajectory = Instantiate(_lineTemplate, _parent.transform, false);
        _visualizationTrajectory.transform.localPosition = Vector3.zero;
        _visualizationTrajectory.SetPosition(0, Trajectory.startPoint);
        _visualizationTrajectory.SetPosition(1, Trajectory.endPoint);
    }
}