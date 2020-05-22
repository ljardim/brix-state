using UnityEditor;
using UnityEngine;

namespace Brix.State.Editor {
    public class BrixEditor : EditorWindow {
        private void Update() {
            if (settings.currentGraph != null) {
                var nodesWereDeleted = settings.currentGraph.DeleteNodesMarkedForDelete();
                if (nodesWereDeleted) {
                    Repaint();
                }
            }

            if (_currentStateManager == null || _previousState == _currentStateManager.currentState) {
                return;
            }

            Repaint();
            _previousState = _currentStateManager.currentState;
        }

        #region Helper Methods

        public static void DrawNodeCurve(Rect start, Rect end, bool left, Color curveColor) {
            var startPos = new Vector3(left ? start.x + start.width : start.x, start.y + start.height * .5f, 0);
            var endPos = new Vector3(end.x + end.width * .5f, end.y + end.height * .5f, 0);
            var startTan = startPos + Vector3.right * 50;
            var endTan = endPos + Vector3.left * 50;

            var shadow = new Color(0, 0, 0, 1);
            for (var i = 0; i < 1; i++) {
                Handles.DrawBezier(startPos, endPos, startTan, endTan, shadow, null, 4);
            }

            Handles.DrawBezier(startPos, endPos, startTan, endTan, curveColor, null, 3);
        }

        #endregion

        #region Variables

        public static EditorSettings settings;
        public static bool forceSetDirty;

        private static StateManager _currentStateManager;
        private Vector3 _mousePosition;
        private bool _clickedOnWindow;
        private Node _selectedNode;
        private int _transitionFromNodeId;
        private Rect _mouseRect = new Rect(0, 0, 1, 1);
        private readonly Rect _all = new Rect(-5, -5, 10000, 10000);
        private GUIStyle _style;
        private GUIStyle _activeStyle;
        private Vector2 _scrollPos;
        private Vector2 _scrollStartPos;
        private static BrixEditor _editor;
        private static StateManager _prevStateManager;
        private static State _previousState;

        private enum UserActions {
            AddState,
            AddTransitionNode,
            DeleteNode,
            CommentNode,
            MakeTransition,
            MakePortal,
            ResetPan
        }

        private const int MOUSE_BUTTON_LEFT = 0;
        private const int MOUSE_BUTTON_RIGHT = 1;
        private const int MOUSE_BUTTON_MIDDLE = 2;

        #endregion

        #region Init

        [MenuItem("Brix/StateEditor")]
        private static void ShowEditor() {
            _editor = GetWindow<BrixEditor>();
            _editor.minSize = new Vector2(800, 600);
        }

        private void OnEnable() {
            settings = Resources.Load("EditorSettings") as EditorSettings;
            if (settings == null) {
                Debug.Log("Editor settings was not found. Creating default ones in Brix/Resources");
                
                AssetDatabase.CreateFolder("Assets", "Brix");
                AssetDatabase.CreateFolder("Assets/Brix", "Resources");
                AssetDatabase.CreateFolder("Assets/Brix/Resources", "Nodes");
            
                var editorSkin = CreateInstance<GUISkin>();
                AssetDatabase.CreateAsset(editorSkin, "Assets/Brix/Resources/EditorSkin.guiskin");
                
                var activeStateSkin = CreateInstance<GUISkin>();
                AssetDatabase.CreateAsset(activeStateSkin, "Assets/Brix/Resources/ActiveStateSkin.guiskin");

                var commentNode = CreateInstance<CommentNode>();
                AssetDatabase.CreateAsset(commentNode, "Assets/Brix/Resources/Nodes/CommentNode.asset");
                
                var portalNode = CreateInstance<PortalNode>();
                AssetDatabase.CreateAsset(portalNode, "Assets/Brix/Resources/Nodes/PortalNode.asset");
                
                var transitionNode = CreateInstance<TransitionNode>();
                AssetDatabase.CreateAsset(transitionNode, "Assets/Brix/Resources/Nodes/TransitionNode.asset");
                
                var stateNode = CreateInstance<StateNode>();
                AssetDatabase.CreateAsset(stateNode, "Assets/Brix/Resources/Nodes/StateNode.asset");

                var defaultGraph = CreateInstance<StateGraph>();
                AssetDatabase.CreateAsset(defaultGraph, "Assets/Brix/Resources/DefaultGraph.asset");
                
                settings = CreateInstance<EditorSettings>();
                settings.skin = editorSkin;
                settings.activeSkin = activeStateSkin;
                settings.commentNode = commentNode;
                settings.portalNode = portalNode;
                settings.transitionNode = transitionNode;
                settings.stateNode = stateNode;
                settings.currentGraph = defaultGraph;
                AssetDatabase.CreateAsset(settings, "Assets/Brix/Resources/EditorSettings.asset");
            }

            settings = Resources.Load("EditorSettings") as EditorSettings;
            _style = settings.skin.GetStyle("window");
            _activeStyle = settings.activeSkin.GetStyle("window");
        }

