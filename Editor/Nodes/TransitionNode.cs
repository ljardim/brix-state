using UnityEditor;
using UnityEngine;

namespace Brix.State.Editor {
    [CreateAssetMenu(menuName = Constants.Menus.STATE + "Nodes/Transition Node")]
    public class TransitionNode : DrawNode {
        public override void DrawWindow(Node inputNode) {
            EditorGUILayout.LabelField("");
            var transitionInputNode =
                BrixEditor.Settings.currentGraph.GetNodeById(inputNode.transitionInputNode);
            if (transitionInputNode == null) {
                return;
            }

            if (transitionInputNode.stateRef.currentState == null) {
                BrixEditor.Settings.currentGraph.MarkNodeForDeletion(inputNode.id);
                return;
            }

            var transition =
                transitionInputNode.stateRef.currentState.GetTransition(inputNode.transRef.transitionId);

            if (transition == null) {
                return;
            }

            transition.condition =
                (Condition) EditorGUILayout.ObjectField(transition.condition, typeof(Condition), false);

            if (transition.condition == null) {
                EditorGUILayout.LabelField("No Condition!");
                inputNode.isAssigned = false;
            } else {
                inputNode.isAssigned = true;
                if (inputNode.isDuplicate) {
                    EditorGUILayout.LabelField("Duplicate Condition!");
                } else {
                    GUILayout.Label(transition.condition.description);

                    var targetNode =
                        BrixEditor.Settings.currentGraph.GetNodeById(inputNode.transitionTargetNode);
                    if (targetNode == null) {
                        transition.targetState = null;
                    } else {
                        transition.targetState = targetNode.isDuplicate ? null : targetNode.stateRef.currentState;
                    }
                }
            }

            if (inputNode.transRef.previousCondition == transition.condition) {
                return;
            }

            inputNode.transRef.previousCondition = transition.condition;
            inputNode.isDuplicate = BrixEditor.Settings.currentGraph.IsTransitionDuplicate(inputNode);

            if (!inputNode.isDuplicate) {
                BrixEditor.ForceSetDirty = true;
            }
        }

        public override void DrawCurve(Node inputNode) {
            var inputNodeRect = inputNode.windowRect;
            inputNodeRect.y += inputNode.windowRect.height * .5f;
            inputNodeRect.width = 1;
            inputNodeRect.height = 1;

            var fromNode = BrixEditor.Settings.currentGraph.GetNodeById(inputNode.transitionInputNode);
            if (fromNode == null) {
                BrixEditor.Settings.currentGraph.MarkNodeForDeletion(inputNode.id);
            } else {
                var targetColor = Color.green;
                if (!inputNode.isAssigned || inputNode.isDuplicate) {
                    targetColor = Color.red;
                }

                var fromRect = fromNode.windowRect;
                BrixEditor.DrawNodeCurve(fromRect, inputNodeRect, true, targetColor);
            }

            if (inputNode.isDuplicate) {
                return;
            }

            if (inputNode.transitionTargetNode > 0) {
                var targetNode = BrixEditor.Settings.currentGraph.GetNodeById(inputNode.transitionTargetNode);
                if (targetNode == null) {
                    inputNode.transitionTargetNode = -1;
                } else {
                    inputNodeRect = inputNode.windowRect;
                    inputNodeRect.x += inputNodeRect.width;
                    var targetRect = targetNode.windowRect;
                    targetRect.x -= targetRect.width * .5f;

                    var targetColor = Color.green;
                    if (targetNode.drawNode is StateNode) {
                        if (!targetNode.isAssigned || targetNode.isDuplicate) {
                            targetColor = Color.red;
                        }
                    } else {
                        targetColor = targetNode.isAssigned ? Color.yellow : Color.red;
                    }

                    BrixEditor.DrawNodeCurve(inputNodeRect, targetRect, false, targetColor);
                }
            }
        }
    }
}