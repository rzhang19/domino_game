using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH 
{
    public class TransformActionClass : ActionClass
    {
        public Dictionary<Component, CustomTransform> oldTargetStates = new Dictionary<Component, CustomTransform>();

        public void Init(List<Component> targets, List<CustomTransform> oldProperties) {
            for (int i = 0; i < targets.Count; i++)
            {
                oldTargetStates.Add(targets[i], oldProperties[i]);
            }
        }

        public override void Undo() 
        {
            foreach(KeyValuePair<Component, CustomTransform> state in this.oldTargetStates)
            {
                Component target = state.Key;
                CustomTransform oldTransform = state.Value;
                target.transform.position = oldTransform.position;
                target.transform.rotation = oldTransform.rotation;
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