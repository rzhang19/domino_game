using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BH
{
    public class BackgroundDominoes : MonoBehaviour
    {
        List<BackgroundDomino> _backgroundDominoes;

        [SerializeField] float _toggleInterval = 0.5f;

        void Awake()
        {
            _backgroundDominoes = GetComponentsInChildren<BackgroundDomino>().ToList();
        }

        void Start()
        {
            StartCoroutine(ToggleEveryInterval(_toggleInterval));
        }

        IEnumerator ToggleEveryInterval(float interval)
        {
            while (true)
            {
                int index = Random.Range(0, _backgroundDominoes.Count);
                _backgroundDominoes[index].ToggleLowHigh();

                yield return new WaitForSeconds(interval);
            }
        }
    }
}
