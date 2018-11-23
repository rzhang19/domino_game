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
        List<SelectableFields> savedSelectables;

        /// <summary>
        /// Saves a 1-to-1 mapping between targets and their to-be-saved colors and transforms.
        /// Will be switched to a constructor later. (Had some confusion because many Unity classes can't have constructors.)
        /// </summary>
        /// <param name="targets">List of Selectables representing the deleted targets.</param>
        public void Init(List<Selectable> targets)
        {
            savedSelectables = new List<SelectableFields>();
            SelectableFields test = new SelectableFields();
            foreach (Selectable sel in targets)
            {
                savedSelectables.Add(new SelectableFields(sel.GetColor(), new CustomTransform(sel.transform)));
            }
        }

        /// <summary>
        /// Respawns dominos with their original colors and transforms.
        /// </summary>
        public override void Undo() 
        {
            foreach (SelectableFields selFields in savedSelectables)
            {
                Color newColor = selFields.color;
                CustomTransform newTransform = selFields.transform;
                Selectable newSel = SelectableManager.Instance.SpawnSelectable(newTransform.position, newTransform.rotation);
                newSel.SetColor(newColor);
            }
        }

        // Extrapolated all fields of Selectables that should be saved.
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