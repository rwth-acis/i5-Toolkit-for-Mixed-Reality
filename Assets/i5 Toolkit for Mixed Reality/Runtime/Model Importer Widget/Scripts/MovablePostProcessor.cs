using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    public class MovablePostProcessor : IModelImportPostProcessor
    {
        public void PostProcessGameObject(GameObject gameObject)
        {
            gameObject.AddComponent<ObjectManipulator>();
        }
    }
}