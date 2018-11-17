using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    [System.Serializable]
    public class SerializableSelectable
    {
        public SerializableSelectable()
        {
            _type = "haha placeholder!";
            _serializableTransform = new SerializableTransform();
            _color = new Color();
        }

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
