using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH 
{
    public class ColorActionClass : ActionClass
    {
        public Dictionary<Selectable, Color> oldTargetStates = new Dictionary<Selectable, Color>();

        public void Init(List<Selectable> targets, List<Color> oldProperties) {
            for (int i = 0; i < targets.Count; i++)
            {
                oldTargetStates.Add(targets[i], oldProperties[i]);
            }
        }

        public override void Undo() 
        {
            foreach(KeyValuePair<Selectable, Color> state in this.oldTargetStates)
            {
                Selectable target = state.Key;
                Color oldColor = state.Value;
                target.ChangeColor(oldColor);
            }
        }

        // Use this for initialization
        void Start () {
            
        }
        
        // Update is called once per frame
        void Update () {
            
        }
        
    }
}