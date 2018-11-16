using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH 
{   
    /// <summary>
    /// Represents the action of deleting one or more targets. Used to save and restore dominos.
    /// </summary>
    /// <seealso cref="BH.ActionClass" />
    public class DeleteActionClass : ActionClass
    {
        List<Color> oldColors;
        List<CustomTransform> oldTransforms;

        /// <summary>
        /// Saves a 1-to-1 mapping between targets and their to-be-saved colors and transforms.
        /// Will be switched to a constructor later. (Had some confusion because many Unity classes can't have constructors.)
        /// </summary>
        /// <param name="oldColors">List of Colors representing the old coloring of the targets.</param>
        /// <param name="oldTransforms">List of CustomTransforms representing the old transforms of the targets.</param>
        public void Init(List<Color> oldColors, List<CustomTransform> oldTransforms)
        {
            this.oldColors = new List<Color>(oldColors);
            this.oldTransforms = new List<CustomTransform>(oldTransforms);
        }

        /// <summary>
        /// Respawns dominos with their original colors and transforms.
        /// </summary>
        public override void Undo() 
        {
            // Todo.
            // probably creates new selectables, creates ColorActionClass & TransformActionClass, & calls undo on both
        }
    }
}