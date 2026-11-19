using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapPlayer : MonoBehaviour
{
  private PlayerController controls;

  public event Action<MapDirection> OnMoveInput;
  public event Action OnConfirmInput;

  private void Awake() 
  {
    controls = new PlayerController();
  }

  void OnEnable()
  {
    if (controls == null)
      return;
    controls.Map.Interact.performed += HandleInteract; 
    controls.Map.Move.performed += HandleMove;
    controls.Map.Enable();
  }

  void OnDisable()
  {
    if (controls == null)
      return;
    controls.Map.Interact.performed -= HandleInteract; 
    controls.Map.Move.performed -= HandleMove;
    controls.Map.Disable();
  }

  void OnDestroy()
  {
    controls?.Dispose();
  }

  private void HandleInteract(InputAction.CallbackContext ctx)
  {
    OnConfirmInput?.Invoke();
  }

  private void HandleMove(InputAction.CallbackContext ctx)
  {
    Vector2 input = ctx.ReadValue<Vector2>();

    if (TryGetDirection(input, out MapDirection direction))
      OnMoveInput?.Invoke(direction);
  }

  private bool TryGetDirection(Vector2 input, out MapDirection direction)
  {
    direction = default;

    if (input.magnitude < 0.6f)
      return false;

    float absX = Mathf.Abs(input.x);
    float absY = Mathf.Abs(input.y);

    if (Mathf.Abs(absX - absY) < 0.1f)
      return false;

    if (absX > absY)
      direction = input.x > 0 ? MapDirection.East : MapDirection.West;
    else
      direction = input.y > 0 ? MapDirection.North : MapDirection.South;

    return true;
  }

  public void MoveTo(Vector3 position)
  {
    //TODO: Criar funcao para mover posição personagem com uma pequena animação (colocar animação em outra função com coroutine)
  }
  
  public void SetPosition(Vector3 position)
  {
    transform.position = position;
  }
}

