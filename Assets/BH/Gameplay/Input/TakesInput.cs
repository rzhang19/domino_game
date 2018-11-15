using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    /// <summary>
    /// Manages locks on player inputs.
    /// If a class requires reading player input, it should extend from this class.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    abstract public class TakesInput : MonoBehaviour
    {
        protected HashSet<Object> _locks = new HashSet<Object>(); // Should be used to stop the polling of input in child scripts

        /// <summary>
        /// Locks the input with Object o.
        /// </summary>
        /// <param name="o">The object.</param>
        public void LockInput(Object o)
        {
            _locks.Add(o);
        }

        /// <summary>
        /// Unlocks the input with Object o.
        /// </summary>
        /// <param name="o">The object.</param>
        public void UnlockInput(Object o)
        {
            _locks.Remove(o);
        }

        /// <summary>
        /// Locks the inputs with Object o.
        /// </summary>
        /// <param name="tis">The array of TakesInput.</param>
        /// <param name="o">The object.</param>
        public static void LockInputs(TakesInput[] tis, Object o)
        {
            if (tis == null)
                return;

            foreach (TakesInput ti in tis)
            {
                ti.LockInput(o);
            }
        }

        /// <summary>
        /// Unlocks the inputs with Object o.
        /// </summary>
        /// <param name="tis">The array of TakesInput.</param>
        /// <param name="o">The object.</param>
        public static void UnlockInputs(TakesInput[] tis, Object o)
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
