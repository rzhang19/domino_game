using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    /// <summary>
    /// Represents an action that can be performed on targets, e.g. dominos.
    /// Should be used to save and/or revert the action.
    /// Kept as an abstract class rather than interface to extrapolate functions from subclasses in the future.
    /// </summary>
    /// <seealso cref="BH.ColorActionClass" />
    /// <seealso cref="BH.TransformActionClass" />
    public abstract class ActionClass {
        
        /// <summary>
        /// Undos the action forever.
        /// </summary>
        abstract public void Undo();
        
    }
}