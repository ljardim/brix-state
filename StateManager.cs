using UnityEngine;

namespace Brix.State {
    public class StateManager : MonoBehaviour {
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

        protected virtual void Init() { }

        protected virtual void Tick() { }

        protected virtual void FixedTick() { }
    }
}