using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
#if !ENABLE_LEGACY_INPUT_MANAGER && ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;

#endif

namespace Adinmo
{
    public static class AdinmoInputHandler
    {
        [Preserve]
        public static Vector2 GetPotentialTouchPos()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WebGLPlayer)
            {

                if (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(0))
                {
                    return Input.mousePosition;
                }
            }
            else
            {
                if (Input.touchCount == 1)
                {
                    return Input.touches[0].position;
                }
            }

#elif ENABLE_INPUT_SYSTEM
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor  || Application.platform == RuntimePlatform.WebGLPlayer)
            {

                if (Mouse.current.leftButton.isPressed || Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    return Mouse.current.position.ReadValue();
                }
            }
            else
            {
                if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count == 1)
                {
                    return UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0].screenPosition;
                }
            }


#endif
            return -Vector2.one;
        }

        [Preserve]
        public static UnityEngine.Touch GetTouch()
        {
            UnityEngine.Touch touch = new();
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WebGLPlayer)
            {
                touch.position = Input.mousePosition;
                if (Input.mousePosition.x < 0 || Input.mousePosition.y < 0 || Input.mousePosition.x > Screen.width || Input.mousePosition.y > Screen.height)
                {
                    touch.phase = UnityEngine.TouchPhase.Canceled;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    touch.phase = UnityEngine.TouchPhase.Ended;
                }
                else if (Input.GetMouseButtonDown(0))
                {
                    touch.phase = UnityEngine.TouchPhase.Began;
                }
                else if (Input.GetMouseButton(0))
                {
                    touch.phase = UnityEngine.TouchPhase.Moved;
                }
            }
            else
            {
                touch = Input.GetTouch(0);
            }
#elif ENABLE_INPUT_SYSTEM
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WebGLPlayer)
            {
                touch.position = Mouse.current.position.ReadValue();
                if (touch.position.x < 0 || touch.position.y < 0 || touch.position.x > Screen.width || touch.position.y > Screen.height)
                {
                    touch.phase = UnityEngine.TouchPhase.Canceled;
                }
                else if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    touch.phase = UnityEngine.TouchPhase.Ended;
                }
                else if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    touch.phase = UnityEngine.TouchPhase.Began;
                }
                else if (Mouse.current.leftButton.isPressed)
                {
                    touch.phase = UnityEngine.TouchPhase.Moved;
                }
            }
            else
            {
                UnityEngine.InputSystem.EnhancedTouch.Touch enhancedTouch = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0];
                touch.position = enhancedTouch.screenPosition;
                switch (enhancedTouch.phase)
                {
                    case UnityEngine.InputSystem.TouchPhase.Began:
                        touch.phase = UnityEngine.TouchPhase.Began;
                        break;
                    case UnityEngine.InputSystem.TouchPhase.Canceled:
                        touch.phase = UnityEngine.TouchPhase.Canceled;
                        break;
                    case UnityEngine.InputSystem.TouchPhase.Ended:
                        touch.phase = UnityEngine.TouchPhase.Ended;
                        break;
                    case UnityEngine.InputSystem.TouchPhase.Moved:
                        touch.phase = UnityEngine.TouchPhase.Moved;
                        break;
                }

            }

#endif
            return touch;
        }
        [Preserve]
        public static void EnableEnhancedTouch()
        {
#if !ENABLE_LEGACY_INPUT_MANAGER && ENABLE_INPUT_SYSTEM
            UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
#endif
        }
        [Preserve]
        public static void Vibrate()
        {
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }
    }


}


