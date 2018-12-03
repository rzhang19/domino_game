using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    /// <summary>
    /// Serializable version of the <see cref="Selectable"/> class.
    /// </summary>
    [System.Serializable]
    public class SerializableSelectable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableSelectable"/> class.
        /// </summary>
        public SerializableSelectable()
        {
            _type = "haha placeholder!";
            _serializableTransform = new SerializableTransform();
            _color = new Color();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializableSelectable"/> class.
        /// </summary>
        /// <param name="selectable">The selectable to copy from.</param>
        public SerializableSelectable(Selectable selectable)
        {
            _type = "haha placeholder!";
            _serializableTransform = new SerializableTransform(selectable.transform);
            _color = selectable.GetColor();
        }

        public string _type;
        public SerializableTransform _serializableTransform;
        public Color _color;

        public override string ToString()
        {
            return "_type: " + _type + "\n"
                + "_transform: " + _serializableTransform.ToString() + "\n"
                + "_color: " + _color;
        }
    }

    [System.Serializable]
    public class SerializableSelectables
    {
        public List<SerializableSelectable> _serializableSelectables;

        public SerializableSelectables()
        {
            _serializableSelectables = new List<SerializableSelectable>();
        }

        public SerializableSelectables(SerializableSelectable[] serializableSelectables)
        {
            _serializableSelectables = new List<SerializableSelectable>(serializableSelectables);
        }
        
        public SerializableSelectables(Selectable[] selectables)
        {
            _serializableSelectables = new List<SerializableSelectable>();

            foreach (Selectable selectable in selectables)
            {
                _serializableSelectables.Add(new SerializableSelectable(selectable));
            }
        }

        public override string ToString()
        {
            string ret = "";
            foreach (SerializableSelectable serializableSelectable in _serializableSelectables)
            {
                ret += serializableSelectable.ToString() + ",\n";
            }
            return ret.Substring(0, Mathf.Max(0, ret.Length - 2));
        }
    }
}
