using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BH.DesignPatterns;
using System.IO;

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

        Data _localData;

        bool _freezeRotation = false;
        bool _freezePosition = false;

        void Awake()
        {
            if (!_selectablePrefab)
                Debug.LogError("Selectable prefab is not initialized.");

            _localData = new Data();
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
        /// Spawns an instance of the selectable prefab with specified position and default rotation.
        /// </summary>
        /// <param name="pos">The position.</param>
        public void SpawnSelectable(Vector3 pos)
        {
            SpawnSelectable(pos, Quaternion.identity);
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

            if (_freezePosition)
                sel.FreezePosition();
            else
                sel.UnfreezePosition();

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
        /// Saves the data of spawned selectables in persistent memory (as well as locally).
        /// </summary>
        public void SaveDataPersistent()
        {
            SaveDataLocal();

            // Example call to DataManager
            DataManager.Instance.SaveData("dinorider23", "hunter2", _localData, (err) => {
                if (err != DataManagerStatusCodes.SUCCESS)
                    Debug.LogError("Couldn't save data!");
            });
        }

        /// <summary>
        /// Saves the data of spawned selectables locally.
        /// </summary>
        public void SaveDataLocal()
        {
            _localData = new Data(_activeSelectables.ToArray());
        }

        /// <summary>
        /// Loads the data of spawned selectables from persistent memory (not implemented).
        /// </summary>
        public void LoadData()
        {
            // Load JSON from a file
        }

        /// <summary>
        /// Resets scene with the local data by restoring all selectables using their saved representations.
        /// </summary>
        public void ResetData()
        {
            List<SerializableSelectable> serializableSelectables = _localData._serializableSelectables._serializableSelectables;
            
            if (_activeSelectables.Count != serializableSelectables.Count)
            {
                Debug.LogError("Error: # of active selectables != # of saved selectables. Should be the same, since spectator mode never changes # of selectables.");
                return;
            }
            else
            {
                for (int i = 0; i < _activeSelectables.Count; i++)
                {
                    // Transform
                    _activeSelectables[i].SetTransform(serializableSelectables[i]._serializableTransform);

                    // Physics
                    _activeSelectables[i].ResetVelocities();

                    // Color
                    _activeSelectables[i].SetColor(serializableSelectables[i]._color);
                }
            }
        }

        /// <summary>
        /// Freezes the rotation of every spawned selectable.
        /// </summary>
        public void FreezeRotation()
        {
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
            _freezeRotation = false;

            foreach (Selectable activeSelectable in _activeSelectables)
            {
                activeSelectable.UnfreezeRotation();
            }
        }

        /// <summary>
        /// Freezes the position of every spawned selectable.
        /// </summary>
        public void FreezePosition()
        {
            _freezePosition = true;

            foreach (Selectable activeSelectable in _activeSelectables)
            {
                activeSelectable.FreezePosition();
            }
        }

        /// <summary>
        /// Unfreezes the position of every spawned selectable.
        /// </summary>
        public void UnfreezePosition()
        {
            _freezePosition = false;

            foreach (Selectable activeSelectable in _activeSelectables)
            {
                activeSelectable.UnfreezePosition();
            }
        }

        /// <summary>
        /// Gets the selectable prefab.
        /// </summary>
        /// <returns>Returns the selectable prefab.</returns>
        public Selectable GetSelectablePrefab()
        {
            return _selectablePrefab;
        }
    }
}
