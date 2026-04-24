using System.Collections.Generic;
using UnityEngine;

namespace InfinityRunner.World
{
    public class TrackTile : MonoBehaviour
    {
        [Header("Connection Points")]
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;

        [Header("Spawn Sockets")]
        [SerializeField] private Transform[] obstacleSockets;
        [SerializeField] private Transform[] collectibleSockets;

        public Transform StartPoint => startPoint;
        public Transform EndPoint => endPoint;
        public IReadOnlyList<Transform> ObstacleSockets => obstacleSockets;
        public IReadOnlyList<Transform> CollectibleSockets => collectibleSockets;

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
            if (startPoint == null || endPoint == null)
                return false;

            return LocalLengthZ > 0.01f;
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
                Debug.LogError($"[{name}] Tile inválido. Verifique StartPoint e EndPoint.");
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
                Gizmos.DrawSphere(startPoint.position, 0.2f);
            }

            if (endPoint != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(endPoint.position, 0.2f);
            }
        }
#endif
    }
}