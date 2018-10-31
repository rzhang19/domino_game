using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BH
{
    public class ClosestColliderBelow : MonoBehaviour
    {
        MeshFilter _meshFilter;
        MeshCollider _meshCollider;
        float _colliderHeight = 10f;
        public Transform _closestTransform;

        [SerializeField] LayerMask _mask;
        [SerializeField] float _xzScale = 1.1f;

        void Awake()
        {
            _meshFilter = GetComponentInChildren<MeshFilter>();
        }

        void Update()
        {
            if (!_meshCollider)
                return;

            List<Vector3> localVertices = _meshFilter.sharedMesh.vertices.ToList();
            List<Vector3> modifiedVertices = new List<Vector3>();
            foreach (Vector3 vec in localVertices)
            {
                Vector3 modifiedVec = transform.TransformPoint(vec);
                modifiedVec = new Vector3(modifiedVec.x, transform.position.y, modifiedVec.z);
                modifiedVec = transform.InverseTransformPoint(modifiedVec);
                modifiedVec = new Vector3(modifiedVec.x * _xzScale, modifiedVec.y, modifiedVec.z * _xzScale);
                modifiedVertices.Add(modifiedVec);
            }
            foreach (Vector3 vec in localVertices)
            {
                Vector3 modifiedVec = transform.TransformPoint(vec);
                modifiedVec = new Vector3(modifiedVec.x, transform.position.y - _colliderHeight, modifiedVec.z);
                modifiedVec = transform.InverseTransformPoint(modifiedVec);
                modifiedVec = new Vector3(modifiedVec.x * _xzScale, modifiedVec.y, modifiedVec.z * _xzScale);
                modifiedVertices.Add(modifiedVec);
            }

            List<int> localTriangles = _meshFilter.sharedMesh.triangles.ToList();
            List<int> modifiedTriangles = localTriangles.ToList();
            foreach (int i in localTriangles)
            {
                modifiedTriangles.Add(i + localVertices.Count);
            }

            Mesh colliderMesh = new Mesh();
            colliderMesh.Clear();
            colliderMesh.vertices = modifiedVertices.ToArray();
            colliderMesh.triangles = modifiedTriangles.ToArray();
            _meshCollider.sharedMesh = colliderMesh;
        }

        // OnTriggerStay() (supposedly) is called after FixedUpdate(), so we use FixedUpdate() to reset the distance.
        void FixedUpdate()
        {
            _closestTransform = null;
        }

        // Obtain the closest distance to a collider below the object.
        void OnTriggerStay(Collider col)
        {
            if (_mask == (_mask | (1 << col.gameObject.layer)) && col.transform.root != transform.root)
            {
                float distance = transform.position.y - col.transform.position.y;
                if (!_closestTransform || (distance < transform.position.y - _closestTransform.position.y && distance > 0f))
                    _closestTransform = col.transform;
            }
        }

        void OnDisable()
        {
            Destroy(_meshCollider);
            _meshCollider = null;
            _closestTransform = null;
        }

        void OnEnable()
        {
            _meshCollider = gameObject.AddComponent<MeshCollider>();
            _meshCollider.convex = true;
            _meshCollider.isTrigger = true;
            _closestTransform = null;
        }
    }
}
