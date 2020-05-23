using UnityEngine;

namespace Brix.State {
    public abstract class StateManager : MonoBehaviour {
        public State currentState;
        [HideInInspector]public float delta;

        private void Start() {
            if (currentState != null) {
                currentState.OnEnter();
            }
            Init();
        }

        private void Update() {
            delta = Time.deltaTime;
            if (currentState != null) {
                currentState.Tick(this);
            }
            Tick();
        }

        private void FixedUpdate() {
            delta = Time.fixedDeltaTime;
            if (currentState != null) {
                currentState.FixedTick(this);
            }
            FixedTick();
        }

        protected abstract void Init();
        protected abstract void Tick();
        protected abstract void FixedTick();
    }
}