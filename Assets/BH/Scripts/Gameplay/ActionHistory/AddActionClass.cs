using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH 
{   
    /// <summary>
    /// Represents the action of creating target(s). Used to undo domino creation.
    /// </summary>
    /// <seealso cref="BH.ActionClass" />
    public class AddActionClass : ActionInterface
    {
        Dictionary<int, Selectable> savedSelectables = new Dictionary<int, Selectable>();

        /// <summary>
        /// Saves the created targets.
        /// Will be switched to a constructor later. (Had some confusion because many Unity classes can't have constructors.)
        /// </summary>
        /// <param name="targets">List of Selectable targets that were created.</param>
        public void Init(List<Selectable> targets) 
        {
            foreach (Selectable t in targets)
            {
                savedSelectables[t.GetInstanceID()] = t;
            }
        }

        /// <summary>
        /// Deletes the saved target.
        /// </summary>
        public void Undo() 
        {
            foreach (Selectable sel in savedSelectables.Values) 
            {
                SelectableManager.Instance.DespawnSelectable(sel);
            }

            savedSelectables.Clear();
        }

        /// <summary>
        /// Updates the requested Selectable's instance if relevant to this action. Useful if instance changes.
        /// <param name='oldID'>ID of the old Selectable instance, as returned by Unity's Object.GetInstanceID(). </param>
        /// <param name='newInstance'>New Selectable instance to update to. </param>
        /// </summary>
        public void UpdateInstance(int oldID, Selectable newInstance)
        {
            if (!savedSelectables.ContainsKey(oldID)) return;
            savedSelectables.Remove(oldID);
            savedSelectables[newInstance.GetInstanceID()] = newInstance;
        }

        /// <summary>
        /// Getter for the list containing the instance IDs of the Selectables targeted by this action.
        /// </summary>
        /// <returns>
        ///     A <c>List&lt;int&gt;</c> of the targets' instance IDs.
        /// </returns>
        public List<int> GetTargetIDs() 
        {
            return new List<int>(savedSelectables.Keys);
        }     
    }
}