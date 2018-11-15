using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serializable version of Unity's Transform class.
/// </summary>
[System.Serializable]
public class SerializableTransform
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SerializableTransform"/> class.
    /// </summary>
    /// <param name="transform">The transform.</param>
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

/// <summary>
/// Serializable list of SerializableTransform objects.
/// </summary>
[System.Serializable]
public class SerializableTransforms
{
    public List<SerializableTransform> _serializableTransforms;

    /// <summary>
    /// Initializes a new instance of the <see cref="SerializableTransforms"/> class.
    /// </summary>
    /// <param name="serializableTransforms">The serializable transforms.</param>
    public SerializableTransforms(SerializableTransform[] serializableTransforms)
    {
        _serializableTransforms = new List<SerializableTransform>(serializableTransforms);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SerializableTransforms"/> class.
    /// </summary>
    /// <param name="transforms">The transforms.</param>
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
        return ret.Substring(0, Mathf.Max(0, ret.Length - 2));
    }
}
