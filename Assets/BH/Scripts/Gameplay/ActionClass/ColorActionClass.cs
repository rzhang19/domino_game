using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH 
{   
    /// <summary>
    /// Represents the action of coloring one or more targets. Used to save and revert to a specific domino coloring.
    /// </summary>
    /// <seealso cref="BH.ActionClass" />
    public class ColorActionClass : ActionClass
    {
        Dictionary<Selectable, Color> oldTargetStates = new Dictionary<Selectable, Color>();

        /// <summary>
        /// Saves a 1-to-1 mapping between targets and their to-be-saved colors.
        /// Will be switched to a constructor later. (Had some confusion because many Unity classes can't have constructors.)
        /// </summary>
        /// <param name="targets">List of Selectable targets.</param>
        /// <param name="oldProperties">List of Colors to save.</param>
        public void Init(List<Selectable> targets, List<Color> oldProperties) {
            for (int i = 0; i < targets.Count; i++)
            {
                oldTargetStates.Add(targets[i], oldProperties[i]);
            }
        }

        /// <summary>
        /// Restores each target to their mapped color. Cannot reverse once executed.
        /// </summary>
        public override void Undo() 
        {
            foreach(KeyValuePair<Selectable, Color> state in this.oldTargetStates)
            {
                Selectable target = state.Key;
                Color oldColor = state.Value;
                target.ChangeColor(oldColor);
            }
        }
    }
}