        #endregion

        #region GUI Methods

        private void OnGUI() {
            if (Selection.activeTransform != null) {
                _currentStateManager = Selection.activeTransform.GetComponentInChildren<StateManager>();
                if (_prevStateManager != _currentStateManager) {
                    _prevStateManager = _currentStateManager;
                    Repaint();
                }
            }

            var currentEvent = Event.current;
            _mousePosition = currentEvent.mousePosition;
            HandleUserInput(currentEvent);

            DrawNodes();

            if (settings.currentGraph == null) {
                return;
            }

            if (currentEvent.type == EventType.MouseDrag || GUI.changed) {
                Repaint();
            }

            if (settings.makeTransition) {
                _mouseRect.x = _mousePosition.x;
                _mouseRect.y = _mousePosition.y;
                var fromNode = settings.currentGraph.GetNodeById(_transitionFromNodeId).windowRect;
                DrawNodeCurve(fromNode, _mouseRect, true, Color.blue);
                Repaint();
            }

            if (!forceSetDirty) {
                return;
            }

            forceSetDirty = false;
            EditorUtility.SetDirty(settings);
            EditorUtility.SetDirty(settings.currentGraph);

            foreach (var node in settings.currentGraph.nodes) {
                if (node.stateRef.currentState != null) {
                    EditorUtility.SetDirty(node.stateRef.currentState);
                }
            }
        }

        private void DrawNodes() {
            GUILayout.BeginArea(_all, _style);
            BeginWindows();

            EditorGUILayout.LabelField(" ", GUILayout.Width(100));
            EditorGUILayout.LabelField("Assign Graph:", GUILayout.Width(100));
            settings.currentGraph = (StateGraph) EditorGUILayout.ObjectField(settings.currentGraph, typeof(StateGraph),
                false, GUILayout.Width(200));

            if (settings.currentGraph != null) {
                foreach (var node in settings.currentGraph.nodes) {
                    node.DrawCurve();
                }

                for (var i = 0; i < settings.currentGraph.nodes.Count; i++) {
                    var node = settings.currentGraph.nodes[i];

                    if (node.drawNode is StateNode) {
                        if (_currentStateManager != null &&
                            node.stateRef.currentState == _currentStateManager.currentState) {
                            node.windowRect = GUI.Window(i, node.windowRect, DrawNodeWindow, node.windowTitle,
                                _activeStyle);
                        } else {
                            node.windowRect = GUI.Window(i, node.windowRect, DrawNodeWindow, node.windowTitle);
                        }
                    } else {
                        node.windowRect = GUI.Window(i, node.windowRect, DrawNodeWindow, node.windowTitle);
                    }
                }
            }

            EndWindows();
            GUILayout.EndArea();
        }

        private static void DrawNodeWindow(int id) {
            settings.currentGraph.nodes[id].DrawWindow();
            GUI.DragWindow();
        }

        private void HandleUserInput(Event inputEvent) {
            if (settings.currentGraph == null) {
                return;
            }

            if (settings.makeTransition) {
                if (inputEvent.button == MOUSE_BUTTON_LEFT && inputEvent.type == EventType.MouseDown) {
                    MakeTransition();
                }
            } else {
                if (inputEvent.button == MOUSE_BUTTON_RIGHT && inputEvent.type == EventType.MouseDown) {
                    _clickedOnWindow = false;
                    foreach (var node in settings.currentGraph.nodes) {
                        if (!node.windowRect.Contains(inputEvent.mousePosition)) {
                            continue;
                        }

                        _clickedOnWindow = true;
                        _selectedNode = node;
                        break;
                    }

                    if (_clickedOnWindow) {
                        ShowModifyNodeMenu(inputEvent);
                    } else {
                        ShowAddNodeMenu(inputEvent);
                    }
                }
            }

            if (inputEvent.button == MOUSE_BUTTON_MIDDLE) {
                switch (inputEvent.type) {
                    case EventType.MouseDown:
                        _scrollStartPos = inputEvent.mousePosition;
                        break;
                    case EventType.MouseDrag:
                        HandlePanning(inputEvent);
                        break;
                    case EventType.MouseUp:
                        break;
                }
            }
        }

        private void HandlePanning(Event inputEvent) {
            var diff = inputEvent.mousePosition - _scrollStartPos;
            diff *= .6f;
            _scrollStartPos = inputEvent.mousePosition;
            _scrollPos += diff;

            foreach (var node in settings.currentGraph.nodes) {
                node.windowRect.x += diff.x;
                node.windowRect.y += diff.y;
            }
        }

        private void ResetScroll() {
            foreach (var node in settings.currentGraph.nodes) {
                node.windowRect.x -= _scrollPos.x;
                node.windowRect.y -= _scrollPos.y;
            }

            _scrollPos = Vector2.zero;
        }

