using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    /// <summary>
    /// Encapsulates a stack representing the history of actions that manipulate Selectables.
    /// </summary>
    public class ActionHistory
    {
        Stack<ActionClass> actions;
        Dictionary<int, HashSet<ActionClass>> instances;
        
        /// <summary>
        /// Initializes to an empty stack.
        /// </summary>
        public ActionHistory() 
        { 
            actions = new Stack<ActionClass>();
            instances = new Dictionary<int, HashSet<ActionClass>>();
        }
        
        /*
        public ActionHistory(Stack<ActionClass> existingHistory)
        {
            ActionClass actions[] = existingHistory.ToArray();
            foreach (ActionClass a in actions)
            {
                List<int> instanceIDs = a.GetTargetIDs();
                foreach (int id in instanceIDs)
                {
                    if (!instances.ContainsKey(id))
                    {
                        instances[id] = new HashSet<ActionClass>(a);
                    }
                    instances[id].Add(a);
                }
            }
        }
        */

        /// <summary>
        /// Pushes a color action onto the stack and updates its internal Selectable-to-action map.
        /// <param name='targets'>A <c>Dictionary&lt;Selectable,Color&gt;</c> mapping each updated Selectable to its color. </param>
        /// </summary>
        public void PushColorAction(Dictionary<Selectable, Color> targets)
        {
            ColorActionClass SavedColorsAction = new ColorActionClass();
            List<Selectable> selectables = new List<Selectable>(targets.Keys);
            SavedColorsAction.Init(selectables, new List<Color>(targets.Values));
            actions.Push(SavedColorsAction);
            AddActionOnSelectablesToInstancesMap(selectables, SavedColorsAction);
        }

        /// <summary>
        /// Pushes a transform action onto the stack and updates its internal Selectable-to-action map.
        /// <param name='targets'>A <c>Dictionary&lt;Selectable,CustomTransform&gt;</c> mapping each updated Selectable to its transform. </param>
        /// </summary>
        public void PushTransformAction(Dictionary<Selectable, CustomTransform> targets)
        {
            TransformActionClass SavedTransformsAction = new TransformActionClass();
            List<Selectable> selectables = new List<Selectable>(targets.Keys);
            SavedTransformsAction.Init(selectables, new List<CustomTransform>(targets.Values));
            actions.Push(SavedTransformsAction);
            AddActionOnSelectablesToInstancesMap(selectables, SavedTransformsAction);
        }

        /// <summary>
        /// Pushes an add action onto the stack and updates its internal Selectable-to-action map.
        /// <param name='targets'>A <c>List&lt;Selectable&gt;</c> containing the Selectables to be added. </param>
        /// </summary>
        public void PushAddAction(List<Selectable> selectables)
        {
            AddActionClass SavedAddsAction = new AddActionClass();
            SavedAddsAction.Init(selectables);
            actions.Push(SavedAddsAction);
            AddActionOnSelectablesToInstancesMap(selectables, SavedAddsAction);
        }

        /// <summary>
        /// Pushes a delete action onto the stack and updates its internal Selectable-to-action map.
        /// <param name='targets'>A <c>List&lt;Selectable&gt;</c> containing the Selectables to be deleted. </param>
        /// </summary>
        public void PushDeleteAction(List<Selectable> selectables)
        {
            DeleteActionClass SavedDeletesAction = new DeleteActionClass();
            SavedDeletesAction.Init(selectables);
            actions.Push(SavedDeletesAction);
            AddActionOnSelectablesToInstancesMap(selectables, SavedDeletesAction);
        }      

        /// <summary>
        /// Attempts an undo by popping the last action off the stack.
        /// If the action was a delete, undoing it has changed some Selectable IDs by respawning them,
        /// so update all relevant actions to use the new Selectable instances.
        /// </summary>
        /// <returns>
        ///     <c>True</c> if the undo succeeded. 
        ///     Note: If it returns false, the action history may be corrupted.
        /// </returns>
        public bool AttemptUndo()
        {
            if (actions.Count <= 0) return false;

            ActionClass actionToUndo = actions.Pop();

            // Remove action from all instance-action mappings in this.instances
            List<int> affectedSelectableIDs = actionToUndo.GetTargetIDs();
            foreach (int id in affectedSelectableIDs)
            {
                HashSet<ActionClass> actionsOnInstance;
                if (instances.TryGetValue(id, out actionsOnInstance))
                {
                    actionsOnInstance.Remove(actionToUndo);
                }  
            }

            actionToUndo.Undo();

            // If we just undid a delete, update all relevant actions to the new Selectable instances
            if (actionToUndo.GetType() == typeof(DeleteActionClass))
            {
                DeleteActionClass deleteAction = (DeleteActionClass)actionToUndo;
                Dictionary<int, Selectable> updatedSelectableInstances = deleteAction.GetUpdatedInstances();
                foreach (KeyValuePair<int, Selectable> update in updatedSelectableInstances)
                {
                    int oldID = update.Key;
                    Selectable newSelectable = update.Value;
                    HashSet<ActionClass> oldActions;
                    if (instances.TryGetValue(oldID, out oldActions))
                    {
                        instances.Remove(oldID);
                        int newID = newSelectable.GetInstanceID();
                        if (!instances.ContainsKey(newID))
                        {
                            instances.Add(newID, oldActions);
                            foreach (ActionClass a in oldActions)
                            {
                                a.UpdateInstance(oldID, newSelectable);
                            }
                        }
                        else
                        {
                            Debug.Log("Error: newSelectable's Unity instance ID isn't unique. Should never happen");
                            return false;
                        }
                    }
                    else
                    {
                        Debug.Log("Error: ActionHistory's stack inconsistent with a DeleteAction. Should never happen");
                        return false;
                    }
                }
            }

            return true;
        }

        void AddActionOnSelectablesToInstancesMap(List<Selectable> selectables, ActionClass action)
        {
            foreach (Selectable sel in selectables)
            {
                int ID = sel.GetInstanceID();
                if (!instances.ContainsKey(ID))
                {
                    instances[ID] = new HashSet<ActionClass>();
                }
                instances[ID].Add(action);
            }
        }
    }
}