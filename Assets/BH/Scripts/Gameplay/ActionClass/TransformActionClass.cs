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
        Dictionary<Component, CustomTransform> oldTargetStates = new Dictionary<Component, CustomTransform>();

        /// <summary>
        /// Saves a 1-to-1 mapping between targets and their to-be-saved transforms.
        /// Will be switched to a constructor later. (Had some confusion because many Unity classes can't have constructors.)
        /// </summary>
        /// <param name="targets">List of Selectable targets.</param>
        /// <param name="oldProperties">List of CustomTransforms to save.</param>
        public void Init(List<Component> targets, List<CustomTransform> oldProperties) {
            for (int i = 0; i < targets.Count; i++)
            {
                oldTargetStates.Add(targets[i], oldProperties[i]);
            }
        }

        /// <summary>
        /// Restores each target to their mapped transform. Cannot reverse once executed.
        /// </summary>
        public override void Undo() 
        {
            foreach(KeyValuePair<Component, CustomTransform> state in this.oldTargetStates)
            {
                Component target = state.Key;
                CustomTransform oldTransform = state.Value;
                target.transform.position = oldTransform.position;
                target.transform.rotation = oldTransform.rotation;
                target.transform.localScale = oldTransform.localScale;
            }
        }
    }
}