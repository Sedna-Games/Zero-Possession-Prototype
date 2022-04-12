using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
#endif

namespace InputManagerScript
{
	public class InputManager : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move, look;
        public bool jump, dash, slide, reload;

		[Header("Movement Settings")]
		public bool analogMovement;

#if !UNITY_IOS || !UNITY_ANDROID
		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;
#endif

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnDash(InputValue value)
		{
			DashInput(value.isPressed);
		}
		public void OnSlide(InputValue value)
		{
            SlideInput(value.isPressed);
        }
		public void OnReload(InputValue value)
		{
            ReloadInput(value.isPressed);
        }
#else
	// old input sys if we do decide to have it (most likely wont)...
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		}

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void DashInput(bool newDashState)
		{
			dash = newDashState;
		}
		public void SlideInput(bool newSlideState)
		{
			slide = newSlideState;
		}
		public void ReloadInput(bool newReloadState)
		{
			reload = newReloadState;
		}

#if !UNITY_IOS || !UNITY_ANDROID

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}

#endif

	}

}