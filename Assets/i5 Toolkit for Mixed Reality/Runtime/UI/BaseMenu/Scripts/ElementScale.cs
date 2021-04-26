using i5.Toolkit.Core.Utilities.UnityAdapters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace i5.Toolkit.MixedReality.UI
{
    public class ElementScale : IRectangle
    {
        private Vector2 size;

        public Vector2 Size
        {
            get => size;
            set
            {
                size = value;
                ApplySize();
            }
        }

        private void ApplySize()
        {

        }
    }
}