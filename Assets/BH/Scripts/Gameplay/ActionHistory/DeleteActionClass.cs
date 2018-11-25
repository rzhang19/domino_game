using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH 
{   
    /// <summary>
    /// Represents the action of deleting one or more targets. Used to save and restore dominos.
    /// </summary>
    /// <seealso cref="BH.ActionClass" />
    public class DeleteActionClass : ActionInterface
    {
        Dictionary<int, SelectableFields> savedSelectables = new Dictionary<int, SelectableFields>();
        
        //maps between old instance ID & new Selectable instance spawned by calling Undo(). Needed in ActionHistory class
        Dictionary<int, Selectable> updatedSelectableInstances = new Dictionary<int, Selectable>();

        /// <summary>
        /// Saves a 1-to-1 mapping between targets and their to-be-saved colors and transforms.
        /// Will be switched to a constructor later. (Had some confusion because many Unity classes can't have constructors.)
        /// </summary>
        /// <param name="targets">List of Selectables representing the deleted targets.</param>
        public void Init(List<Selectable> targets)
        {
            foreach (Selectable sel in targets)
            {
                savedSelectables[sel.GetInstanceID()] = new SelectableFields(
                    sel.GetColor(), 
                    new CustomTransform(sel.transform)
                );
            }
        }

        /// <summary>
        /// Respawns dominos with their original colors and transforms.
        /// </summary>
        public void Undo() 
        {
            foreach (KeyValuePair<int, SelectableFields> update in savedSelectables)
            {
                int oldID = update.Key;
                SelectableFields selFields = update.Value;
                
                Color newColor = selFields.color;
                CustomTransform newTransform = selFields.transform;
                Selectable newSel = SelectableManager.Instance.SpawnSelectable(newTransform.position, newTransform.rotation);
                newSel.SetColor(newColor);

                updatedSelectableInstances[oldID] = newSel;
            }

            savedSelectables.Clear();
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

        /// <summary>
        /// Updates the requested Selectable's instance if relevant to this action. Useful if instance changes.
        /// <param name='oldID'>ID of the old Selectable instance, as returned by Unity's Object.GetInstanceID(). </param>
        /// <param name='newInstance'>New Selectable instance to update to. </param>
        /// </summary>
        public void UpdateInstance(int oldID, Selectable newInstance)
        {
            if (!savedSelectables.ContainsKey(oldID)) return;
            SelectableFields savedFields = savedSelectables[oldID];
            savedSelectables.Remove(oldID);
            savedSelectables[newInstance.GetInstanceID()] = savedFields;
        }

        /// <summary>
        /// Getter for the mapping between old Selectable instance IDs and new Selectable objects. Should be used after undo() was called.
        /// </summary>
        /// <returns>
        ///     A <c>Dictionary&lt;int,Selectable&gt;</c> mapping old instance IDs of the deleted targets to the new Selectable instances of the targets. 
        ///     Empty if new Selectables weren't spawned, i.e. undo() hasn't been called yet.
        /// </returns>
        public Dictionary<int, Selectable> GetUpdatedInstances() 
        {
            return updatedSelectableInstances;
        } 

        // Extrapolated all fields of Selectables that should be saved. Maps these fields to the Selectable instance.
        struct SelectableFields 
        {
            public Color color;
            public CustomTransform transform;

            public SelectableFields(Color c, CustomTransform t)
            {
                color = c;
                transform = t;
            }
        }
    }
}