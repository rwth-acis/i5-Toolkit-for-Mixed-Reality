using UnityEngine;

namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    [RequireComponent(typeof(ModelInstantiatorBehaviour))]
    public abstract class InstantiationEffectBaseBehaviour : MonoBehaviour
    {
        protected IInstantiationEffect instantiationEffectLogic;

        protected virtual void Awake()
        {
            InitializeEffect();
        }

        protected virtual void Start()
        {
            GetComponent<ModelInstantiatorBehaviour>().ModelInstantiator.InstantiationEffect = instantiationEffectLogic;
        }

        protected abstract void InitializeEffect();
    }
}