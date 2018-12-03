using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BH
{
    /// <summary>
    /// Serializable data that represents game saves, i.e. in-game domino layouts.
    /// </summary>
    [System.Serializable]
    public class Data
    {
        /// <summary>Initializes a new instance of the <see cref="Data"/> class.</summary>
        public Data()
        {
            _serializableSelectables = new SerializableSelectables();
        }

        /// <summary>Initializes a new instance of the <see cref="Data"/> class.</summary>
        /// <param name="selectables">The selectables to be contained in this instance.</param>
        public Data(Selectable[] selectables)
        {
            _serializableSelectables = new SerializableSelectables(selectables);
        }
        
        public SerializableSelectables _serializableSelectables;

        public override string ToString()
        {
            return "_serializableSelectables: " + _serializableSelectables.ToString();
        }
    }
}