        private void MakeTransition() {
            settings.makeTransition = false;
            _clickedOnWindow = false;
            foreach (var node in settings.currentGraph.nodes) {
                if (!node.windowRect.Contains(_mousePosition)) {
                    continue;
                }

                _clickedOnWindow = true;
                _selectedNode = node;
                break;
            }

            if (!_clickedOnWindow) {
                return;
            }

            if (!(_selectedNode.drawNode is StateNode) && !(_selectedNode.drawNode is PortalNode)) {
                return;
            }

            if (_selectedNode.id == _transitionFromNodeId) {
                return;
            }

            var fromNode = settings.currentGraph.GetNodeById(_transitionFromNodeId);
            fromNode.transitionTargetNode = _selectedNode.id;

            var toNode = settings.currentGraph.GetNodeById(fromNode.transitionInputNode);
            var transition = toNode.stateRef.currentState.GetTransition(fromNode.transRef.transitionId);

            transition.targetState = _selectedNode.stateRef.currentState;
        }

        #endregion

        #region Context Menus

        private void ShowAddNodeMenu(Event inputEvent) {
            var menu = new GenericMenu();
            menu.AddSeparator("");
            if (settings.currentGraph == null) {
                menu.AddDisabledItem(new GUIContent("Add State"));
                menu.AddDisabledItem(new GUIContent("Add Comment"));
            } else {
                menu.AddItem(new GUIContent("Add State"), false, OnMenuItemClicked, UserActions.AddState);
                menu.AddItem(new GUIContent("Add Portal"), false, OnMenuItemClicked, UserActions.MakePortal);
                menu.AddItem(new GUIContent("Add Comment"), false, OnMenuItemClicked, UserActions.CommentNode);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Reset Panning"), false, OnMenuItemClicked, UserActions.ResetPan);
            }

            menu.ShowAsContext();
            inputEvent.Use();
        }

        private void ShowModifyNodeMenu(Event inputEvent) {
            var menu = new GenericMenu();

            if (_selectedNode.drawNode is StateNode) {
                if (_selectedNode.stateRef.currentState == null) {
                    menu.AddSeparator("");
                    menu.AddDisabledItem(new GUIContent("Add Transition Condition"));
                } else {
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Add Transition Condition"), false, OnMenuItemClicked,
                        UserActions.AddTransitionNode);
                }

                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Delete"), false, OnMenuItemClicked, UserActions.DeleteNode);
            }

            if (_selectedNode.drawNode is PortalNode) {
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Delete"), false, OnMenuItemClicked, UserActions.DeleteNode);
            }

            if (_selectedNode.drawNode is TransitionNode) {
                if (_selectedNode.isDuplicate || !_selectedNode.isAssigned) {
                    menu.AddSeparator("");
                    menu.AddDisabledItem(new GUIContent("On True Transition To"));
                } else {
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("On True Transition To"), false, OnMenuItemClicked,
                        UserActions.MakeTransition);
                }

                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Delete"), false, OnMenuItemClicked, UserActions.DeleteNode);
            }

            if (_selectedNode.drawNode is CommentNode) {
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Delete"), false, OnMenuItemClicked, UserActions.DeleteNode);
            }

            menu.ShowAsContext();
            inputEvent.Use();
        }

        private void OnMenuItemClicked(object input) {
            var action = (UserActions) input;
            switch (action) {
                case UserActions.AddState:
                    settings.AddNodeOnGraph(settings.stateNode, 200, 100, "State", _mousePosition);
                    break;
                case UserActions.MakePortal:
                    settings.AddNodeOnGraph(settings.portalNode, 100, 80, "Portal", _mousePosition);
                    break;
                case UserActions.AddTransitionNode:
                    AddTransitionNode(_selectedNode, _mousePosition);
                    break;
                case UserActions.CommentNode:
                    settings.AddNodeOnGraph(settings.commentNode, 200, 100, "Comment", _mousePosition);
                    break;
                case UserActions.DeleteNode:
                    if (_selectedNode.drawNode is TransitionNode) {
                        var enterNode = settings.currentGraph.GetNodeById(_selectedNode.transitionInputNode);
                        enterNode?.stateRef.currentState.RemoveTransition(_selectedNode.transRef.transitionId);
                    }

                    settings.currentGraph.MarkNodeForDeletion(_selectedNode.id);
                    break;
                case UserActions.MakeTransition:
                    _transitionFromNodeId = _selectedNode.id;
                    settings.makeTransition = true;
                    break;
                case UserActions.ResetPan:
                    ResetScroll();
                    break;
            }

            forceSetDirty = true;
        }

        private static void AddTransitionNode(Node fromNode, Vector3 pos) {
            var transition = StateNode.AddTransition(fromNode);
            AddTransitionNodeFromTransition(transition, fromNode, pos);
        }

        public static void AddTransitionNodeFromTransition(Transition transition, Node fromNode, Vector3 pos) {
            settings.AddNodeOnGraph(settings.transitionNode, 200, 100, "Condition", pos, fromNode.id, transition.id);
        }

        #endregion
    }
}