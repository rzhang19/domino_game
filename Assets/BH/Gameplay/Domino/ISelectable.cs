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
    }
}
