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

        List<Selectable> _activeDominoes = new List<Selectable>();
        [SerializeField] Selectable _dominoPrefab;
        string _currentSave = "";

        bool _freezeRotation = false;

        void Awake()
        {
            if (!_dominoPrefab)
                Debug.LogError("Domino prefab is not initialized.");

            SaveData();
        }
        
        public void SpawnDomino()
        {
            SpawnDomino(Vector3.zero + Vector3.up * 2, Quaternion.identity);
        }

        public void SpawnDomino(Vector3 pos, Quaternion rot)
        {
            Selectable domino = _dominoPrefab.Get<Selectable>(null, pos, rot);
            domino.transform.position = pos;
            domino.SetVelocity(Vector3.zero); // Need to reset velocity because we're using object pooling.
            domino.SetAngularVelocity(Vector3.zero); // Need to reset velocity because we're using object pooling.
            if (_freezeRotation)
                domino.FreezeRotation();
            else
                domino.UnfreezeRotation();
            _activeDominoes.Add(domino);
        }

        public void DespawnDomino(Selectable domino)
        {
            _activeDominoes.Remove(domino);
            domino.Delete();
        }

        public void DespawnAllDominoes()
        {
            // If SelectController calls this function in any way, it must clear
            // its own "selected objects" array as well.
            // Or we could invoke an event to tell SelectController to clear its
            // selected-objects array.
            foreach (Selectable activeDomino in _activeDominoes)
            {
                activeDomino.Delete();
            }

            _activeDominoes.RemoveAll(activeDomino => true);
        }

        public void SaveData()
        {
            List<Transform> activeTransforms = new List<Transform>();
            foreach (Selectable activeDomino in _activeDominoes)
            {
                activeTransforms.Add(activeDomino.transform);
            }

            SerializableTransforms serializedActiveTransforms = new SerializableTransforms(activeTransforms.ToArray());
            _currentSave = JsonUtility.ToJson(serializedActiveTransforms);

            // Write the JSON to a file
        }

        public void LoadData()
        {
            // Load JSON from a file
        }
        
        public void ResetDominoes()
        {
            DespawnAllDominoes();

            SerializableTransforms serializedActiveTransforms = JsonUtility.FromJson<SerializableTransforms>(_currentSave);
            foreach (SerializableTransform st in serializedActiveTransforms._serializableTransforms)
            {
                SpawnDomino(st._position, st._rotation);
            }
        }

        public void FreezeRotation()
        {
            if (_freezeRotation)
                return;

            _freezeRotation = true;

            foreach (Selectable activeDomino in _activeDominoes)
            {
                activeDomino.FreezeRotation();
            }
        }

        public void UnfreezeRotation()
        {
            if (!_freezeRotation)
                return;

            _freezeRotation = false;

            foreach (Selectable activeDomino in _activeDominoes)
            {
                activeDomino.UnfreezeRotation();
            }
        }
    }
}
