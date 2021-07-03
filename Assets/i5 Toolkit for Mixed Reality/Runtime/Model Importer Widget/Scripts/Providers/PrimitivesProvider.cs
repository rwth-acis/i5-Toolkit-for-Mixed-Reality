using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    public class PrimitivesProvider : IModelProvider
    {
        public GameObject ProvideModel()
        {
            return GameObject.CreatePrimitive(PrimitiveType.Capsule);
        }
    }
}
