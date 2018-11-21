using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BH
{
    /// <summary>
    /// Detects the closest collider below a given mesh filter.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class DetectColliderBelow : MonoBehaviour
    {
        MeshFilter _meshFilter;
        MeshCollider _meshCollider;
        float _colliderHeight = 10f;
        /// <summary>
        /// The transform of the closest collider below.
        /// </summary>
        Transform _closestTransform;

        [SerializeField] LayerMask _colliderMask;
        [SerializeField] float _defaultXScale = 1.2f;
        [SerializeField] float _defaultZScale = 1.2f;
        float _xScale = float.MinValue;
        float _zScale = float.MinValue;

        void Awake()
        {
            enabled = false; // Not supposed to be active initially, unless intended by the programmer.
        }

        void Update()
        {
            RefreshCollider();
        }

        void RefreshCollider()
        {
            if (!_meshCollider || !_meshFilter || !_meshFilter.gameObject.activeInHierarchy)
                return;

            List<Vector3> localVertices = _meshFilter.sharedMesh.vertices.ToList();
            List<Vector3> modifiedVertices = new List<Vector3>();

            foreach (Vector3 vec in localVertices)
            {
                Vector3 modifiedVec = _meshFilter.transform.TransformPoint(new Vector3(vec.x * _xScale, vec.y, vec.z * _zScale));
                modifiedVec = new Vector3(modifiedVec.x, _meshFilter.transform.position.y, modifiedVec.z);
                //modifiedVec = _meshFilter.transform.InverseTransformPoint(modifiedVec);
                //modifiedVec = new Vector3(modifiedVec.x * _xScale, modifiedVec.y, modifiedVec.z * _zScale);
                modifiedVertices.Add(modifiedVec);
            }
            foreach (Vector3 vec in localVertices)
            {
                Vector3 modifiedVec = _meshFilter.transform.TransformPoint(new Vector3(vec.x * _xScale, vec.y, vec.z * _zScale));
                modifiedVec = new Vector3(modifiedVec.x, _meshFilter.transform.position.y - _colliderHeight, modifiedVec.z);
                //modifiedVec = _meshFilter.transform.InverseTransformPoint(modifiedVec);
                //modifiedVec = new Vector3(modifiedVec.x * _xScale, modifiedVec.y, modifiedVec.z * _zScale);
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

        // Obtain the closest transform (that has a collider) below the mesh filter.
        void OnTriggerStay(Collider col)
        {
            if (!_meshCollider || !_meshFilter || !_meshFilter.gameObject.activeInHierarchy)
                return;

            if (_colliderMask == (_colliderMask | (1 << col.gameObject.layer)) && col.transform.root != _meshFilter.transform.root)
            {
                float distance = _meshFilter.transform.position.y - col.transform.position.y;
                if (!_closestTransform || (distance < _meshFilter.transform.position.y - _closestTransform.position.y && distance > 0f))
                    _closestTransform = col.transform;
            }
        }

        void OnDisable()
        {
            Destroy(_meshCollider);
            _meshCollider = null;
            _closestTransform = null;
            _meshFilter = null;
            _xScale = _defaultXScale;
            _zScale = _defaultZScale;
        }

        void OnEnable()
        {
            _meshCollider = gameObject.AddComponent<MeshCollider>();
            _meshCollider.convex = true;
            _meshCollider.isTrigger = true;
            _closestTransform = null;
        }

        public Transform GetClosestTransform()
        {
            return _closestTransform;
        }

        public void SetActiveMeshFilter(MeshFilter meshFilter, float xScale = float.MinValue, float zScale = float.MinValue)
        {
            enabled = true;
            _meshFilter = meshFilter;

            if (xScale == float.MinValue)
                _xScale = _defaultXScale;
            else
                _xScale = xScale;
            
            if (zScale == float.MinValue)
                _zScale = _defaultZScale;
            else
                _zScale = zScale;
        }

        public void SetInactive()
        {
            enabled = false;
        }
    }
}
