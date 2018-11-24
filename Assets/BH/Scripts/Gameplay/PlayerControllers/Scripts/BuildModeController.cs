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
    public class BuildModeController : TakesInput
    {
        // Input state
        bool _selectDown = false;
        bool _selectUp = false;
        bool _pickupDown = false;
        bool _pickupUp = false;
        bool _spawnSelectableDown = false;
        bool _spawnSelectableUp = false;
        bool _upOrSide = false;
        bool changeAxis = false;
        bool _undoDown = false;
        float _scrollWheel = 0f;

        Camera _cam;
        float _distance = float.MaxValue;
        [SerializeField] LayerMask _selectableMask;
        
        // For selection functionality
        List<Selectable> _selected = new List<Selectable>();
        [SerializeField] SelectionRectController _selectionRectController;

        // For pickup functionality
        [SerializeField] Vector3 _pickUpOffset = Vector3.up;
        Vector3 _offset;
        Vector3 _offsetBase;
        Selectable _pickedUp;
        ClosestColliderBelow _closestColliderBelow = null;
        [SerializeField] AnimationCurve _velocityCurve;
        [SerializeField] float _maxVelocityDistance = 10f;
        [SerializeField] float _maxVelocity = 50f;
        [SerializeField] float _bufferDistance = 2f;

        [SerializeField] LayerMask _selectableSurfaceMask;
        [SerializeField] LayerMask _spawnableSurfaceMask;

        // Store actions to allow undos
        Stack<ActionClass> actions = new Stack<ActionClass>();
        
        // Variables needed for detecting "new" scrolls.
        [SerializeField] float _scrollRefreshPeriod = .5f;
        bool _saveOnScroll = true;
        Coroutine _resetSaveOnScrollCoroutine;

        // Needed to detect objects below a held object.
        DetectColliderBelow _detectColliderBelow;

        // Needed to show a "preview" before spawning a selectable.
        bool _spawningSelectable = false;
        [SerializeField] GhostSelectable _ghostSelectablePrefab;
        GhostSelectable _ghostSelectable;
        Vector3 _theMiddleOfNowhere = Vector3.down * 1000f;
        bool _ghostInTheMiddleOfNowhereLastFrame = true;
        
        // Rotation for ghost / spawned selectable.
        Quaternion _spawnRotation = Quaternion.identity;
        [SerializeField] float _rotationPerTick = 10f;

        void GetInput()
        {
            if (_locks.Count > 0)
            {
                _selectDown = false;
                _selectUp = true;
                _pickupDown = false;
                _pickupUp = true;
                _spawnSelectableDown = false;
                _spawnSelectableUp = true;
                _upOrSide = false;
                _undoDown = false;
                _scrollWheel = 0f;

                // Unselect everything upon input lock.
                foreach (Selectable selectable in _selected)
                    selectable.Deselect();
                _selected.RemoveAll(selected => true);

                _spawningSelectable = false;

                return;
            }

            _selectDown = InputManager.GetKeyDown("Attack2") && !EventSystem.current.IsPointerOverGameObject();
            _selectUp = InputManager.GetKeyUp("Attack2");
            _pickupDown = InputManager.GetKeyDown("Attack1") && !EventSystem.current.IsPointerOverGameObject();
            _pickupUp = InputManager.GetKeyUp("Attack1");
            _spawnSelectableDown = InputManager.GetKeyDown("Attack1") && !EventSystem.current.IsPointerOverGameObject();
            _spawnSelectableUp = InputManager.GetKeyUp("Attack1");
            _scrollWheel = Input.GetAxisRaw("Mouse ScrollWheel") * 10f;
            _undoDown = InputManager.GetKeyDown("Undo");
            _upOrSide = InputManager.GetKeyDown("Toggle Rotation Axis");
        }

        void Awake()
        {
            if (!_cam)
            {
                _cam = Camera.main;
                if (!_cam)
                    Debug.LogError("Camera is not initialized.");
            }

            if (!_detectColliderBelow)
            {
                _detectColliderBelow = GetComponentInChildren<DetectColliderBelow>();
                if (!_detectColliderBelow)
                    Debug.LogError("Detect Collider Below reference is not initialized.");
            }

            if (!_ghostSelectablePrefab)
            {
                Debug.LogError("Ghost Selectable prefab is not initialized.");
            }
            else
            {
                _ghostSelectable = Instantiate(_ghostSelectablePrefab, _theMiddleOfNowhere, Quaternion.identity);
            }
        }

        void Update()
        {
            GetInput();

            // Undo last action
            if (_undoDown && actions.Count > 0)
            {
                ActionClass lastAction = actions.Pop();
                lastAction.Undo();

                // If the player undoes an action, it's clear they've stopped scrolling! Reset _saveOnScroll.
                _saveOnScroll = true;

                // Stop any running timers that will reset _saveOnScroll needlessly.
                if (_resetSaveOnScrollCoroutine != null)
                {
                    StopCoroutine(_resetSaveOnScrollCoroutine);
                    _resetSaveOnScrollCoroutine = null;
                }
            }

            // Pickup release
            if (_pickedUp && _pickupUp)
            {
                if (_pickedUp._rigidbody.velocity.y > 0f)
                    _pickedUp._rigidbody.velocity = new Vector3(_pickedUp._rigidbody.velocity.x, 0f, _pickedUp._rigidbody.velocity.z);
                _pickedUp._rigidbody.useGravity = true;
                _pickedUp = null;
                _offset = Vector3.zero;

                //if (_closestColliderBelow)
                //{
                //    _closestColliderBelow.enabled = false;
                //    _closestColliderBelow = null;
                //}

                if (_detectColliderBelow.enabled)
                    _detectColliderBelow.SetInactive();
            }

            if(_upOrSide)
            {
                changeAxis = !changeAxis;
            }
            
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            // If some Selectable is picked up, move its position accordingly.
            if (_pickedUp)
            {
                CalculateOffsetBase(ray, _distance, _selectableSurfaceMask, _offsetBase, out _offsetBase);

                //float closestColliderY = float.MinValue;
                //if (_closestColliderBelow && _closestColliderBelow._closestTransform)
                //    closestColliderY = _closestColliderBelow._closestTransform.position.y;

                float closestColliderY = float.MinValue;
                if (_detectColliderBelow.enabled && _detectColliderBelow.GetClosestTransform())
                    closestColliderY = _detectColliderBelow.GetClosestTransform().position.y;

                Vector3 desiredPos = _offsetBase + _offset;
                desiredPos.y = Mathf.Max(desiredPos.y, closestColliderY + _bufferDistance);
                Vector3 diff = desiredPos - _pickedUp._rigidbody.position;
                _pickedUp._rigidbody.velocity = diff.normalized * _velocityCurve.Evaluate(diff.magnitude / _maxVelocityDistance) * _maxVelocity;
            }
            else if (HandleRectSelection()) { }
            else if (Physics.Raycast(ray, out hitInfo, _distance, _selectableMask))
            {
                // If nothing is picked up, raycast to see if you can pick up or select a Selectable.
                // Note: if you've already picked something up, it doesn't make sense to have the ability
                //     up another Selectable (same with selecting).

                // Get a reference to the Selectable that you hit with the raycast.
                Selectable sel = hitInfo.collider.GetComponentInChildren<Selectable>();
                if (sel)
                {
                    // Check if the player pressed the "pick up" input.
                    if (_pickupDown && sel._canBePickedUp)
                    {
                        // Save a reference to the picked-up selectable.
                        _pickedUp = sel;

                        // Save old transforms so we can undo this pick-up later.
                        SaveOldTransformsActionOf(new List<Selectable>(new Selectable[] { _pickedUp }));

                        // Unfreeze the position for movement. Set gravity off.
                        _pickedUp.UnfreezePosition();
                        _pickedUp._rigidbody.useGravity = false;

                        // Set offset base's value, offset's value.
                        CalculateOffsetBase(ray, _distance, _selectableSurfaceMask, _pickedUp._rigidbody.position, out _offsetBase);
                        _offset = _pickedUp._rigidbody.position - _offsetBase + _pickUpOffset;

                        //_closestColliderBelow = hitInfo.collider.GetComponent<ClosestColliderBelow>();
                        //if (_closestColliderBelow)
                        //    _closestColliderBelow.enabled = true;

                        MeshFilter pickedUpMeshFilter = hitInfo.collider.GetComponentInChildren<MeshFilter>();
                        if (pickedUpMeshFilter)
                            _detectColliderBelow.SetActiveMeshFilter(pickedUpMeshFilter, 1.2f, 2f);

                        //Debug.Log("Picked up " + hitInfo.collider.name + ". With offset " + _offset);
                    }
                    else if (_selectDown) // Else check if the player pressed the "select" input.
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
                }
            }
            else if (_spawningSelectable && _spawnSelectableDown && _locks.Count <= 0 && !EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out hitInfo, _distance, _spawnableSurfaceMask))
            {
                // Spawn a selectable if the player requests.
                Selectable newSel = SelectableManager.Instance.SpawnSelectable(hitInfo.point, _spawnRotation);
                SaveAddActionOf(new List<Selectable>(new Selectable[] { newSel }));
            }
            else if (_selectDown && !Physics.Raycast(ray, out hitInfo, _distance, _selectableMask))
            {
                // Clicked on an area without any dominoes => deselect all!
                DeselectAll();
            }

            HandleRotation();
            
            // Ghost preview during spawning of the selectable.
            if (_spawningSelectable)
            {
                Vector3 newGhostPosition = _theMiddleOfNowhere;

                if (!_pickedUp && _locks.Count <= 0 && !EventSystem.current.IsPointerOverGameObject()
                    && !Physics.Raycast(ray, out hitInfo, _distance, _selectableMask)
                    && Physics.Raycast(ray, out hitInfo, _distance, _spawnableSurfaceMask))
                {
                    newGhostPosition = hitInfo.point;
                }

                if (_scrollWheel != 0f)
                {
                    _spawnRotation = Quaternion.Euler(_spawnRotation.eulerAngles.x,
                        _spawnRotation.eulerAngles.y + _rotationPerTick * _scrollWheel,
                        _spawnRotation.eulerAngles.z);

                    _ghostSelectable.transform.rotation = _spawnRotation;
                }

                // If ghost just "spawned", play the fade-in animation.
                bool ghostSpawnedThisFrame = _ghostInTheMiddleOfNowhereLastFrame && newGhostPosition != _theMiddleOfNowhere;
                if (ghostSpawnedThisFrame)
                    _ghostSelectable.AnimateFadeIn();

                _ghostSelectable.transform.position = newGhostPosition;
                _ghostInTheMiddleOfNowhereLastFrame = _ghostSelectable.transform.position == _theMiddleOfNowhere;
            }
            else
            {
                _ghostSelectable.transform.position = _theMiddleOfNowhere;
                _ghostInTheMiddleOfNowhereLastFrame = true;
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
                    // Todo: i think we should change this to always select units, that's more intuitive to me.
                    // Then we'd need another easy way to deselect all units at once.
                    // e.g. a quick right click on something other than a domino will deselect everything
                    if (unit.IsSelected())
                    {
                        Deselect(unit);
                    }
                    else
                    {
                        Select(unit);
                    }
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
        
        ///// <summary>
        ///// Rotates the selected game objects at a constant speed.
        ///// </summary>
        //public void RotateSelected()
        //{
        //    Vector3 center = FindCenter(_selectedTransforms.ToArray());

        //    foreach (Selectable selectable in _selected)
        //    {
        //        selectable.RotateAround(center, Vector3.up, 30f * Time.deltaTime);
        //    }
        //}

        /// <summary>
        /// Rotates the selected game objects a specified amount.
        /// </summary>
        /// <param name="deg">The rotation in degrees.</param>
        public void RotateSelected(float deg)
        {
            // Freeze all positions.
            SelectableManager.Instance.FreezePosition();

            List<Transform> selectedTransforms = new List<Transform>();
            foreach (Selectable sel in _selected)
                selectedTransforms.Add(sel.transform);

            Vector3 center = FindCenter(selectedTransforms.ToArray());

            //Debug.Log(_upOrSide);

            //if (changeAxis)
            //{
            //    rotAxis = Vector3.left;
            //    //Debug.Log("Rotating left");
            //}
            //else
            //{
            //    rotAxis = Vector3.up;
            //    //Debug.Log("Rotating up");
            //}

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

            //Bounds bounds = new Bounds();
            //foreach (Transform tf in tfs)
            //{
            //    bounds.Encapsulate(tf.position);
            //}

            //return bounds.center;

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
            SaveOldColorsAction();
            foreach (Selectable selectable in _selected)
            {
                selectable.SetColor(color);
            }
        }

        /// <summary>
        /// Resets the color of the selected game objects.
        /// </summary>
        public void ResetColor()
        {
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

        /// <summary>
        /// Saves the target Selectables' transforms (i.e. positions, rotations) into action history.
        /// </summary>
        private void SaveOldTransformsActionOf(List<Selectable> targets)
        {
            // Freeze everything. Needed so that undo function is never broken.
            // Why?:
            //   We're currently only saving data for selected objects, so
            //   non-selected objects should be static. If they aren't static,
            //   we don't have the historical information about them to undo
            //   their changes and that's bad.
            // An alternative would be to save every object's transforms, but
            // that does not scale nearly as well as the current solution does.
            // It's a straightforward and surefire solution with performance tradeoffs.
            SelectableManager.Instance.FreezeRotation();
            SelectableManager.Instance.FreezePosition();

            List<Selectable> selectedObjs = new List<Selectable>();
            List<CustomTransform> oldTransforms = new List<CustomTransform>();
            foreach (Selectable sObj in targets)
            {
                selectedObjs.Add(sObj);
                oldTransforms.Add(new CustomTransform(sObj.transform));
            }
            TransformActionClass SavedTransformsAction = new TransformActionClass();
            SavedTransformsAction.Init(selectedObjs, oldTransforms);
            actions.Push(SavedTransformsAction);
        }

        /// <summary>
        /// Saves the currently selected game objects' colors into action history.
        /// </summary>
        private void SaveOldColorsAction()
        {
            List<Selectable> selectables = new List<Selectable>();
            List<Color> colors = new List<Color>();
            foreach (Selectable selectable in _selected)
            {
                selectables.Add(selectable);
                colors.Add(selectable.GetColor());
            }
            ColorActionClass SavedColorsAction = new ColorActionClass();
            SavedColorsAction.Init(selectables, colors);
            actions.Push(SavedColorsAction);
        }

        /// <summary>
        /// Saves the creation of the target Selectables into action history.
        /// </summary>
        private void SaveAddActionOf(List<Selectable> targets)
        {
            AddActionClass savedAddAction = new AddActionClass();
            savedAddAction.Init(targets);
            actions.Push(savedAddAction);
        }

        /// <summary>
        /// Saves the deletion of the target Selectables into action history.
        /// </summary>
        private void SaveDeleteActionOf(List<Selectable> targets)
        {
            DeleteActionClass savedDeleteAction = new DeleteActionClass();
            savedDeleteAction.Init(targets);
            actions.Push(savedDeleteAction);
        }

        /// <summary>
        /// Setter for BuildModeController's action history. 
        /// Allows action history to persist outside of BuildModeController's life.
        /// </summary>
        public void SetActions(Stack<ActionClass> actions)
        {
            this.actions = actions;
        }

        /// <summary>
        /// Getter for BuildModeController's action history.
        /// </summary>
        public Stack<ActionClass> GetActions()
        {
            return this.actions;
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
        /// Toggles "spawn selectable" mode.
        /// </summary>
        public void ToggleSpawnSelectable()
        {
            if (_spawningSelectable)
                _spawningSelectable = false;
            else
                _spawningSelectable = true;
        }
    }
}
