using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#if ANDROID
using Microsoft.Xna.Framework.Input.Touch;
#endif

namespace InputManager
{
    public class InputEngine : GameComponent
    {
        // User-configurable.
        const float Deadzone = 0.25f;
        //const float DiagonalAvoidance = 0.2f;

        public static Color clearColor;
#if ANDROID
        static TouchCollection currentTouchState;
        public static GestureType currentGestureType = GestureType.None;
        public static Vector2 PreviousGesturePosition;
        public static Vector2 CurrentGesturePosition;
        static public TouchCollection CurrentTouchState
        {
            get
            {
                return currentTouchState;
            }

            set
            {
                currentTouchState = value;
            }
        }


#endif

        private static GamePadState previousPadState;
        private static GamePadState currentPadState;

        private static KeyboardState previousKeyState;
        private static KeyboardState currentKeyState;

        private static Vector2 previousMousePos;
        private static Vector2 currentMousePos;

        private static MouseState previousMouseState;
        private static MouseState currentMouseState;

        private static Vector2 thumbStick;

        private static float m_Time;
        private float m_Ratio;
        private static float m_CurrTime;
        private static float m_LeftMotor;
        private static float m_RightMotor;

        public static bool UsingKeyboard = true;

        public InputEngine(Game _game)
            : base(_game)
        {


#if ANDROID
            CurrentTouchState = TouchPanel.GetState();
#endif

            currentPadState = GamePad.GetState(PlayerIndex.One);
            currentKeyState = Keyboard.GetState();

            _game.Components.Add(this);
        }

        public new void Dispose()
        {
            GamePad.SetVibration(0, 0, 0);
        }

        private Vector2 ApplyDeadZone(GamePadThumbSticks ThumbStick)
        {
            // Scaled Radial Dead Zone
            Vector2 stickDirection = new Vector2(ThumbStick.Right.X,
                                                -ThumbStick.Right.Y);

            if (stickDirection.Length() < Deadzone)
            {
                stickDirection = Vector2.Zero;
            }
            else
            {
                stickDirection.Normalize();
                stickDirection *= ((stickDirection.Length() - Deadzone) / (1 - Deadzone));
            }

            return stickDirection;
        }

        public static void ClearState()
        {
            previousMouseState = Mouse.GetState();
            currentMouseState = Mouse.GetState();
            previousKeyState = Keyboard.GetState();
            currentKeyState = Keyboard.GetState();

#if ANDROID
            CurrentGesturePosition = PreviousGesturePosition;
            currentGestureType = GestureType.None;
#endif
        }

        public override void Update(GameTime gametime)
        {
#if ANDROID
            CurrentTouchState = TouchPanel.GetState();
            if (CurrentTouchState.Count == 1)
            {
                PreviousGesturePosition = CurrentGesturePosition;
                CurrentGesturePosition = CurrentTouchState[0].Position;
            }

            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample gesture = TouchPanel.ReadGesture();
                switch (gesture.GestureType)
                {
                    case GestureType.None:
                        clearColor = Color.CornflowerBlue;
                        break;
                    case GestureType.DoubleTap:
                        clearColor = clearColor == Color.CornflowerBlue ? Color.Red : Color.CornflowerBlue;
                        break;
                    case GestureType.Tap:
                        clearColor = clearColor == Color.CornflowerBlue ? Color.Black : Color.CornflowerBlue;
                        break;
                    case GestureType.Flick:
                        clearColor = clearColor == Color.CornflowerBlue ? Color.Pink : Color.CornflowerBlue;
                        break;
                    case GestureType.HorizontalDrag:
                        clearColor = clearColor == Color.CornflowerBlue ? Color.Peru : Color.CornflowerBlue;
                        break;
                    case GestureType.VerticalDrag:
                        clearColor = clearColor == Color.CornflowerBlue ? Color.Wheat : Color.CornflowerBlue;
                        break;

                }
                currentGestureType = gesture.GestureType;
            }
#endif
            PreviousPadState = currentPadState;
            previousKeyState = currentKeyState;

            currentPadState = GamePad.GetState(PlayerIndex.One);
            currentKeyState = Keyboard.GetState();


            previousMouseState = currentMouseState;
            currentMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            currentMouseState = Mouse.GetState();

            KeysPressedInLastFrame.Clear();
            CheckForTextInput();

            UpdateControls();
            thumbStick = ApplyDeadZone(currentPadState.ThumbSticks);

            UpdateRumble((float)gametime.ElapsedGameTime.TotalSeconds);

