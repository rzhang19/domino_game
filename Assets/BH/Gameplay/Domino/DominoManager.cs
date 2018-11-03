using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BH.DesignPatterns;

namespace BH
{
    public class DominoManager : Singleton<DominoManager>
    {
        protected DominoManager() { }

        List<Domino> _activeDominos = new List<Domino>();
        [SerializeField] Domino _dominoPrefab;

        void Awake()
        {
            if (!_dominoPrefab)
                Debug.LogError("Domino prefab is not initialized.");
        }

        public void SpawnDomino()
        {
            SpawnDomino(Vector3.zero + Vector3.up, Quaternion.identity);
        }

        public void SpawnDomino(Vector3 pos, Quaternion rot)
        {
            Domino domino = _dominoPrefab.Get<Domino>(null, pos, Quaternion.identity);
            domino.transform.position = pos;
            _activeDominos.Add(domino);
        }

        public void DespawnDomino(Domino domino)
        {
            _activeDominos.Remove(domino);
            domino.enabled = false;
        }
    }
}
