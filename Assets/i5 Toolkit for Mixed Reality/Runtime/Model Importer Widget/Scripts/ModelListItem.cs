using i5.Toolkit.Core.Experimental.UnityAdapters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    public class ModelListItem
    {
        public IBoxVolume ModelTargetVolume { get; private set; }

        public ITextDisplay NameLabel { get; private set; }
    }
}