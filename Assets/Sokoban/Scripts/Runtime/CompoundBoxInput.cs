using System;
using UnityEngine.InputSystem;

namespace CompoundBox
{
    public sealed class CompoundBoxInput : IDisposable
    {
        private readonly InputActionMap gameplay;
        private readonly InputAction moveUp;
        private readonly InputAction moveRight;
        private readonly InputAction moveDown;
        private readonly InputAction moveLeft;
        private readonly InputAction confirm;
        private readonly InputAction advance;
        private readonly InputAction levelSelect;
        private readonly InputAction previousLevel;
        private readonly InputAction nextLevel;
        private readonly InputAction restart;
        private readonly InputAction undo;
        private readonly InputAction redo;
        private readonly InputAction split;
        private readonly InputAction precisionCut;
        private readonly InputAction recombine;
        private readonly InputAction rotateLeft;
        private readonly InputAction rotateRight;
        private readonly InputAction mute;

        public CompoundBoxInput()
        {
            gameplay = new InputActionMap("Gameplay");
            moveUp = AddButton(
                "Move Up",
                "<Keyboard>/w",
                "<Keyboard>/upArrow",
                "<Gamepad>/leftStick/up",
                "<Gamepad>/dpad/up");
            moveRight = AddButton(
                "Move Right",
                "<Keyboard>/d",
                "<Keyboard>/rightArrow",
                "<Gamepad>/leftStick/right",
                "<Gamepad>/dpad/right");
            moveDown = AddButton(
                "Move Down",
                "<Keyboard>/s",
                "<Keyboard>/downArrow",
                "<Gamepad>/leftStick/down",
                "<Gamepad>/dpad/down");
            moveLeft = AddButton(
                "Move Left",
                "<Keyboard>/a",
                "<Keyboard>/leftArrow",
                "<Gamepad>/leftStick/left",
                "<Gamepad>/dpad/left");
            confirm = AddButton(
                "Confirm",
                "<Keyboard>/enter",
                "<Keyboard>/space",
                "<Gamepad>/buttonSouth");
            advance = AddButton(
                "Advance",
                "<Keyboard>/n",
                "<Keyboard>/tab",
                "<Gamepad>/start");
            levelSelect = AddButton(
                "Level Select",
                "<Keyboard>/l",
                "<Gamepad>/select");
            previousLevel = AddButton("Previous Level", "<Keyboard>/leftBracket");
            nextLevel = AddButton("Next Level", "<Keyboard>/rightBracket");
            restart = AddButton("Restart", "<Keyboard>/r");
            undo = AddButton("Undo", "<Keyboard>/z");
            redo = AddButton("Redo", "<Keyboard>/y");
            split = AddButton("Split", "<Keyboard>/x");
            precisionCut = AddButton("Precision Cut", "<Keyboard>/v");
            recombine = AddButton("Recombine", "<Keyboard>/c");
            rotateLeft = AddButton("Rotate Left", "<Keyboard>/q");
            rotateRight = AddButton("Rotate Right", "<Keyboard>/e");
            mute = AddButton("Mute", "<Keyboard>/m");
            gameplay.Enable();
        }

        public bool ControlHeld =>
            UnityEngine.InputSystem.Keyboard.current != null &&
            (UnityEngine.InputSystem.Keyboard.current.leftCtrlKey.isPressed ||
             UnityEngine.InputSystem.Keyboard.current.rightCtrlKey.isPressed);

        public bool ShiftHeld =>
            UnityEngine.InputSystem.Keyboard.current != null &&
            (UnityEngine.InputSystem.Keyboard.current.leftShiftKey.isPressed ||
             UnityEngine.InputSystem.Keyboard.current.rightShiftKey.isPressed);

        public bool MutePressed => mute.WasPressedThisFrame();
        public bool ConfirmPressed => confirm.WasPressedThisFrame();
        public bool AdvancePressed => advance.WasPressedThisFrame();
        public bool LevelSelectPressed => levelSelect.WasPressedThisFrame();
        public bool PreviousLevelPressed => previousLevel.WasPressedThisFrame();
        public bool NextLevelPressed => nextLevel.WasPressedThisFrame();
        public bool RestartPressed => restart.WasPressedThisFrame();
        public bool UndoPressed => undo.WasPressedThisFrame();
        public bool RedoPressed => redo.WasPressedThisFrame();
        public bool SplitPressed => split.WasPressedThisFrame();
        public bool PrecisionCutPressed => precisionCut.WasPressedThisFrame();
        public bool RecombinePressed => recombine.WasPressedThisFrame();
        public bool RotateLeftPressed => rotateLeft.WasPressedThisFrame();
        public bool RotateRightPressed => rotateRight.WasPressedThisFrame();

        public bool TryReadDirection(out GridDirection direction)
        {
            if (moveUp.WasPressedThisFrame())
            {
                direction = GridDirection.Up;
                return true;
            }

            if (moveRight.WasPressedThisFrame())
            {
                direction = GridDirection.Right;
                return true;
            }

            if (moveDown.WasPressedThisFrame())
            {
                direction = GridDirection.Down;
                return true;
            }

            if (moveLeft.WasPressedThisFrame())
            {
                direction = GridDirection.Left;
                return true;
            }

            direction = GridDirection.None;
            return false;
        }

        public bool TryReadHeldDirection(out GridDirection direction)
        {
            if (moveUp.IsPressed())
            {
                direction = GridDirection.Up;
                return true;
            }

            if (moveRight.IsPressed())
            {
                direction = GridDirection.Right;
                return true;
            }

            if (moveDown.IsPressed())
            {
                direction = GridDirection.Down;
                return true;
            }

            if (moveLeft.IsPressed())
            {
                direction = GridDirection.Left;
                return true;
            }

            direction = GridDirection.None;
            return false;
        }

        public void Dispose()
        {
            gameplay.Disable();
            gameplay.Dispose();
        }

        private InputAction AddButton(string name, params string[] bindings)
        {
            var action = gameplay.AddAction(name, InputActionType.Button);
            for (var i = 0; i < bindings.Length; i++)
            {
                action.AddBinding(bindings[i]);
            }

            return action;
        }
    }
}
