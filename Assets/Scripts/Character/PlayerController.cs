using UnityEngine;
using InfinityRunner.Core;

namespace InfinityRunner.Character
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerMovement movement;

        [Header("Keyboard Input")]
        [SerializeField] private KeyCode moveLeftKey = KeyCode.A;
        [SerializeField] private KeyCode moveRightKey = KeyCode.D;
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode slideKey = KeyCode.S;

        [Header("Arrow Keys Alternative")]
        [SerializeField] private bool allowArrowKeys = true;

        private void Reset()
        {
            movement = GetComponent<PlayerMovement>();
        }

        private void Awake()
        {
            if (movement == null)
                movement = GetComponent<PlayerMovement>();
        }

        private void Update()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
                return;

            ReadInput();
        }

        private void ReadInput()
        {
            bool leftPressed = Input.GetKeyDown(moveLeftKey) || (allowArrowKeys && Input.GetKeyDown(KeyCode.LeftArrow));
            bool rightPressed = Input.GetKeyDown(moveRightKey) || (allowArrowKeys && Input.GetKeyDown(KeyCode.RightArrow));
            bool jumpPressed = Input.GetKeyDown(jumpKey) || (allowArrowKeys && Input.GetKeyDown(KeyCode.UpArrow));
            bool slidePressed = Input.GetKeyDown(slideKey) || (allowArrowKeys && Input.GetKeyDown(KeyCode.DownArrow));

            if (leftPressed)
                movement.MoveLeft();

            if (rightPressed)
                movement.MoveRight();

            if (jumpPressed)
                movement.Jump();

            if (slidePressed)
                movement.Slide();
        }
    }
}