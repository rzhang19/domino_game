using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    abstract public class TakesInput : MonoBehaviour
    {
        protected HashSet<Object> _locks = new HashSet<Object>(); // Should be used to stop the polling of input in child scripts

        public void LockInput(Object o)
        {
            _locks.Add(o);
        }

        public void UnlockInput(Object o)
        {
            _locks.Remove(o);
        }

        public static void DisableInputs(TakesInput[] tis, Object o)
        {
            if (tis == null)
                return;

            foreach (TakesInput ti in tis)
            {
                ti.LockInput(o);
            }
        }

        public static void EnableInputs(TakesInput[] tis, Object o)
        {
            if (tis == null)
                return;

            foreach (TakesInput ti in tis)
            {
                ti.UnlockInput(o);
            }
        }
    }
}
