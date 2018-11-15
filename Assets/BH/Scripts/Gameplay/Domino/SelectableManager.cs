using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BH.DesignPatterns;
using System.IO; // May need this to write data out to a file.

namespace BH
{
    /// <summary>
    /// Manages "Selectable" objects--any game object that can be spawned/despawned by the player.
    /// A client can spawn and despawn game objects through SelectableManager calls.
    /// </summary>
    /// <seealso cref="BH.DesignPatterns.Singleton{BH.SelectableManager}" />
    /// <seealso cref="BH.Selectable" />
    public class SelectableManager : Singleton<SelectableManager>
    {
        protected SelectableManager() { }

        List<Selectable> _activeSelectables = new List<Selectable>();
        [SerializeField] Selectable _selectablePrefab;
        // string _persistentSave = ""; // Eventually we will have to distinguish a persistent save from a local save.
        string _currentSave = "";

        bool _freezeRotation = false;

        void Awake()
        {
            if (!_selectablePrefab)
                Debug.LogError("Selectable prefab is not initialized.");

            SaveLayout();
        }

        /// <summary>
        /// Sets the selectable prefab.
        /// </summary>
        /// <param name="sel">The selectable prefab.</param>
        public void SetSelectablePrefab(Selectable sel)
        {
            _selectablePrefab = sel;
        }

        /// <summary>
        /// Getter for the list of currently active selectables.
        /// </summary>
        /// <returns>
        ///   A <c>List&lt;Selectable&gt;</c> of active selectables.
        /// </returns>
        public List<Selectable> GetActiveSelectables()
        {
            return _activeSelectables;
        }

        /// <summary>
        /// Spawns an instance of the selectable prefab with default position and rotation.
        /// </summary>
        public void SpawnSelectable()
        {
            SpawnSelectable(Vector3.zero + Vector3.up * 2, Quaternion.identity);
        }

        /// <summary>
        /// Spawns an instance of the selectable prefab with specified position and rotation.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <param name="rot">The rotation.</param>
        public void SpawnSelectable(Vector3 pos, Quaternion rot)
        {
            Selectable sel = _selectablePrefab.Get<Selectable>(null, pos, rot);
            sel.transform.position = pos;
            sel.SetVelocity(Vector3.zero); // Need to reset velocity because we're using object pooling.
            sel.SetAngularVelocity(Vector3.zero); // Need to reset velocity because we're using object pooling.
            if (_freezeRotation)
                sel.FreezeRotation();
            else
                sel.UnfreezeRotation();
            _activeSelectables.Add(sel);
        }

        /// <summary>
        /// Despawns the selectable boject.
        /// </summary>
        /// <param name="sel">The selectable object.</param>
        public void DespawnSelectable(Selectable sel)
        {
            _activeSelectables.Remove(sel);
            sel.Delete();
        }

        /// <summary>
        /// Despawns all selectable objects.
        /// </summary>
        public void DespawnAllSelectable()
        {
            // If SelectController calls this function in any way, it must clear
            // its own "selected objects" array as well.
            // Or we could invoke an event to tell SelectController to clear its
            // selected-objects array.
            foreach (Selectable activeSelectable in _activeSelectables)
            {
                activeSelectable.Delete();
            }

            _activeSelectables.RemoveAll(activeSelectable => true);
        }

        /// <summary>
        /// Saves the layout of spawned selectables locally.
        /// </summary>
        public void SaveLayout()
        {
            List<Transform> activeTransforms = new List<Transform>();
            foreach (Selectable activeSelectable in _activeSelectables)
            {
                activeTransforms.Add(activeSelectable.transform);
            }

            SerializableTransforms serializedActiveTransforms = new SerializableTransforms(activeTransforms.ToArray());
            _currentSave = JsonUtility.ToJson(serializedActiveTransforms);

            // Write the JSON to a file
        }

        /// <summary>
        /// Loads the layout (not implemented).
        /// </summary>
        public void LoadLayout()
        {
            // Load JSON from a file
        }

        /// <summary>
        /// Resets the layout to the currently loaded layout by restoring all selectables to their saved transforms.
        /// </summary>
        public void ResetLayout()
        {
            SerializableTransforms serializedActiveTransforms = JsonUtility.FromJson<SerializableTransforms>(_currentSave);
            if (_activeSelectables.Count != serializedActiveTransforms._serializableTransforms.Count)
            {
                Debug.LogError("Error: # of active selectables != # of saved transforms. Should be the same, since spectator mode never changes # of selectables.");
                return;
            }
            else
            {
                for (int i = 0; i < _activeSelectables.Count; i++)
                {
                    _activeSelectables[i].SetTransform(serializedActiveTransforms._serializableTransforms[i]);
                    _activeSelectables[i].ResetVelocities();
                }
            }
        }

        /// <summary>
        /// Freezes the rotation of every spawned selectable.
        /// </summary>
        public void FreezeRotation()
        {
            if (_freezeRotation)
                return;

            _freezeRotation = true;

            foreach (Selectable activeSelectable in _activeSelectables)
            {
                activeSelectable.FreezeRotation();
            }
        }

        /// <summary>
        /// Unfreezes the rotation of every spawned selectable.
        /// </summary>
        public void UnfreezeRotation()
        {
            if (!_freezeRotation)
                return;

            _freezeRotation = false;

            foreach (Selectable activeSelectable in _activeSelectables)
            {
                activeSelectable.UnfreezeRotation();
            }
        }
    }
}
