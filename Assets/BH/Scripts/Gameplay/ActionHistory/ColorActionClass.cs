using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH 
{   
    /// <summary>
    /// Represents the action of coloring one or more targets. Used to save and revert to a specific domino coloring.
    /// </summary>
    /// <seealso cref="BH.ActionClass" />
    public class ColorActionClass : ActionInterface
    {
        Dictionary<int, SelectableToColor> oldSelectableStates = new Dictionary<int, SelectableToColor>();

        /// <summary>
        /// Saves a 1-to-1 mapping between targets and their to-be-saved colors.
        /// Will be switched to a constructor later. (Had some confusion because many Unity classes can't have constructors.)
        /// </summary>
        /// <param name="targets">List of Selectable targets.</param>
        /// <param name="oldProperties">List of Colors to save.</param>
        public void Init(List<Selectable> targets, List<Color> oldProperties) {
            for (int i = 0; i < targets.Count; i++)
            {
                oldSelectableStates[targets[i].GetInstanceID()] = new SelectableToColor(targets[i], oldProperties[i]);
            }
        }

        /// <summary>
        /// Restores each target to their mapped color. Cannot reverse once executed.
        /// </summary>
        public void Undo() 
        {
            foreach(SelectableToColor state in this.oldSelectableStates.Values)
            {
                Selectable target = state.sel;
                Color oldColor = state.color;
                target.SetColor(oldColor);
            }

            oldSelectableStates.Clear();
        }
        
        /// <summary>
        /// Getter for the list containing the instance IDs of the Selectables targeted by this action.
        /// </summary>
        /// <returns>
        ///     A <c>List&lt;int&gt;</c> of the targets' instance IDs.
        /// </returns>
        public List<int> GetTargetIDs() 
        {
            return new List<int>(oldSelectableStates.Keys);
        }

        /// <summary>
        /// Updates the requested Selectable's instance if relevant to this action. Useful if instance changes.
        /// <param name='oldID'>ID of the old Selectable instance, as returned by Unity's Object.GetInstanceID(). </param>
        /// <param name='newInstance'>New Selectable instance to update to. </param>
        /// </summary>
        public void UpdateInstance(int oldID, Selectable newInstance)
        {
            if (!oldSelectableStates.ContainsKey(oldID)) return;
            SelectableToColor savedColorMapping = oldSelectableStates[oldID];
            savedColorMapping.sel = newInstance;
            oldSelectableStates.Remove(oldID);
            oldSelectableStates[newInstance.GetInstanceID()] = savedColorMapping;
        }

        // Links a Selectable to its saved color.
        struct SelectableToColor
        {
            public Selectable sel;
            public Color color;

            public SelectableToColor(Selectable s, Color c)
            {
                sel = s;
                color = c;
            }
        }
    }
}