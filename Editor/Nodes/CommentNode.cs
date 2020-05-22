using UnityEngine;

namespace Brix.State.Editor {
    [CreateAssetMenu(menuName = Constants.Menus.STATE + "Nodes/Comment Node")]
    public class CommentNode : DrawNode {
        public override void DrawWindow(Node inputNode) {
            inputNode.comment = GUILayout.TextArea(inputNode.comment, 200);
        }

        public override void DrawCurve(Node inputNode) { }
    }
}