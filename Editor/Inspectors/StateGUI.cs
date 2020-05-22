using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Brix.State.Editor {
    [CustomEditor(typeof(State))]
    public class StateGUI : UnityEditor.Editor {
        private SerializedObject _serializedState;
        private ReorderableList _onFixedList;
        private ReorderableList _onUpdateList;
        private ReorderableList _onEnterList;
        private ReorderableList _onExitList;
        private ReorderableList _transitions;

        private bool _showDefaultGUI;
        private bool _showActions = true;
        private bool _showTransitions = true;

        private void OnEnable() {
            _serializedState = null;
        }

        public override void OnInspectorGUI() {
            _showDefaultGUI = EditorGUILayout.Toggle("DefaultGUI", _showDefaultGUI);
            if (_showDefaultGUI) {
                base.OnInspectorGUI();
                return;
            }

            _showActions = EditorGUILayout.Toggle("Show Actions", _showActions);

            if (_serializedState == null) {
                SetupReordableLists();
            }

            _serializedState.Update();

            if (_showActions) {
                EditorGUILayout.LabelField("Actions that execute on FixedUpdate()");
                _onFixedList.DoLayoutList();
                EditorGUILayout.LabelField("Actions that execute on Update()");
                _onUpdateList.DoLayoutList();
                EditorGUILayout.LabelField("Actions that execute when entering this State");
                _onEnterList.DoLayoutList();
                EditorGUILayout.LabelField("Actions that execute when exiting this State");
                _onExitList.DoLayoutList();
            }

            _showTransitions = EditorGUILayout.Toggle("Show Transitions", _showTransitions);

            if (_showTransitions) {
                EditorGUILayout.LabelField("Conditions to exit this State");
                _transitions.DoLayoutList();
            }

            _serializedState.ApplyModifiedProperties();
        }

        private void SetupReordableLists() {
            State curState = (State) target;
            _serializedState = new SerializedObject(curState);
            _onFixedList = new ReorderableList(_serializedState, _serializedState.FindProperty("onFixed"), true, true,
                                               true, true);
            _onUpdateList = new ReorderableList(_serializedState, _serializedState.FindProperty("onUpdate"), true, true,
                                                true, true);
            _onEnterList = new ReorderableList(_serializedState, _serializedState.FindProperty("onEnter"), true, true,
                                               true, true);
            _onExitList = new ReorderableList(_serializedState, _serializedState.FindProperty("onExit"), true, true,
                                              true,
                                              true);
            _transitions = new ReorderableList(_serializedState, _serializedState.FindProperty("transitions"), true,
                                               true,
                                               true, true);

            HandleReordableList(_onFixedList, "On Fixed");
            HandleReordableList(_onUpdateList, "On Update");
            HandleReordableList(_onEnterList, "On Enter");
            HandleReordableList(_onExitList, "On Exit");
            HandleTransitionReordable(_transitions, "Condition --> New State");
        }

        private static void HandleReordableList(ReorderableList list, string targetName) {
            list.drawHeaderCallback = rect => { EditorGUI.LabelField(rect, targetName); };

            list.drawElementCallback = (rect, index, isActive, isFocused) => {
                SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element,
                                      GUIContent.none);
            };
        }

        private static void HandleTransitionReordable(ReorderableList list, string targetName) {
            list.drawHeaderCallback = rect => { EditorGUI.LabelField(rect, targetName); };

            list.drawElementCallback = (rect, index, isActive, isFocused) => {
                SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width * .3f, EditorGUIUtility.singleLineHeight),
                                      element.FindPropertyRelative("condition"), GUIContent.none);
                EditorGUI.ObjectField(
                    new Rect(rect.x + +(rect.width * .35f), rect.y, rect.width * .3f,
                             EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("targetState"),
                    GUIContent.none);
                EditorGUI.LabelField(
                    new Rect(rect.x + +(rect.width * .75f), rect.y, rect.width * .2f,
                             EditorGUIUtility.singleLineHeight), "Disable");
                EditorGUI.PropertyField(
                    new Rect(rect.x + +(rect.width * .90f), rect.y, rect.width * .2f,
                             EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("disable"),
                    GUIContent.none);
            };
        }
    }
}