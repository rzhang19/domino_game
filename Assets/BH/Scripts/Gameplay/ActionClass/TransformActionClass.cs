using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH 
{
    /// <summary>
    /// Represents the action of transforming one or more targets. Used to save and revert to specific domino transforms.
    /// Transforms must be of our custom type CustomTransform because Unity restricts Transform instantiation.
    /// </summary>
    /// <seealso cref="BH.ActionClass" />
    public class TransformActionClass : ActionClass
    {
        Dictionary<int, SelectableToTransform> oldSelectableStates = new Dictionary<int, SelectableToTransform>();

        /// <summary>
        /// Saves a 1-to-1 mapping between targets and their to-be-saved transforms.
        /// Will be switched to a constructor later. (Had some confusion because many Unity classes can't have constructors.)
        /// </summary>
        /// <param name="targets">List of Selectable targets.</param>
        /// <param name="oldProperties">List of CustomTransforms to save.</param>
        public void Init(List<Selectable> targets, List<CustomTransform> oldProperties) {
            for (int i = 0; i < targets.Count; i++)
            {
                oldSelectableStates[targets[i].GetInstanceID()] = new SelectableToTransform(targets[i], oldProperties[i]);
            }
        }

        /// <summary>
        /// Restores each target to their mapped transform. Cannot reverse once executed.
        /// </summary>
        public override void Undo() 
        {
            foreach(SelectableToTransform state in this.oldSelectableStates.Values)
            {
                Selectable target = state.sel;
                CustomTransform oldTransform = state.transform;
                target.transform.position = oldTransform.position;
                target.transform.rotation = oldTransform.rotation;
                target.transform.localScale = oldTransform.localScale;
                target.ResetVelocities();
            }

            oldSelectableStates.Clear();
        }

        /// <summary>
        /// Getter for the list containing the instance IDs of the Selectables targeted by this action.
        /// </summary>
        /// <returns>
        ///     A <c>List&lt;int&gt;</c> of the targets' instance IDs.
        /// </returns>
        public override List<int> GetTargetIDs() 
        {
            return new List<int>(oldSelectableStates.Keys);
        }

        /// <summary>
        /// Updates the requested Selectable's instance if relevant to this action. Useful if instance changes.
        /// <param name='oldID'>ID of the old Selectable instance, as returned by Unity's Object.GetInstanceID(). </param>
        /// <param name='newInstance'>New Selectable instance to update to. </param>
        /// </summary>
        public override void UpdateInstance(int oldID, Selectable newInstance)
        {
            if (!oldSelectableStates.ContainsKey(oldID)) return;
            SelectableToTransform savedTransformMapping = oldSelectableStates[oldID];
            savedTransformMapping.sel = newInstance;
            oldSelectableStates.Remove(oldID);
            oldSelectableStates[newInstance.GetInstanceID()] = savedTransformMapping;
        }

        // Links a Selectable to its saved transform.
        struct SelectableToTransform
        {
            public Selectable sel;
            public CustomTransform transform;

            public SelectableToTransform(Selectable s, CustomTransform t)
            {
                sel = s;
                transform = t;
            }
        }
    }
}