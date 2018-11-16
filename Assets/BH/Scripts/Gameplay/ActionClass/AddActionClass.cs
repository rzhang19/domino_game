using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH 
{   
    /// <summary>
    /// Represents the action of creating target(s). Used to undo domino creation.
    /// </summary>
    /// <seealso cref="BH.ActionClass" />
    public class AddActionClass : ActionClass
    {
        List<Selectable> targets;

        /// <summary>
        /// Saves the created targets.
        /// Will be switched to a constructor later. (Had some confusion because many Unity classes can't have constructors.)
        /// </summary>
        /// <param name="targets">List of Selectable targets that were created.</param>
        public void Init(List<Selectable> targets) {
            this.targets = new List<Selectable>(targets);
        }

        /// <summary>
        /// Deletes the saved target.
        /// </summary>
        public override void Undo() 
        {
            // Todo
        }
    }
}