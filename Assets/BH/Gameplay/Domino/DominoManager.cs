using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BH.DesignPatterns;
using System.IO;

namespace BH
{
    public class DominoManager : Singleton<DominoManager>
    {
        protected DominoManager() { }

        List<Selectable> _activeDominos = new List<Selectable>();
        [SerializeField] Selectable _dominoPrefab;
        string myData = "";

        void Awake()
        {
            if (!_dominoPrefab)
                Debug.LogError("Domino prefab is not initialized.");

            SaveData();
        }
        
        public void SpawnDomino()
        {
            SpawnDomino(Vector3.zero + Vector3.up, Quaternion.identity);
        }

        public void SpawnDomino(Vector3 pos, Quaternion rot)
        {
            Selectable domino = _dominoPrefab.Get<Selectable>(null, pos, rot);
            domino.transform.position = pos;
            _activeDominos.Add(domino);
        }

        public void DespawnDomino(Selectable domino)
        {
            _activeDominos.Remove(domino);
            domino.Delete();
            domino.enabled = false;
        }

        public void SaveData()
        {
            List<Transform> activeTransforms = new List<Transform>();
            foreach (Selectable activeDomino in _activeDominos)
            {
                activeTransforms.Add(activeDomino.transform);
            }

            SerializableTransforms serializedActiveTransforms = new SerializableTransforms(activeTransforms.ToArray());
            myData = JsonUtility.ToJson(serializedActiveTransforms);
        }

        public void LoadData()
        {

        }

        public void SpawnLoadedData()
        {
            SerializableTransforms serializedActiveTransforms = JsonUtility.FromJson<SerializableTransforms>(myData);
            foreach (SerializableTransform st in serializedActiveTransforms._serializableTransforms)
            {
                SpawnDomino(st._position, st._rotation);
            }
        }
    }
}
