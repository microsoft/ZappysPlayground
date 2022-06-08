using System.Collections;
using System.Collections.Generic;
using MSPlayground.Common.Helper;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MSPlayground.Core
{
    public class TextSlateController : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private TextMeshProUGUI _textObject;
        [SerializeField] private Color _defaultColor = Color.white;
        [SerializeField] private int _maxItems = 50;

        public void AddText(string line)
        {
            AddText(line, _defaultColor);
        }

        public void AddText(string line, Color color)
        {
            TextMeshProUGUI newLine = Instantiate<TextMeshProUGUI>(_textObject);

            newLine.text = line;
            newLine.color = color;

            AddObject(newLine.gameObject);
        }

        void AddObject(GameObject go)
        {
            go.SetActive(true);
            Transform contentTransform = _scrollRect.content.transform;

            // lazy initialization due to order of Start calls
            if (contentTransform==null)
            {
                return;
            }

            go.transform.SetParent(contentTransform, false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform as RectTransform);

            int itemsToDelete = contentTransform.childCount - _maxItems;
            for (int i = 0; i < itemsToDelete; i++)
            {
                GameObject.Destroy(contentTransform.GetChild(0).gameObject);
            }

            // If console is active, wait a frame before updating vertical normalized position
            if (go.activeInHierarchy)
            {
                StartCoroutine(Coroutines.WaitOneFrame(() => _scrollRect.verticalNormalizedPosition = 0));
            }
        }

        private void OnEnable()
        {
            StartCoroutine(Coroutines.WaitOneFrame(() => _scrollRect.verticalNormalizedPosition = 0));
        }
    }
}