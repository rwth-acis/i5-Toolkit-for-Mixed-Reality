using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace i5.Toolkit.MixedReality.UI
{
    public class WindowScaleManager : MonoBehaviour
    {
        private const float unityUnitsToRectTransform = 78.2f;
        private const float buttonWidth = 0.085f;

        [Header("Header")]
        [SerializeField] private Transform headerBackPlate;
        [SerializeField] private Transform titleBar;
        [SerializeField] private RectTransform headerText;
        [SerializeField] private Transform headerButtonParent;

        [Header("Content")]
        [SerializeField] private Transform contentBackPlate;
        [SerializeField] private Transform backdrop;

        public Vector2 size = new Vector2(0.85f, 0.47f);
        public int numberOfButtons = 2;

        private BoxCollider mainCollider;
        private BoxCollider headerCollider;

        private void OnValidate()
        {
            if (mainCollider == null)
            {
                mainCollider = GetComponent<BoxCollider>();
            }
            if (headerCollider == null && titleBar != null)
            {
                headerCollider = titleBar.GetComponent<BoxCollider>();
            }

            headerBackPlate.localScale = new Vector3(size.x, headerBackPlate.localScale.y, headerBackPlate.localScale.z);
            titleBar.localPosition = new Vector3(titleBar.localPosition.x, size.y / 2f + 0.05f, titleBar.localPosition.z);
            contentBackPlate.localScale = new Vector3(size.x, size.y, contentBackPlate.localScale.z);
            headerText.localPosition = new Vector3(-size.x / 2f + 0.02f, headerText.localPosition.y, headerText.localPosition.z);
            headerText.sizeDelta = new Vector2((size.x - 0.02f - buttonWidth * numberOfButtons) * unityUnitsToRectTransform, headerText.sizeDelta.y);
            headerButtonParent.localPosition = new Vector3(size.x / 2f - 0.02f, headerButtonParent.localPosition.y, headerButtonParent.localPosition.z);
            headerCollider.size = new Vector3(size.x, headerCollider.size.y, headerCollider.size.z);

            backdrop.localScale = new Vector3(size.x + 0.06f, size.y + 0.15f, backdrop.localScale.z);
            mainCollider.size = new Vector3(size.x + 0.1f, size.y + 0.16f, mainCollider.size.z);
        }
    }
}