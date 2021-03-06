using System.Collections.Generic;
using UnityAtoms;
using UnityEngine;

namespace Brix.State {
    [CreateAssetMenu(menuName = Constants.Menus.STATE + "New State")]
    public class State : ScriptableObject {
        public int idCount;
        public AtomAction[] onEnter;
        public AtomAction[] onExit;
        public AtomAction[] onFixed;
        public AtomAction[] onUpdate;
        [SerializeField]public List<Transition> transitions = new List<Transition>();

        public void FixedTick(StateManager stateManager) {
            ExecuteActions(onFixed);
            CheckTransitions(stateManager);
        }

        public void Tick(StateManager stateManager) {
            ExecuteActions(onUpdate);
            CheckTransitions(stateManager);
        }

        public void OnEnter() {
            ExecuteActions(onEnter);
        }

        public void OnExit() {
            ExecuteActions(onExit);
        }

        private void CheckTransitions(StateManager stateManager) {
            foreach (var transition in transitions) {
                if (transition.disable) {
                    continue;
                }

                if (!transition.condition.CheckCondition()) {
                    continue;
                }

                if (transition.targetState == null) {
                    return;
                }

                stateManager.currentState = transition.targetState;
                OnExit();
                stateManager.currentState.OnEnter();
                return;
            }
        }

        private static void ExecuteActions(IEnumerable<AtomAction> actions) {
            foreach (var action in actions) {
                if (action != null) {
                    action.Do();
                }
            }
        }

        public Transition AddTransition() {
            var retVal = new Transition();
            transitions.Add(retVal);
            retVal.id = idCount;
            idCount++;
            return retVal;
        }

        public Transition GetTransition(int id) {
            foreach (var transition in transitions) {
                if (transition.id == id) {
                    return transition;
                }
            }

            return null;
        }

        public void RemoveTransition(int id) {
            var transition = GetTransition(id);
            if (transition != null) {
                transitions.Remove(transition);
            }
        }
    }
}