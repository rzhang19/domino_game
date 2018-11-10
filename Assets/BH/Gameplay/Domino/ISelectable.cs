using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BH
{
    public interface ISelectable
    {
        bool IsSelected();
        void Select();
        void Deselect();
        void Toggle();
        void Delete();
        void Rotate(Vector3 point, Vector3 axis, float deg);
    }
}
