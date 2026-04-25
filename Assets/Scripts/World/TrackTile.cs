using System;
using System.Collections.Generic;
using UnityEngine;

namespace InfinityRunner.World
{
    public class TrackTile : MonoBehaviour
    {
        [Header("Connection Points")]
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;

        [Header("Socket Roots")]
        [SerializeField] private Transform obstacleSocketRoot;
        [SerializeField] private Transform collectibleSocketRoot;

        [SerializeField, HideInInspector] private Transform[] obstacleSockets = Array.Empty<Transform>();
        [SerializeField, HideInInspector] private Transform[] collectibleSockets = Array.Empty<Transform>();

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

        private void Awake()
        {
            RefreshSockets();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            RefreshSockets();
        }
#endif

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
                    $"[{name}] Tile inválido. " +
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
                Debug.LogError($"[{name}] Tile inválido. Verifique StartPoint e EndPoint.");
                return;
            }

            Quaternion desiredRootRotation = attachPoint.rotation * Quaternion.Inverse(startPoint.localRotation);
            transform.rotation = desiredRootRotation;
            transform.position = attachPoint.position - (desiredRootRotation * startPoint.localPosition);
        }

        private void RefreshSockets()
        {
            obstacleSockets = GetDirectChildren(obstacleSocketRoot);
            collectibleSockets = GetDirectChildren(collectibleSocketRoot);
        }

        private Transform[] GetDirectChildren(Transform root)
        {
            if (root == null)
                return Array.Empty<Transform>();

            int count = root.childCount;
            Transform[] result = new Transform[count];

            for (int i = 0; i < count; i++)
                result[i] = root.GetChild(i);

            return result;
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

            if (obstacleSockets != null)
            {
                Gizmos.color = Color.red;
                foreach (var socket in obstacleSockets)
                {
                    if (socket != null)
                        Gizmos.DrawCube(socket.position, Vector3.one * 0.2f);
                }
            }

            if (collectibleSockets != null)
            {
                Gizmos.color = Color.yellow;
                foreach (var socket in collectibleSockets)
                {
                    if (socket != null)
                        Gizmos.DrawSphere(socket.position, 0.15f);
                }
            }
        }
#endif
    }
}