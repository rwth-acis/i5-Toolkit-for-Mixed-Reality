using System.Threading.Tasks;
using UnityEngine;

namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    public interface IModelProvider
    {
        Task<GameObject> ProvideModelAsync();
    }
}