            base.Update(gametime);
        }

        public List<string> KeysPressedInLastFrame = new List<string>();

        private void CheckForTextInput()
        {
            foreach (var key in Enum.GetValues(typeof(Keys)) as Keys[])
            {
                if (IsKeyPressed(key))
                {
                    KeysPressedInLastFrame.Add(key.ToString());
                    break;
                }
            }
        }

        public static bool IsButtonPressed(Buttons buttonToCheck)
        {
            if (currentPadState.IsButtonUp(buttonToCheck) && PreviousPadState.IsButtonDown(buttonToCheck))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool IsButtonHeld(Buttons buttonToCheck)
        {
            if (currentPadState.IsButtonDown(buttonToCheck))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsKeyHeld(Keys buttonToCheck)
        {
            if (currentKeyState.IsKeyDown(buttonToCheck))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public static bool IsKeyPressed(Keys keyToCheck)
        {
            if (currentKeyState.IsKeyUp(keyToCheck) && previousKeyState.IsKeyDown(keyToCheck))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static GamePadState CurrentPadState
        {
            get { return currentPadState; }
            set { currentPadState = value; }
        }
        public static KeyboardState CurrentKeyState
        {
            get { return currentKeyState; }
        }

        public static MouseState CurrentMouseState
        {
            get { return currentMouseState; }
        }

        public static MouseState PreviousMouseState
        {
            get { return previousMouseState; }
        }

        public static bool IsMouseLeftClick()
        {
            if (currentMouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed)
                return true;
            else
                return false;
        }

        public static bool IsMouseRightClick()
        {
            if (currentMouseState.RightButton == ButtonState.Released && previousMouseState.RightButton == ButtonState.Pressed)
                return true;
            else
                return false;
        }

        public static bool IsMouseRightHeld()
        {
            if (currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Pressed)
                return true;
            else
                return false;
        }

        public static bool IsMouseLeftHeld()
        {
            if (currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Pressed)
                return true;
            else
                return false;
        }

        public static Vector2 MousePosition
        {
            get { return currentMousePos; }
        }

        public static GamePadState PreviousPadState
        {
            get
            {
                return previousPadState;
            }

            set
            {
                previousPadState = value;
            }
        }

        public static Vector2 SmoothThumbStick
        {
            get { return thumbStick; }
            set { thumbStick = value; }
        }

        public static bool IsPadInputChanged(bool ignoreThumbsticks)
        {
            if ((currentPadState.IsConnected) && (currentPadState.PacketNumber != previousPadState.PacketNumber))
            {
                // Ignore thumbstick movement.
                if (ignoreThumbsticks == true
                    && ((currentPadState.ThumbSticks.Left.Length() != previousPadState.ThumbSticks.Left.Length())
                    && (currentPadState.ThumbSticks.Right.Length() != previousPadState.ThumbSticks.Right.Length()))
                    )
                    return false;
                return true;
            }
            return false;
        }

        private static List<Keys> PressedKeys = new List<Keys>()
        {
            #region Used Keys
            Keys.W,
            Keys.S,
            Keys.A,
            Keys.D,
            Keys.C,
            Keys.Enter,
            Keys.Escape,
            Keys.Space
            #endregion
        };

        public static bool IsKeyInputChanged()
        {
            foreach (var key in PressedKeys)
            {
                if (IsKeyPressed(key) || IsKeyHeld(key))
                {
                    return true;
                }
                else return false;
            }
            return false;
        }

        private void UpdateControls()
        {
            if (IsPadInputChanged(false))
            {
                UsingKeyboard = false;
            }

            if (CurrentMouseState != PreviousMouseState || IsKeyInputChanged())
            {
                UsingKeyboard = true;
            }
        }

        private void UpdateRumble(float TimeElapsed)
        {
            //if (GlobalObjects.m_Settings.m_RumbleEnabled)
            //{
            m_CurrTime -= TimeElapsed;
            m_Ratio = m_CurrTime / m_Time;
            if (m_CurrTime <= 0)
            {
                GamePad.SetVibration(0, 0, 0);
            }
            else
            {
                GamePad.SetVibration(0, m_LeftMotor * m_Ratio, m_RightMotor * m_Ratio);
            }
            //}
        }

        public static void ShakePad(float Time, float LeftMotor, float RightMotor)
        {
            m_LeftMotor = LeftMotor;
            m_RightMotor = RightMotor;
            m_CurrTime = m_Time = Time;
        }
    }
}
