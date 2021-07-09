using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    public interface IInstantiationEffect
    {
        Task PlayInstantiationEffectAsync(GameObject target);
    }
}