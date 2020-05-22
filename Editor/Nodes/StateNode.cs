using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Brix.State.Editor {
    [CreateAssetMenu(menuName = Constants.Menus.STATE + "Nodes/State Node")]
    public class StateNode : DrawNode {
        public override void DrawWindow(Node inputNode) {
            if (inputNode.stateRef.currentState == null) {
                EditorGUILayout.LabelField("Add state to modify:");
            } else {
                if (inputNode.collapse) {
                    inputNode.windowRect.height = 100;
                }

                inputNode.collapse = EditorGUILayout.Toggle(" ", inputNode.collapse);
            }

            inputNode.stateRef.currentState =
                (State) EditorGUILayout.ObjectField(inputNode.stateRef.currentState, typeof(State), false);

            inputNode.previousCollapse = inputNode.collapse;

            if (inputNode.stateRef.previousState != inputNode.stateRef.currentState) {
                inputNode.isDuplicate = BrixEditor.settings.currentGraph.IsStateDuplicate(inputNode);
                inputNode.stateRef.previousState = inputNode.stateRef.currentState;

                if (!inputNode.isDuplicate) {
                    var pos = new Vector3(inputNode.windowRect.x, inputNode.windowRect.y, 0);
                    pos.x += inputNode.windowRect.width * 2;

                    SetupReordableLists(inputNode);

                    //Load transtions
                    for (var i = 0; i < inputNode.stateRef.currentState.transitions.Count; i++) {
                        pos.y += i * 100;
                        BrixEditor.AddTransitionNodeFromTransition(
                            inputNode.stateRef.currentState.transitions[i], inputNode, pos);
                    }

                    BrixEditor.forceSetDirty = true;
                }
            }

            if (inputNode.isDuplicate) {
                EditorGUILayout.LabelField("State is a duplicate!");
                inputNode.windowRect.height = 100;
                return;
            }

            if (inputNode.stateRef.currentState != null) {
                inputNode.isAssigned = true;

                if (inputNode.collapse) {
                    return;
                }

                if (inputNode.stateRef.serializedState == null) {
                    SetupReordableLists(inputNode);
                }

                float standard = 150;
                inputNode.stateRef.serializedState.Update();
                inputNode.showActions = EditorGUILayout.Toggle("Show Actions ", inputNode.showActions);
                if (inputNode.showActions) {
                    EditorGUILayout.LabelField("");
                    inputNode.stateRef.onFixedList.DoLayoutList();
                    EditorGUILayout.LabelField("");
                    inputNode.stateRef.onUpdateList.DoLayoutList();
                    standard +=
                        100 + 40 + (inputNode.stateRef.onUpdateList.count + inputNode.stateRef.onFixedList.count) * 20;
                }

                inputNode.showEnterExit = EditorGUILayout.Toggle("Show Enter/Exit ", inputNode.showEnterExit);
                if (inputNode.showEnterExit) {
                    EditorGUILayout.LabelField("");
                    inputNode.stateRef.onEnterList.DoLayoutList();
                    EditorGUILayout.LabelField("");
                    inputNode.stateRef.onExitList.DoLayoutList();
                    standard +=
                        100 + 40 + (inputNode.stateRef.onEnterList.count + inputNode.stateRef.onExitList.count) * 20;
                }

                inputNode.stateRef.serializedState.ApplyModifiedProperties();
                inputNode.windowRect.height = standard;
            } else {
                inputNode.isAssigned = false;
            }
        }

        public override void DrawCurve(Node inputNode) { }

        public static Transition AddTransition(Node inputNode) {
            return inputNode.stateRef.currentState.AddTransition();
        }

        private static void SetupReordableLists(Node inputNode) {
            inputNode.stateRef.serializedState = new SerializedObject(inputNode.stateRef.currentState);
            inputNode.stateRef.onFixedList = new ReorderableList(inputNode.stateRef.serializedState,
                inputNode.stateRef.serializedState.FindProperty(
                    "onFixed"), true, true, true, true);
            inputNode.stateRef.onUpdateList = new ReorderableList(inputNode.stateRef.serializedState,
                inputNode.stateRef.serializedState.FindProperty(
                    "onUpdate"), true, true, true, true);
            inputNode.stateRef.onEnterList = new ReorderableList(inputNode.stateRef.serializedState,
                inputNode.stateRef.serializedState.FindProperty(
                    "onEnter"), true, true, true, true);
            inputNode.stateRef.onExitList = new ReorderableList(inputNode.stateRef.serializedState,
                inputNode.stateRef.serializedState.FindProperty(
                    "onExit"), true, true, true, true);

            HandleReordableList(inputNode.stateRef.onFixedList, "On Fixed");
            HandleReordableList(inputNode.stateRef.onUpdateList, "On Update");
            HandleReordableList(inputNode.stateRef.onEnterList, "On Enter");
            HandleReordableList(inputNode.stateRef.onExitList, "On Exit");
        }

        private static void HandleReordableList(ReorderableList list, string targetName) {
            list.drawHeaderCallback = rect => { EditorGUI.LabelField(rect, targetName); };

            list.drawElementCallback = (rect, index, isActive, isFocused) => {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element,
                    GUIContent.none);
            };
        }
    }
}