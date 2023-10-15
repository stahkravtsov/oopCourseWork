using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

public class BermudianTriangle : MonoBehaviour, IPointerDownHandler, IPointerMoveHandler, IPointerUpHandler
{
   private const float Epsilon = 0.1f;
   
   [SerializeField] private SpriteShapeController _spriteShapeController;

   private bool _isTouch;
   private int _idPointToEdit = -1;
  
   public void OnPointerDown(PointerEventData eventData)
   {
      Vector3 clickPosition = Camera.main.ScreenToWorldPoint(eventData.pressPosition);
      clickPosition = new Vector3(clickPosition.x, clickPosition.y);
      
      Debug.Log("Pointer Clicked at position " + clickPosition);

      for (int i = 0; i < _spriteShapeController.spline.GetPointCount(); i++)
      {
         Vector3 deltaPosition = clickPosition - (_spriteShapeController.spline.GetPosition(i) + _spriteShapeController.transform.position);

         //Debug.Log($" i = {i} splinePos = {_spriteShapeController.spline.GetPosition(i)} delta = {deltaPosition}");
         
         if (Mathf.Abs(deltaPosition.x) < Epsilon && Mathf.Abs(deltaPosition.y) < Epsilon)
         {
            _isTouch = true;
            _idPointToEdit = i;
            return;
         }
      }
   }

   public void OnPointerMove(PointerEventData eventData)
   {
      
      //Debug.Log("OnPointerMove");
      
      if (_isTouch && _idPointToEdit != -1)
      {
         Vector3 currentLocalPosition = Camera.main.ScreenToWorldPoint(eventData.pressPosition);
         currentLocalPosition = new Vector3(currentLocalPosition.x, currentLocalPosition.y) -
                                _spriteShapeController.transform.position;
         
         _spriteShapeController.spline.SetPosition(_idPointToEdit, currentLocalPosition);
      }
   }

   public void OnPointerUp(PointerEventData eventData)
   {
      if (_isTouch && _idPointToEdit != -1)
      {
         Vector3 currentLocalPosition = Camera.main.ScreenToWorldPoint(eventData.pressPosition);
         currentLocalPosition = new Vector3(currentLocalPosition.x, currentLocalPosition.y) -
                                _spriteShapeController.transform.position;
         
         Debug.Log($"OnPointerUp at position {currentLocalPosition} to point {_idPointToEdit}");
         
         _spriteShapeController.spline.SetPosition(_idPointToEdit, currentLocalPosition);
      }
      
      _isTouch = false;
      _idPointToEdit = -1;
   }
}
