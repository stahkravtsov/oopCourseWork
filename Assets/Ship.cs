using UnityEngine;
using UnityEngine.Events;

public class Ship : MonoBehaviour
{
     [SerializeField] private LineRenderer _lineTemplate;
     [SerializeField] private GameObject parent;
     [SerializeField] [Range(0,10f)]  private float _shipSpeed;

     public UnityEvent<Ship> RemoveFromListEvent = new UnityEvent<Ship>();

     private Line _trajectory;
     private LineRenderer _visualizationTrajectory;
     private bool _isMoving;

     private void Update()
     {
          if (!_isMoving)
          {
               return;
          }
          
          transform.localPosition = Vector2.MoveTowards(transform.localPosition, _trajectory.endPoint,
               _shipSpeed * Time.deltaTime);
               
          if (transform.localPosition.Equals(_trajectory.endPoint))
          {
               RemoveFromListEvent?.Invoke(this);
               Destroy(this);
          }
     }

     public void OnDestroy()
     {
          RemoveFromListEvent = null;
          Destroy(_visualizationTrajectory.gameObject);
          Destroy(gameObject);
     }

     public Line GetTrajectory()
     {
          return _trajectory;
     }

     public void MoveShip(bool isMoving)
     {
          _isMoving = isMoving;
     }
     
     public void InitializeShip(Vector2 start, Vector2 end)
     {
          _trajectory = new Line(start, end);
          SetUpShipTransform();
          DrawTrajectory();
     }

     private void SetUpShipTransform()
     {
          transform.localPosition = _trajectory.startPoint;
          Quaternion quaternion = Quaternion.AngleAxis(_trajectory.GatAngle(), Vector3.forward);
          transform.localRotation = Quaternion.AngleAxis(_trajectory.GatAngle(), Vector3.forward);
     }

     private void DrawTrajectory()
     {
          _visualizationTrajectory = Instantiate(_lineTemplate, parent.transform, false);
          _visualizationTrajectory.transform.localPosition = Vector3.zero;
          _visualizationTrajectory.SetPosition(0, _trajectory.startPoint);
          _visualizationTrajectory.SetPosition(1, _trajectory.endPoint);
     }
}
