using UnityEngine;

namespace InfinityRunner.World
{
    public class SceneryChunk : MonoBehaviour
    {
        [Header("Connection Points")]
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;

        public Transform StartPoint => startPoint;
        public Transform EndPoint => endPoint;

        public float LocalLengthZ
        {
            get
            {
                if (startPoint == null || endPoint == null)
                    return 0f;

                return endPoint.localPosition.z - startPoint.localPosition.z;
            }
        }

        public bool IsValid()
        {
            if (startPoint == null)
            {
                Debug.LogError($"[{name}] StartPoint não foi atribuído.");
                return false;
            }

            if (endPoint == null)
            {
                Debug.LogError($"[{name}] EndPoint não foi atribuído.");
                return false;
            }

            if (LocalLengthZ <= 0.01f)
            {
                Debug.LogError(
                    $"[{name}] Chunk inválido. " +
                    $"StartPoint localPosition: {startPoint.localPosition} | " +
                    $"EndPoint localPosition: {endPoint.localPosition} | " +
                    $"LocalLengthZ: {LocalLengthZ}"
                );
                return false;
            }

            return true;
        }

        public void AlignTo(Transform attachPoint)
        {
            if (attachPoint == null)
            {
                Debug.LogError($"[{name}] AttachPoint é nulo.");
                return;
            }

            if (!IsValid())
            {
                Debug.LogError($"[{name}] Chunk inválido. Verifique StartPoint e EndPoint.");
                return;
            }

            Quaternion desiredRootRotation = attachPoint.rotation * Quaternion.Inverse(startPoint.localRotation);
            transform.rotation = desiredRootRotation;
            transform.position = attachPoint.position - (desiredRootRotation * startPoint.localPosition);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (startPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(startPoint.position, 0.25f);
            }

            if (endPoint != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(endPoint.position, 0.25f);
            }
        }
#endif
    }
}