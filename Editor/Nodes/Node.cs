using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Brix.State.Editor {
    [System.Serializable]
    public class Node {
        public int id;
        public DrawNode drawNode;
        public Rect windowRect;
        public string windowTitle;
        public int transitionInputNode;
        public int transitionTargetNode;
        public bool isDuplicate;
        public string comment = "This is a comment";
        public bool isAssigned;

        public bool collapse;
        [HideInInspector] public bool previousCollapse;

        public bool showActions = true;
        public bool showEnterExit = false;

        [SerializeField] public StateNodeReferences stateRef;
        [SerializeField] public TransitionNodeReferences transRef;

        public void DrawWindow() {
            if (drawNode != null) {
                drawNode.DrawWindow(this);
            }
        }

        public void DrawCurve() {
            if (drawNode != null) {
                drawNode.DrawCurve(this);
            }
        }
    }

    [System.Serializable]
    public class StateNodeReferences {
        public State currentState;
        [HideInInspector] public State previousState;
        public SerializedObject serializedState;

        public ReorderableList onFixedList;
        public ReorderableList onUpdateList;
        public ReorderableList onEnterList;
        public ReorderableList onExitList;
    }

    [System.Serializable]
    public class TransitionNodeReferences {
        [HideInInspector] public Condition previousCondition;
        public int transitionId;
    }
}