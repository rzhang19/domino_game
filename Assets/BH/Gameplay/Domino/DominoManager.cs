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
        }

        public void DespawnAllDominos()
        {
            // If SelectController calls this function in any way, it must clear
            // its own "selected objects" array as well.
            // Or we could invoke an event to tell SelectController to clear its
            // selected-objects array.
            foreach (Selectable activeDomino in _activeDominos)
            {
                activeDomino.Delete();
            }

            _activeDominos.RemoveAll(activeDomino => true);
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

            // Write the JSON to a file
        }

        public void LoadData()
        {
            // Load JSON from a file
        }
        
        public void ResetDominos()
        {
            DespawnAllDominos();

            SerializableTransforms serializedActiveTransforms = JsonUtility.FromJson<SerializableTransforms>(myData);
            foreach (SerializableTransform st in serializedActiveTransforms._serializableTransforms)
            {
                SpawnDomino(st._position, st._rotation);
            }
        }
    }
}
