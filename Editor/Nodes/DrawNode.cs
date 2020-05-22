using UnityEngine;

namespace Brix.State.Editor {
    public abstract class DrawNode : ScriptableObject {
        public abstract void DrawWindow(Node inputNode);
        public abstract void DrawCurve(Node inputNode);
    }
}