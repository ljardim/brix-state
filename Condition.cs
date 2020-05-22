using UnityEngine;

namespace Brix.State {
    public abstract class Condition : ScriptableObject {
        public string description;
        public abstract bool CheckCondition();
    }
}