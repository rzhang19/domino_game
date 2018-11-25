﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    /// <summary>
    /// Represents an action that can be performed on targets, e.g. dominos.
    /// Should be used to save and/or revert the action.
    /// Once reverted, action is useless and shouldn't be reused.
    /// Kept as an abstract class rather than interface to extrapolate functions from subclasses in the future.
    /// </summary>

    public abstract class ActionClass {
        
        /// <summary>
        /// Undos the action forever.
        /// </summary>
        abstract public void Undo();

        /// <summary>
        /// Returns a list of instance IDs of the targets of the action. 
        /// Instance IDs should be generated by Unity's Object.GetInstanceID().
        /// </summary>
        abstract public List<int> GetTargetIDs();

        /// <summary>
        /// Updates the instance of a target identified by the ID of its old instance.
        /// Useful when a target instance changes during an action's lifetime.
        /// </summary>
        abstract public void UpdateInstance(int oldID, Selectable newInstance);
    }
}