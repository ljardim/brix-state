using UnityEngine;

namespace Brix.State.Editor {
    [CreateAssetMenu(menuName = Constants.Menus.STATE + "Settings")]
    public class EditorSettings : ScriptableObject {
        public StateGraph currentGraph;

        [Header("Skin")]
        public GUISkin activeSkin;
        public GUISkin skin;

        [HideInInspector]public bool makeTransition;
        
        [Header("Nodes")]
        public CommentNode commentNode;
        public PortalNode portalNode;
        public StateNode stateNode;
        public TransitionNode transitionNode;

        public void AddNodeOnGraph(DrawNode type,
            float width,
            float height,
            string title,
            Vector3 pos,
            int transitionFromNodeIndex = 0,
            int transitionRefId = 0) {
            Node node = new Node {
                id = currentGraph.idCount,
                drawNode = type,
                windowRect = {
                    width = width,
                    height = height,
                    x = pos.x,
                    y = pos.y
                },
                windowTitle = title,
                transRef = new TransitionNodeReferences {
                    transitionId = transitionRefId
                },
                stateRef = new StateNodeReferences(),
                transitionInputNode = transitionFromNodeIndex
            };

            currentGraph.nodes.Add(node);
            currentGraph.idCount++;
        }
    }
}