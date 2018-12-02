using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BH
{
    /// <summary>
    /// Manages the controls in build mode.
    /// This class reads and responds to player inputs.
    /// Most notably, this class provides the logic for game object selection and pickup.
    /// </summary>
    /// <seealso cref="BH.TakesInput" />
    [RequireComponent(typeof(LineRenderer))]
    public class BuildModeController : TakesInput
    {
        // Input state
        bool _select = false;
        bool _selectDown = false;
        bool _selectUp = false;
        bool _pickupDown = false;
        bool _pickupUp = false;
        bool _spawnSelectableDown = false;
        bool _spawnSelectableUp = false;
        bool _upOrSide = false;
        bool changeAxis = false;
        bool _undoDown = false;
        bool _copyDown = false;
        bool _pasteDown = false;
        bool _dragDown = false;
        bool _dragUp = false;
        float _scrollWheel = 0f;

        float timeBetweenDominoes = 0.5f;
        float timer;

        Camera _cam;
        float _distance = float.MaxValue;
        [SerializeField] LayerMask _selectableMask;
        
        // For selection functionality
        List<Selectable> _selected = new List<Selectable>();
        [SerializeField] SelectionRectController _selectionRectController;

        // For copy functionality
        //List<SelectableInfo> _copied = new List<SelectableInfo>();
        List<SerializableSelectable> _copied = new List<SerializableSelectable>();

        // For paste functionality
        List<GhostSelectable> _ghostSelectablesToPaste = new List<GhostSelectable>();
        bool _spawningPastables = false;
        bool _ghostCenterInTheMiddleOfNowhereLastFrame = true;

        // For pickup functionality
        [SerializeField] Vector3 _pickUpOffset = Vector3.up;
        Vector3 _offsetBase;
        [SerializeField] AnimationCurve _velocityCurve;
        [SerializeField] float _maxVelocityDistance = 10f;
        [SerializeField] float _maxVelocity = 50f;
        [SerializeField] float _bufferDistance = 2f;

        List<PickedUpSelectable> _pickedUpSelectables = new List<PickedUpSelectable>();

        [SerializeField] LayerMask _selectableSurfaceMask;
        [SerializeField] LayerMask _spawnableSurfaceMask;
        
        // Variables needed for detecting "new" scrolls.
        [SerializeField] float _scrollRefreshPeriod = .5f;
        bool _saveOnScroll = true;
        Coroutine _resetSaveOnScrollCoroutine;

        ActionHistory actionHistory = new ActionHistory();

        //// Needed to detect objects below a held object.
        //DetectColliderBelow _detectColliderBelow;

        // Needed to show a "preview" before spawning a selectable.
        bool _spawningSelectable = false;
        [SerializeField] GhostSelectable _ghostSelectablePrefab;
        GhostSelectable _ghostSelectable;
        Vector3 _theMiddleOfNowhere = Vector3.down * 1000f;
        bool _ghostInTheMiddleOfNowhereLastFrame = true;
        
        // Rotation for ghost / spawned selectable.
        Quaternion _spawnRotation = Quaternion.identity;
        [SerializeField] float _rotationPerTick = 10f;

        // Drag-place dominoes
        LineRenderer _lineRenderer;
        Vector3 _lineRendererVertexOffset = Vector3.up * 0.1f;
        [SerializeField] float _minimumDistanceBetweenDragVertices = 0.1f;
        List<Vector3> _dragVertices = new List<Vector3>();
        bool _isDragging = false;

        void GetInput()
        {
            if (_locks.Count > 0)
            {
                _select = false;
                _selectDown = false;
                _selectUp = true;
                _pickupDown = false;
                _pickupUp = true;
                _spawnSelectableDown = false;
                _spawnSelectableUp = true;
                _upOrSide = false;
                _undoDown = false;
                _copyDown = false;
                _pasteDown = false;
                _dragDown = false;
                _dragUp = true;
                _scrollWheel = 0f;

                // Unselect everything upon input lock.
                foreach (Selectable selectable in _selected)
                    selectable.Deselect();
                _selected.RemoveAll(selected => true);

                return;
            }

            _select = InputManager.GetKey("Attack2");
            _selectDown = InputManager.GetKeyDown("Attack2") && !EventSystem.current.IsPointerOverGameObject();
            _selectUp = InputManager.GetKeyUp("Attack2");
            _pickupDown = InputManager.GetKeyDown("Attack1") && !EventSystem.current.IsPointerOverGameObject();
            _pickupUp = InputManager.GetKeyUp("Attack1");
            _spawnSelectableDown = InputManager.GetKeyDown("Attack1") && !EventSystem.current.IsPointerOverGameObject();
            _spawnSelectableUp = InputManager.GetKeyUp("Attack1");
            _scrollWheel = Input.GetAxisRaw("Mouse ScrollWheel") * 10f;
            _undoDown = InputManager.GetKeyDown("Undo");
            _upOrSide = InputManager.GetKeyDown("Toggle Rotation Axis");
            _copyDown = InputManager.GetKeyDown("Copy");
            _pasteDown = InputManager.GetKeyDown("Paste");
            _dragDown = InputManager.GetKeyDown("Drag");
            _dragUp = InputManager.GetKeyUp("Drag");
        }

        void Awake()
        {
            if (!_cam)
            {
                _cam = Camera.main;
                if (!_cam)
                    Debug.LogError("Camera is not initialized.");
            }

            //if (!_detectColliderBelow)
            //{
            //    _detectColliderBelow = GetComponentInChildren<DetectColliderBelow>();
            //    if (!_detectColliderBelow)
            //        Debug.LogError("Detect Collider Below reference is not initialized.");
            //}

            if (!_ghostSelectablePrefab)
            {
                Debug.LogError("Ghost Selectable prefab is not initialized.");
            }
            else
            {
                //_ghostSelectable = Instantiate(_ghostSelectablePrefab, _theMiddleOfNowhere, Quaternion.identity);
                _ghostSelectable = _ghostSelectablePrefab.Get<GhostSelectable>(null, _theMiddleOfNowhere, Quaternion.identity);
            }

            _lineRenderer = GetComponent<LineRenderer>();
        }

        void Update()
        {
            GetInput();
            
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            timer += Time.deltaTime;

            // Undo last action
            if (_undoDown)
            {
                bool undoSucceeded = actionHistory.AttemptUndo();

                if (undoSucceeded)
                {
                    DeselectAll();

                    // If the player undoes an action, it's clear they've stopped scrolling! Reset _saveOnScroll.
                    _saveOnScroll = true;

                    // Stop any running timers that will reset _saveOnScroll needlessly.
                    if (_resetSaveOnScrollCoroutine != null)
                    {
                        StopCoroutine(_resetSaveOnScrollCoroutine);
                        _resetSaveOnScrollCoroutine = null;
                    }
                }
            }

            // Copy
            if (_copyDown && _selected.Count > 0)
            {
                // Save info of selected dominos *persistently*.
                _copied.Clear();
                foreach (Selectable selectable in _selected)
                    _copied.Add(new SerializableSelectable(selectable));

                DeselectAll();
            }

            // Paste
            if (_spawningSelectable && _pasteDown && _copied.Count > 0 && Physics.Raycast(ray, out hitInfo, _distance, _selectableSurfaceMask))
            {
                _spawningPastables = true;

                // Purge old preview ghosts.
                ClearPastedSelectablesPreview();

                // Bring in the new preview ghosts.
                foreach (SerializableSelectable serializableSelectable in _copied)
                {
                    // Initially spawn them in their original positions.
                    GhostSelectable preview = _ghostSelectablePrefab.Get<GhostSelectable>();
                    preview.SetTransform(serializableSelectable._serializableTransform);
                    preview.SetColor(serializableSelectable._color);
                    _ghostSelectablesToPaste.Add(preview);
                }

                DeselectAll();
            }

            // Pickup release
            if (_pickedUpSelectables.Count > 0 && _pickupUp)
            {
                foreach (PickedUpSelectable pickedUpSelectable in _pickedUpSelectables)
                {
                    pickedUpSelectable._selectable._rigidbody.velocity = Vector3.zero;
                    pickedUpSelectable._selectable._rigidbody.useGravity = true;
                }

                _pickedUpSelectables.Clear();

                //if (_detectColliderBelow.enabled)
                //    _detectColliderBelow.SetInactive();
            }

            if (_upOrSide)
            {
                changeAxis = !changeAxis;
            }

            // If some Selectable is picked up, move its position accordingly.
            if (_pickedUpSelectables.Count > 0)
            {
                CalculateOffsetBase(ray, _distance, _selectableSurfaceMask, _offsetBase, out _offsetBase);

                //float closestColliderY = float.MinValue;
                //if (_detectColliderBelow.enabled && _detectColliderBelow.GetClosestTransform())
                //    closestColliderY = _detectColliderBelow.GetClosestTransform().position.y;

                foreach (PickedUpSelectable pickedUpSelectable in _pickedUpSelectables)
                {
                    Vector3 desiredPos = _offsetBase + pickedUpSelectable._offset;
                    //desiredPos.y = Mathf.Max(desiredPos.y, closestColliderY + _bufferDistance);
                    Vector3 diff = desiredPos - pickedUpSelectable._selectable._rigidbody.position;
                    pickedUpSelectable._selectable._rigidbody.velocity = diff.normalized * _velocityCurve.Evaluate(diff.magnitude / _maxVelocityDistance) * _maxVelocity;
                }
            }
            else if (_isDragging)
            {
                if (_dragUp)
                {
                    List<Vector3> vertices = new List<Vector3>(_dragVertices);

                    _isDragging = false;
                    _dragVertices.Clear();
                    SetLineRendererPositions(_dragVertices.ToArray());

                    // Guy released curve.
                    if (vertices.Count >= 4)
                    {
                        int fourth = Mathf.FloorToInt(vertices.Count / 3f);
                        int firstIndex = 0;
                        int secondIndex = firstIndex + fourth;
                        int thirdIndex = secondIndex + fourth;
                        int fourthIndex = vertices.Count - 1;

                        Vector3 firstPoint = vertices[firstIndex];
                        Vector3 secondPoint = vertices[secondIndex];
                        Vector3 thirdPoint = vertices[thirdIndex];
                        Vector3 fourthPoint = vertices[fourthIndex];

                        BezierCurve cubicBezier = new BezierCurve(firstPoint, secondPoint, thirdPoint, fourthPoint);

                        float estimatedLength = 0f;
                        int numSteps = 20; // Brute force arc length.
                        for (int i = 0; i < numSteps; i++)
                        {
                            estimatedLength += (cubicBezier.ValueAt((i + 1) / numSteps) - cubicBezier.ValueAt(i / numSteps)).magnitude;
                        }

                        float distanceBetweenDominoes = 1f;
                        int numDominoes = Mathf.FloorToInt(estimatedLength / distanceBetweenDominoes);

                        List<Selectable> newlyAddedSelectables = new List<Selectable>();
                        for (float i = 0f; i < estimatedLength; i += distanceBetweenDominoes)
                        {
                            float t = i / estimatedLength;
                            Vector3 spawnPoint = cubicBezier.ValueAt(t);
                            Vector3 spawnDirection = cubicBezier.TangentAt(t);
                            Debug.DrawRay(spawnPoint, spawnDirection, Color.white, 1f, false);

                            Quaternion spawnRotation = Quaternion.LookRotation(spawnDirection);

                            Selectable newlyAdded = SelectableManager.Instance.SpawnSelectable(spawnPoint, spawnRotation);
                            newlyAddedSelectables.Add(newlyAdded);
                        }

                        SaveAddActionOf(newlyAddedSelectables);
                    }

                }
                else if (Physics.Raycast(ray, out hitInfo, _distance, _selectableSurfaceMask) && ((hitInfo.point - _dragVertices.Last()).magnitude > _minimumDistanceBetweenDragVertices))
                {
                    _dragVertices.Add(hitInfo.point);

                    List<Vector3> bezierVertices = new List<Vector3>();
                    if (_dragVertices.Count >= 4)
                    {
                        int fourth = Mathf.FloorToInt(_dragVertices.Count / 3f);
                        int firstIndex = 0;
                        int secondIndex = firstIndex + fourth;
                        int thirdIndex = secondIndex + fourth;
                        int fourthIndex = _dragVertices.Count - 1;

                        Vector3 firstPoint = _dragVertices[firstIndex];
                        Vector3 secondPoint = _dragVertices[secondIndex];
                        Vector3 thirdPoint = _dragVertices[thirdIndex];
                        Vector3 fourthPoint = _dragVertices[fourthIndex];

                        BezierCurve cubicBezier = new BezierCurve(firstPoint, secondPoint, thirdPoint, fourthPoint);
                        
                        int numSteps = 20;
                        for (int i = 0; i <= numSteps; i++)
                        {
                            bezierVertices.Add(cubicBezier.ValueAt((float)i / numSteps));
                        }
                    }
                    
                    SetLineRendererPositions(bezierVertices.ToArray());
                }
            }
            else if (HandleRectSelection()) { }
            else if (_pickupDown && Physics.Raycast(ray, out hitInfo, _distance, _selectableMask))
            {
                // Get a reference to the Selectable that you hit with the raycast.
                Selectable selectable = hitInfo.collider.GetComponentInChildren<Selectable>();
                CalculateOffsetBase(ray, _distance, _selectableSurfaceMask, selectable._rigidbody.position, out _offsetBase);
                _pickedUpSelectables.Clear();

                if (_selected.Contains(selectable))
                {
                    foreach (Selectable sel in _selected)
                    {
                        if (sel._canBePickedUp)
                        {
                            Vector3 offset = sel._rigidbody.position - _offsetBase + _pickUpOffset;
                            PickedUpSelectable pickedUp = new PickedUpSelectable(sel, offset);
                            _pickedUpSelectables.Add(pickedUp);

                            pickedUp._selectable.UnfreezePosition();
                            pickedUp._selectable._rigidbody.useGravity = false;
                        }
                    }
                }
                else
                {
                    DeselectAll();

                    if (selectable._canBePickedUp)
                    {
                        Vector3 offset = selectable._rigidbody.position - _offsetBase + _pickUpOffset;
                        PickedUpSelectable pickedUp = new PickedUpSelectable(selectable, offset);
                        _pickedUpSelectables.Add(pickedUp);

                        pickedUp._selectable.UnfreezePosition();
                        pickedUp._selectable._rigidbody.useGravity = false;
                    }
                }

                List<Selectable> toSave = new List<Selectable>();
                foreach (PickedUpSelectable pickedUpSelectable in _pickedUpSelectables)
                {
                    toSave.Add(pickedUpSelectable._selectable);
                }
                SaveOldTransformsActionOf(toSave);
            }
            else if (_selectDown && Physics.Raycast(ray, out hitInfo, _distance, _selectableMask))
            {
                Selectable selectable = hitInfo.collider.GetComponentInChildren<Selectable>();
                if (selectable.IsSelected()) // Already selected? Then deselect it.
                {
                    Deselect(selectable);
                }
                else
                {
                    Select(selectable);
                }
            }
            else if (_spawningSelectable && _locks.Count <= 0 && _spawnSelectableDown && _locks.Count <= 0 && !EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out hitInfo, _distance, _spawnableSurfaceMask))
            {
                // "Spawning pastables" is a special case of "spawning selectable".
                if (_spawningPastables) // Player wants to spawn (possibly) multiple selectables.
                {
                    // Spawn the pasted dominos in _pastables
                    List<Selectable> newlySpawned = new List<Selectable>();
                    foreach (GhostSelectable ghostSelectable in _ghostSelectablesToPaste)
                    {
                        newlySpawned.Add(SelectableManager.Instance.SpawnSelectableFromGameObj(ghostSelectable));
                        //ghostSelectable.transform.position = _theMiddleOfNowhere;
                        ghostSelectable.Delete();
                    }
                    _spawningPastables = false;
                    //_justSpawnedPastables = false;
                    _ghostSelectablesToPaste.Clear();

                    SaveAddActionOf(newlySpawned);
                }
                else // Player wants to spawn a single selectable.
                {
                    // Spawn a selectable if the player requests.
                    Selectable newSel = SelectableManager.Instance.SpawnSelectable(hitInfo.point, _spawnRotation);
                    SaveAddActionOf(new List<Selectable>(new Selectable[] { newSel }));
                }
            }
            else if (_selectDown && !Physics.Raycast(ray, out hitInfo, _distance, _selectableMask))
            {
                // Clicked on an area without any dominoes => deselect all!
                DeselectAll();
            }
            else if (_dragDown && !_isDragging && !Physics.Raycast(ray, out hitInfo, _distance, _selectableMask) && Physics.Raycast(ray, out hitInfo, _distance, _selectableSurfaceMask))
            {
                _isDragging = true;
                _dragVertices.Add(hitInfo.point);
                SetLineRendererPositions(_dragVertices.ToArray());
            }

            if (_selected.Count > 0 && _pickedUpSelectables.Count <= 0)
                HandleRotation();

            // The following conditional handles ghost preview logic.
            if (_spawningSelectable && _locks.Count <= 0 && !_isDragging && !_select) // Basically hardcoded !_select to get rid of weird behaviour while mass-selecting & placing dominoes
            {
                // Either spawning a single selectable or a set of copied selectables.
                if (_spawningPastables)
                {
                    // No need for the normal ghost preview.
                    _ghostSelectable.transform.position = _theMiddleOfNowhere;
                    _ghostInTheMiddleOfNowhereLastFrame = true;

                    Vector3 newGhostCenter = _theMiddleOfNowhere;
                    Transform[] pastablesTransforms = _ghostSelectablesToPaste.Select(p => p.transform).ToArray();
                    Vector3 currentGhostCenter = FindCenter(pastablesTransforms);

                    if (_pickedUpSelectables.Count <= 0 && _locks.Count <= 0 && !EventSystem.current.IsPointerOverGameObject()
                        && !Physics.Raycast(ray, out hitInfo, _distance, _selectableMask)
                        && Physics.Raycast(ray, out hitInfo, _distance, _spawnableSurfaceMask))
                    {
                        newGhostCenter = hitInfo.point;

                        if (_scrollWheel != 0f)
                        {
                            float deg = _rotationPerTick * _scrollWheel;
                            RotateGameObjs(deg, _ghostSelectablesToPaste.Select(s => (GameObj)s).ToList());
                        }
                    }

                    // If ghosts just "spawned", play the fade-in animation.
                    bool ghostsSpawnedThisFrame = _ghostCenterInTheMiddleOfNowhereLastFrame && newGhostCenter != _theMiddleOfNowhere;
                    if (ghostsSpawnedThisFrame)
                    {
                        foreach (GhostSelectable ghostSelectable in _ghostSelectablesToPaste)
                            ghostSelectable.AnimateFadeIn();
                    }

                    // "Normalize" the positions of the ghosts around the new ghost center.
                    foreach (GhostSelectable ghostSelectable in _ghostSelectablesToPaste)
                        ghostSelectable.transform.position = ghostSelectable.transform.position - currentGhostCenter + newGhostCenter;

                    _ghostCenterInTheMiddleOfNowhereLastFrame = newGhostCenter == _theMiddleOfNowhere;
                }
                else
                {
                    Vector3 newGhostPosition = _theMiddleOfNowhere;

                    if (_pickedUpSelectables.Count <= 0 && _locks.Count <= 0 && !EventSystem.current.IsPointerOverGameObject()
                        && !Physics.Raycast(ray, out hitInfo, _distance, _selectableMask)
                        && Physics.Raycast(ray, out hitInfo, _distance, _spawnableSurfaceMask))
                    {
                        newGhostPosition = hitInfo.point;

                        if (_scrollWheel != 0f)
                        {
                            _spawnRotation = Quaternion.Euler(_spawnRotation.eulerAngles.x,
                                _spawnRotation.eulerAngles.y + _rotationPerTick * _scrollWheel,
                                _spawnRotation.eulerAngles.z);

                            _ghostSelectable.transform.rotation = _spawnRotation;
                        }
                    }

                    // If ghost just "spawned", play the fade-in animation.
                    bool ghostSpawnedThisFrame = _ghostInTheMiddleOfNowhereLastFrame && newGhostPosition != _theMiddleOfNowhere;
                    if (ghostSpawnedThisFrame)
                        _ghostSelectable.AnimateFadeIn();

                    _ghostSelectable.transform.position = newGhostPosition;
                    _ghostInTheMiddleOfNowhereLastFrame = _ghostSelectable.transform.position == _theMiddleOfNowhere;
                }
            }
            //else if (_spawningSelectable && _spawningPastables)
            //{
            //    if (!_ghostInTheMiddleOfNowhereLastFrame)
            //    {
            //        _ghostSelectable.transform.position = _theMiddleOfNowhere;
            //        _ghostInTheMiddleOfNowhereLastFrame = true;
            //    }

            //    Transform[] pastablesTransforms = _ghostSelectablesToPaste.Select(p => p.transform).ToArray();
            //    Vector3 currentGhostCenter = FindCenter(pastablesTransforms);

            //    // "Normalize" the positions of the ghosts around the new ghost center.
            //    foreach (GhostSelectable ghostSelectable in _ghostSelectablesToPaste)
            //        ghostSelectable.transform.position = ghostSelectable.transform.position - currentGhostCenter + _theMiddleOfNowhere;

            //    _ghostCenterInTheMiddleOfNowhereLastFrame = true;
            //}
            else // Not in the right mode to spawn selectables. Remove all ghost previews.
            {
                if (!_ghostInTheMiddleOfNowhereLastFrame)
                {
                    _ghostSelectable.transform.position = _theMiddleOfNowhere;
                    _ghostInTheMiddleOfNowhereLastFrame = true;
                }

                if (_ghostSelectablesToPaste.Count > 0)
                {
                    _spawningPastables = false;
                    ClearPastedSelectablesPreview();
                }
            }
        }

        /// <summary>
        /// Selects the specified target.
        /// </summary>
        /// <param name="target">The Selectable to be selected.</param>
        public void Select(Selectable target)
        {
            _selected.Add(target);
            target.Select();
        }

        /// <summary>
        /// Handles mass rectangular selection.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if at least one unit was selected in the rectangle.
        /// </returns>
        public bool HandleRectSelection()
        {
            List<Selectable> possibleSelectedUnits = _selectionRectController.AttemptMassSelection(
                SelectableManager.Instance.GetActiveSelectables(),
                Input.mousePosition,
                _selectDown,
                _selectUp
            );

            if (possibleSelectedUnits.Count > 0)
            {
                foreach (Selectable unit in possibleSelectedUnits)
                {
                    Select(unit);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Deselects the specified target.
        /// </summary>
        /// <param name="target">The Selectable to be deselected.</param>
        public void Deselect(Selectable target)
        {
            if (target.IsSelected())
            {
                _selected.Remove(target);
                target.Deselect();
            }
        }

        /// <summary>
        /// Deselects all currently selected selectables.
        /// </summary>
        public void DeselectAll()
        {
            foreach (Selectable selectable in _selected)
                selectable.Deselect();
            _selected.RemoveAll(selected => true);
        }

        /// <summary>
        /// Despawns the selected game objects.
        /// </summary>
        public void DespawnSelected()
        {
            SaveDeleteActionOf(_selected);
            
            foreach (Selectable selectable in _selected)
                SelectableManager.Instance.DespawnSelectable(selectable);

            _selected.RemoveAll(selected => true);
        }

        /// <summary>
        /// Rotates the given game objects a specified amount. 
        /// Only rotates around the objects' averaged centers.
        /// </summary>
        /// <param name="deg">The rotation in degrees.</param>
        public void RotateGameObjs(float deg, List<GameObj> targets)
        {
            Transform[] targetTransforms = targets.Select(t => t.transform).ToArray();
            Vector3 center = FindCenter(targetTransforms);
            foreach (GameObj target in targets)
            {
                target.RotateAround(center, Vector3.up, deg);
            }
        }

        /// <summary>
        /// Rotates the currently selected Selectables a specified amount. 
        /// Rotation can be w.r.t. the objects' individual centers, or their averaged center.
        /// </summary>
        /// <param name="deg">The rotation in degrees.</param>
        public void RotateSelected(float deg)
        {
            List<Transform> selectedTransforms = new List<Transform>();
            foreach (Selectable sel in _selected)
                selectedTransforms.Add(sel.transform);

            Vector3 center = FindCenter(selectedTransforms.ToArray());

            if (changeAxis)
            {
                foreach (Selectable selectable in _selected)
                {
                    selectable.RotateX(deg);
                }
            }
            else
            {
                foreach (Selectable selectable in _selected)
                {
                    selectable.RotateAround(center, Vector3.up, deg);
                }
            }
        }

        Vector3 FindCenter(Transform[] tfs)
        {
            if (tfs.Length <= 0)
                return Vector3.zero;

            if (tfs.Length == 1)
                return tfs[0].position;

            float xSum = 0f, ySum = 0f, zSum = 0f;

            foreach (Transform tf in tfs)
            {
                xSum += tf.position.x;
                ySum += tf.position.y;
                zSum += tf.position.z;
            }

            return new Vector3(xSum, ySum, zSum) / tfs.Length;
        }

        /// <summary>
        /// Changes the color of the selected game objects.
        /// </summary>
        /// <param name="color">The color.</param>
        public void ChangeColor(Color color)
        {
            SaveOldColorsActionOf(_selected);
            foreach (Selectable selectable in _selected)
            {
                selectable.SetColor(color);
            }
        }

        public void ChangeMaterial()
        {
            foreach (Selectable selectable in _selected)
            {
                selectable.SetMaterial();
            }
        }

        /// <summary>
        /// Resets the color of the selected game objects.
        /// </summary>
        public void ResetColor()
        {
            SaveOldColorsActionOf(_selected);
            foreach (Selectable selectable in _selected)
            {
                selectable.ResetColor();
            }
        }

        /// <summary>
        /// Handles rotation logic, including thresholds to detect when a new rotation begins.
        /// </summary>
        private void HandleRotation()
        {
            if (_scrollWheel != 0f)
            {
                if (_saveOnScroll)
                {
                    SaveOldTransformsActionOf(_selected);
                    _saveOnScroll = false;

                    // Set a timer to reset _saveOnScroll back to true. Save the reference to it so you can refresh it (if you need to).
                    _resetSaveOnScrollCoroutine = StartCoroutine(AsyncResetSaveOnScroll(_scrollRefreshPeriod));
                }
                else
                {
                    // If a timer is running, refresh it.
                    // Note: if you reach this section of code, there definitely should be a timer running.
                    if (_resetSaveOnScrollCoroutine != null)
                    {
                        StopCoroutine(_resetSaveOnScrollCoroutine);
                        _resetSaveOnScrollCoroutine = StartCoroutine(AsyncResetSaveOnScroll(_scrollRefreshPeriod));
                    }
                    else
                    {
                        Debug.LogError("_saveOnScroll is false, but a timer isn't running to reset it. How did this happen? wtf");
                    }
                }
                
                RotateSelected(_scrollWheel * 10f);
            }
        }

        // A coroutine that resets _saveOnScroll after a specified period of time.
        IEnumerator AsyncResetSaveOnScroll(float sec)
        {
            yield return new WaitForSeconds(sec);
            _saveOnScroll = true;
        }

        // Saves the target Selectables' transforms (i.e. positions, rotations) into action history.
        void SaveOldTransformsActionOf(List<Selectable> targets)
        {
            // Freeze non-target selectables. Needed so that undo function is never broken.
            // Why?:
            //   We're currently only saving data for target objects, so
            //   non-target objects should be static. If they aren't static,
            //   we don't have the historical information about them to undo
            //   their changes and that's bad.
            // An alternative would be to save every object's transforms, but
            // that does not scale nearly as well as the current solution does.
            // It's a straightforward and surefire solution with performance tradeoffs.
            foreach (Selectable selectable in SelectableManager.Instance.GetActiveSelectables())
            {
                if (!targets.Contains(selectable))
                {
                    selectable.FreezePosition();
                    selectable.FreezeRotation();
                }
            }

            Dictionary<Selectable, CustomTransform> transformActionState = new Dictionary<Selectable, CustomTransform>();
            foreach (Selectable sel in targets)
            {
                transformActionState[sel] = new CustomTransform(sel.transform);
            }
            actionHistory.PushTransformAction(transformActionState);
        }

        // Saves the target Selectables' colors into action history.
        void SaveOldColorsActionOf(List<Selectable> targets)
        {
            Dictionary<Selectable, Color> colorActionState = new Dictionary<Selectable, Color>();
            foreach (Selectable sel in targets)
            {
                colorActionState[sel] = sel.GetColor();
            }
            actionHistory.PushColorAction(colorActionState);
        }

        // Saves the creation of the target Selectables into action history.
        void SaveAddActionOf(List<Selectable> targets)
        {
            actionHistory.PushAddAction(targets);
        }

        // Saves the deletion of the target Selectables into action history.
        void SaveDeleteActionOf(List<Selectable> targets)
        {
            actionHistory.PushDeleteAction(targets);
        }

        /// <summary>
        /// Setter for BuildModeController's action history. 
        /// Allows action history to persist outside of BuildModeController's life.
        /// </summary>
        public void SetActionHistory(ActionHistory history)
        {
            this.actionHistory = history;
        }

        /// <summary>
        /// Getter for BuildModeController's action history.
        /// </summary>
        /// <returns>
        ///     The BuildModeController's ActionHistory instance.
        /// </returns>
        public ActionHistory GetActionHistory()
        {
            return this.actionHistory;
        }
        
        // Calculates an offset base with the following preferences:
        //   1) Set offsetBase to the hit point of the raycast to a selectable surface
        //   2) Set offsetBase to the hit point of the raycast to a horizontal plane that backupOffsetBase lies on.
        //   3) If 1 and 2 fail, set offsetBase to backupOffsetBase. (It is the backup offset base after all.)
        bool CalculateOffsetBase(Ray ray, float distance, LayerMask selectableSurfaceMask, Vector3 backupOffsetBase, out Vector3 offsetBase)
        {
            RaycastHit hitInfo;

            // Initially try to raycast to see if you can hit a selectable surface.
            if (Physics.Raycast(ray, out hitInfo, distance, selectableSurfaceMask))
            {
                offsetBase = hitInfo.point;
                return true;
            }
            else
            {
                // Create an "imaginary" selectable surface that is a horizontal plane going through the backup offset base.
                Plane plane = new Plane(Vector3.up, backupOffsetBase);

                float enter;
                if (plane.Raycast(ray, out enter))
                {
                    offsetBase = ray.GetPoint(enter);
                    return true;
                }
            }

            // Just use the backup offset base, if we can't calculate one.
            offsetBase = backupOffsetBase;
            return false;
        }

        /// <summary>
        /// Sets "spawn selectable" mode.
        /// </summary>
        public void SetSpawnSelectable(bool b)
        {
            _spawningSelectable = b;
        }

        void ClearPastedSelectablesPreview()
        {
            foreach (GhostSelectable gSel in _ghostSelectablesToPaste)
                gSel.Delete();

            _ghostSelectablesToPaste.Clear();
        }

        void SetLineRendererPositions(Vector3[] vertices)
        {
            Vector3[] offsetVertices = vertices.ToList().Select(v => v + _lineRendererVertexOffset).ToArray();
            _lineRenderer.positionCount = offsetVertices.Length;
            _lineRenderer.SetPositions(offsetVertices);
        }
    }

    class PickedUpSelectable
    {
        public PickedUpSelectable(Selectable selectable, Vector3 offset)
        {
            _selectable = selectable;
            _offset = offset;
        }

        public Selectable _selectable;
        public Vector3 _offset;
    }

    class SelectableInfo
    {
        public SerializableTransform transform;
        public Color color;
        public SelectableInfo(SerializableTransform transform, Color color)
        {
            this.transform = transform;
            this.color = color;
        }
    }
}
