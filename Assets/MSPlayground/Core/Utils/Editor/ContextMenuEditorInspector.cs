using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MSPlayground.Core
{
    /// <summary>
    /// Editor inspector that fetches all context menu attribute functions from the type and shortcut gui buttons to those functions
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class ContextMenuEditorInspector : Editor
    {
        private struct ContextMenuData
        {
            public ContextMenu context;
            public MethodInfo method;

            public ContextMenuData(ContextMenu context, MethodInfo method)
            {
                this.context = context;
                this.method = method;
            }
        };

        private Type _inspectorType;
        private List<ContextMenuData> _cachedContextMenuData = new List<ContextMenuData>();

        protected virtual void OnEnable()
        {
            _inspectorType = target.GetType();

            MethodInfo[] methodInfos = _inspectorType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                                                 BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            foreach (var methodInfo in methodInfos)
            {
                ContextMenu contextMenuAttribute = methodInfo.GetCustomAttributes<ContextMenu>().FirstOrDefault();
                if (contextMenuAttribute == null)
                {
                    continue;
                }

                _cachedContextMenuData.Add(new ContextMenuData(contextMenuAttribute, methodInfo));
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (_cachedContextMenuData.Count > 0)
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.LabelField("Context Menus", EditorStyles.centeredGreyMiniLabel);
                for (int i = 0; i < _cachedContextMenuData.Count; ++i)
                {
                    string menuItemName = ObjectNames.NicifyVariableName(_cachedContextMenuData[i].context.menuItem);
                    if (GUILayout.Button(menuItemName, EditorStyles.miniButton))
                    {
                        _cachedContextMenuData[i].method.Invoke(target, new object[] { });
                    }
                }
            }
        }
    }
}