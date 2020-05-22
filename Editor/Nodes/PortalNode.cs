using UnityEngine;
using UnityEditor;

namespace Brix.State.Editor {
    [CreateAssetMenu(menuName = Constants.Menus.STATE + "Nodes/Portal Node")]
    public class PortalNode : DrawNode {
        public override void DrawWindow(Node inputNode) {
            inputNode.stateRef.currentState =
                (State) EditorGUILayout.ObjectField(inputNode.stateRef.currentState, typeof(State), false);
            inputNode.isAssigned = inputNode.stateRef.currentState != null;

            if (inputNode.stateRef.previousState == inputNode.stateRef.currentState) {
                return;
            }

            inputNode.stateRef.previousState = inputNode.stateRef.currentState;
            BrixEditor.forceSetDirty = true;
        }

        public override void DrawCurve(Node inputNode) { }
    }
}