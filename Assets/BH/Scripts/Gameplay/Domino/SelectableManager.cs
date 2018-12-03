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
        bool _randomColors = false;

        void Awake()
        {
            if (!_selectablePrefab)
                Debug.LogError("Selectable prefab is not initialized.");

            LoadData(); // Should show a loading screen to prevent player interaction until this completes (not implemented).
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
        /// <returns>
        ///   The spawned <c>Selectable</c>.
        /// </returns>
        public Selectable SpawnSelectable()
        {
            return SpawnSelectable(Vector3.zero + Vector3.up * 2, Quaternion.identity);
        }

        /// <summary>
        /// Spawns an instance of the selectable prefab with specified position and default rotation.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <returns>
        ///   The spawned <c>Selectable</c>.
        /// </returns>
        public Selectable SpawnSelectable(Vector3 pos)
        {
            return SpawnSelectable(pos, Quaternion.identity);
        }

        /// <summary>
        /// Spawns an instance of the selectable prefab with specified position and rotation.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <param name="rot">The rotation.</param>
        /// <returns>
        ///   The spawned <c>Selectable</c>.
        /// </returns>
        public Selectable SpawnSelectable(Vector3 pos, Quaternion rot)
        {
            Selectable sel = _selectablePrefab.Get<Selectable>(null, pos, rot);
            //sel.transform.position = pos;
            //sel.transform.rotation = rot;
            sel.SetVelocity(Vector3.zero); // Need to reset velocity because we're using object pooling.
            sel.SetAngularVelocity(Vector3.zero); // Need to reset velocity because we're using object pooling.

            if (_randomColors)
                sel.SetColor(new Color(Random.value, Random.value, Random.value));

            if (_freezeRotation)
                sel.FreezeRotation();
            else
                sel.UnfreezeRotation();

            if (_freezePosition)
                sel.FreezePosition();
            else
                sel.UnfreezePosition();

            _activeSelectables.Add(sel);
            return sel;
        }

        /// <summary>
        /// Spawns a new Selectable with the same transform and color as the given GameObj.
        /// </summary>
        /// <param name="orig">The Selectable to copy.</param>
        public Selectable SpawnSelectableFromGameObj(GameObj orig)
        {
            Selectable newSel = SpawnSelectable(orig.transform.position, orig.transform.rotation);
            newSel.SetColor(orig.GetColor());
            return newSel;
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
            DataManager.Instance.SaveData(_localData, (err) => {
                if (err != DataManagerStatusCodes.SUCCESS)
                    Debug.LogError("Couldn't save data!");
                else
                    Debug.Log("Saved data! N I C E");
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
            // Should only be called once at the beginning of the gameplay scene.
            // Should show a loading screen to prevent player interaction until this function completes (not implemented).
            // Definitely will create inconsistencies if this is called after some dominoes have been placed.

            DataManager.Instance.GetData((data, err) => {
                if (err == DataManagerStatusCodes.SUCCESS)
                {
                    Debug.Log("Retrieved data! N I C E");
                    List<SerializableSelectable> serializableSelectables = data._serializableSelectables._serializableSelectables;
                    foreach (SerializableSelectable serializableSelectable in serializableSelectables)
                    {
                        Selectable sel = SpawnSelectable();
                        sel.SetTransform(serializableSelectable._serializableTransform);
                        sel.ResetVelocities();
                        sel.SetColor(serializableSelectable._color);
                    }

                    _localData = data;
                }
                else
                {
                    Debug.LogError("Couldn't get data! noooo");
                    _localData = new Data();
                }
            });
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

        public void SetRandomColors(bool b)
        {
            _randomColors = b;
        }
    }
}
