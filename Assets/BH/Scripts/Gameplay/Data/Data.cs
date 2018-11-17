using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BH
{
    [System.Serializable]
    public class Data
    {
        public Data()
        {
            _serializableSelectables = new SerializableSelectables();
        }

        public Data(Selectable[] selectables)
        {
            _serializableSelectables = new SerializableSelectables(selectables);
        }

        // Data only contains a list of selectables for now.
        public SerializableSelectables _serializableSelectables;

        public override string ToString()
        {
            return "_serializableSelectables: " + _serializableSelectables.ToString();
        }
    }
}
