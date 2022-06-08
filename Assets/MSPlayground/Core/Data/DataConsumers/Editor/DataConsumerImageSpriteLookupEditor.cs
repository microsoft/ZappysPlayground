using UnityEngine;
using UnityEditor;
using System.Collections;
using MSPlayground.Core.Data;

namespace MSPlayground.Core.Data.Editor
{
    /// <summary>
    /// An editor to provide the user with some visual feedback on whether the attached data consumer can
    /// actually target any appropriate children components of the given type(s). Useful especially for similar
    /// data consumers that are not cross-compatible in Canvas UI and Non-Canvas UI.
    /// </summary>
    [CustomEditor(typeof(DataConsumerImageSpriteLookup))]
    public class DataConsumerImageSpriteLookupEditor : UnityEditor.Editor
    {
        private const string WARNING_MESSAGE_NO_CHILD_COMPONENTS_FOUND =
            "There were no {UnityEngine.UI.Image} components found in children. Are you sure you don't want to use the " +
            "{DataConsumerSpriteLookup} component which targets {SpriteRenderers} instead?";
        private DataConsumerImageSpriteLookup _instance;

        void OnEnable()
        {
            _instance = (DataConsumerImageSpriteLookup) target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            UnityEngine.UI.Image image = _instance.GetComponentInChildren<UnityEngine.UI.Image>();
            
            if (image == null)
            {
                // Warn user that there are no target components found in children
                EditorGUILayout.HelpBox(WARNING_MESSAGE_NO_CHILD_COMPONENTS_FOUND, MessageType.Warning);
            }
        }
    }
}