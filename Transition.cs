using System;

namespace Brix.State {
    [Serializable]
    public class Transition {
        public Condition condition;
        public bool disable;
        public int id;
        public State targetState;
    }
}