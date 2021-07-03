using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    public interface IModelProvider
    {
        GameObject ProvideModel();
    }
}