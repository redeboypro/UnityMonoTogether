using System.Collections.Generic;
using UnityEngine;

namespace UnityMonoTogether
{
    internal sealed class SceneController
    {
        private List<string> _sceneRoot;
        private Transform? _selectedTransform;

        private Vector2 _scrollVector;

        public SceneController()
        {
            _sceneRoot = new List<string>();
        }

        public Transform? Selected
        {
            get
            {
                return _selectedTransform;
            }
        }

        public void UpdateSceneRoot()
        {
            _sceneRoot.Clear();
            var sceneRootObjects = Object.FindObjectsOfType<Transform>();
            foreach (var obj in sceneRootObjects)
                _sceneRoot.Add(obj.name);
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            _scrollVector = GUILayout.BeginScrollView(_scrollVector);

            foreach (var objName in _sceneRoot)
                if (GUILayout.Button(objName))
                    _selectedTransform = GameObject.Find(objName).transform;

            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
        }
    }
}
