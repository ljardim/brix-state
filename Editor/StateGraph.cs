using System.Collections.Generic;
using UnityEngine;

namespace Brix.State.Editor {
    [CreateAssetMenu(menuName = Constants.Menus.STATE + "New StateGraph")]
    public class StateGraph : ScriptableObject {
        private readonly List<int> _nodesToDeleteById = new List<int>();
        [SerializeField]public int idCount;
        [SerializeField]public List<Node> nodes = new List<Node>();

        #region Checkers

        public Node GetNodeById(int nodeId) {
            foreach (Node node in nodes) {
                if (node.id == nodeId) {
                    return node;
                }
            }

            return null;
        }

        public bool DeleteNodesMarkedForDelete() {
            var deletedItems = false;
            foreach (var nodeId in _nodesToDeleteById) {
                Node node = GetNodeById(nodeId);
                if (node == null) {
                    continue;
                }

                deletedItems = true;
                nodes.Remove(node);
            }

            if (deletedItems) {
                _nodesToDeleteById.Clear();
            }

            return deletedItems;
        }

        public void MarkNodeForDeletion(int nodeId) {
            if (!_nodesToDeleteById.Contains(nodeId)) {
                _nodesToDeleteById.Add(nodeId);
            }
        }

        public bool IsStateDuplicate(Node inputNode) {
            foreach (Node node in nodes) {
                if (node.id == inputNode.id) {
                    continue;
                }

                if (node.stateRef.currentState == inputNode.stateRef.currentState && !node.isDuplicate) {
                    return true;
                }
            }

            return false;
        }

        public bool IsTransitionDuplicate(Node inputNode) {
            Node inputTransition = GetNodeById(inputNode.transitionInputNode);
            if (inputTransition == null) {
                return false;
            }

            foreach (Transition transition in inputTransition.stateRef.currentState.transitions) {
                if (transition.condition == inputNode.transRef.previousCondition &&
                    inputNode.transRef.transitionId != transition.id) {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}