using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableTransform
{
    public SerializableTransform(Transform transform)
    {
        _position = transform.position;
        _rotation = transform.rotation;
        _scale = transform.localScale;
    }

    public Vector3 _position;
    public Quaternion _rotation;
    public Vector3 _scale;

    public override string ToString()
    {
        return _position + " " + _rotation.eulerAngles + " " + _scale;
    }
}

[System.Serializable]
public class SerializableTransforms
{
    public List<SerializableTransform> _serializableTransforms;

    public SerializableTransforms(SerializableTransform[] serializableTransforms)
    {
        _serializableTransforms = new List<SerializableTransform>(serializableTransforms);
    }

    public SerializableTransforms(Transform[] transforms)
    {
        _serializableTransforms = new List<SerializableTransform>();

        foreach (Transform transform in transforms)
        {
            _serializableTransforms.Add(new SerializableTransform(transform));
        }
    }

    public override string ToString()
    {
        string ret = "";
        foreach (SerializableTransform serializableTransform in _serializableTransforms)
        {
            ret += serializableTransform.ToString() + ",\n";
        }
        return ret.Substring(0, ret.Length - 2);
    }
}
