using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    public class ModelImporterBehaviour : MonoBehaviour
    {
        [SerializeField] private ModelInstantiatorBehaviour modelInstantiator;
        [SerializeField] private ModelListViewBehaviour modelListView;

        private IModelProvider modelProvider;

        public IModelProvider ModelProvider
        {
            get => modelProvider;
            set
            {
                modelProvider = value;
                ModelProviderChanged();
            }
        }

        private void OnEnable()
        {
            modelListView.ModelListItemSelected += OnItemSelected;
        }

        private async void OnItemSelected(int selectedIndex)
        {
            GameObject instantiated = await ModelProvider.ProvideModelAsync(modelListView.DataItems[selectedIndex].id);
            await modelInstantiator.PresentModelAsync(instantiated);
        }

        private void OnDisable()
        {
            modelListView.ModelListItemSelected -= OnItemSelected;
        }

        private async void ModelProviderChanged()
        {
            modelListView.DataItems = await modelProvider.ListAvailableModelsAsync(0, 100);
        }
    }
}