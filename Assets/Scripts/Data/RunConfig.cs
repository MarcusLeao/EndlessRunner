using UnityEngine;

namespace InfinityRunner.Data
{
    [CreateAssetMenu(menuName = "Endeless Runner/Config/Run Config")]
    public class RunConfig : ScriptableObject
    {
        [Header("Forward Speed")]
        [Min(0f)] public float startForwardSpeed = 8f;
        [Min(0f)] public float maxForwardSpeed = 20f;
        [Min(0f)] public float forwardAcceleration = 0.15f;

        [Header("Lane Movement")]
        [Min(0f)] public float laneOffset = 2.5f;
        [Min(0f)] public float laneChangeSpeed = 12f;

        [Header("Jump")]
        [Min(0f)] public float jumpForce = 10f;
        [Min(0f)] public float gravity = 30f;

        [Header("Slide")]
        [Min(0.1f)] public float slideDuration = 0.8f;
        [Min(0.1f)] public float slideControllerHeight = 1.0f;

        [Header("Ground Stick")]
        [Min(0f)] public float groundedVerticalForce = 2f;
    }
}