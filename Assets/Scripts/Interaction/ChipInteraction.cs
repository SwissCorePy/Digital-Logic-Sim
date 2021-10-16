using System;
using System.Collections.Generic;
using Chip;
using Graphics;
using UI;
using UnityEngine;

namespace Interaction
{
    public class ChipInteraction : InteractionHandler
    {
        private const float DragDepth = -50;
        private const float ChipDepth = -0.2f;

        public BoxCollider2D chipArea;
        public Transform chipHolder;
        public LayerMask chipMask;
        public Material selectionBoxMaterial;
        public float chipStackSpacing = 0.15f;
        public float selectionBoundsBorderPadding = 0.1f;
        public Color selectionBoxCol;
        public Color invalidPlacementCol;

        public EditChipMenu editChipMenu;

        private State _currentState;
        private List<Chip.Chip> _newChipsToPlace;
        private List<Chip.Chip> _selectedChips;
        private Vector3[] _selectedChipsOriginalPos;
        private Vector2 _selectionBoxStartPos;
        private Mesh _selectionMesh;

        public List<Chip.Chip> allChips { get; private set; }

        private void Awake()
        {
            _newChipsToPlace = new List<Chip.Chip>();
            _selectedChips = new List<Chip.Chip>();
            allChips = new List<Chip.Chip>();
            MeshShapeCreator.CreateQuadMesh(ref _selectionMesh);
            editChipMenu = GameObject.Find("Edit Chip Menu").GetComponent<EditChipMenu>();
            editChipMenu.Init();
        }

        public event Action<Chip.Chip> ONDeleteChip;

        public override void OrderedUpdate()
        {
            switch (_currentState)
            {
                case State.None:
                    HandleSelection();
                    HandleDeletion();
                    break;
                case State.PlacingNewChips:
                    HandleNewChipPlacement();
                    break;
                case State.SelectingChips:
                    HandleSelectionBox();
                    break;
                case State.MovingOldChips:
                    HandleChipMovement();
                    break;
            }

            DrawSelectedChipBounds();
        }

        public void LoadChip(Chip.Chip chip)
        {
            chip.transform.parent = chipHolder;
            allChips.Add(chip);
        }

        public void SpawnChip(Chip.Chip chipPrefab)
        {
            RequestFocus();
            if (HasFocus)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    // Spawn chip
                    _currentState = State.PlacingNewChips;
                    if (_newChipsToPlace.Count == 0) _selectedChips.Clear();
                    var newChip = Instantiate(chipPrefab, chipHolder);
                    newChip.gameObject.SetActive(true);
                    _selectedChips.Add(newChip);
                    _newChipsToPlace.Add(newChip);
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    // Open chip edit menu
                    editChipMenu.EditChip(chipPrefab);
                }
            }
        }

        private void HandleSelection()
        {
            var mousePos = InputHelper.MouseWorldPos;

            // Left mouse down. Handle selecting a chip, or starting to draw a selection box.
            if (Input.GetMouseButtonDown(0) && !InputHelper.MouseOverUIObject())
            {
                RequestFocus();
                if (HasFocus)
                {
                    _selectionBoxStartPos = mousePos;
                    var objectUnderMouse = InputHelper.GetObjectUnderMouse2D(chipMask);

                    // If clicked on nothing, clear selected items and start drawing selection box
                    if (objectUnderMouse == null)
                    {
                        _currentState = State.SelectingChips;
                        _selectedChips.Clear();
                    }
                    // If clicked on a chip, select that chip and allow it to be moved
                    else
                    {
                        _currentState = State.MovingOldChips;
                        var chipUnderMouse = objectUnderMouse.GetComponent<Chip.Chip>();
                        // If object is already selected, then selection of any other chips should be maintained so they can be moved as a group.
                        // But if object is not already selected, then any currently selected chips should be deselected.
                        if (!_selectedChips.Contains(chipUnderMouse))
                        {
                            _selectedChips.Clear();
                            _selectedChips.Add(chipUnderMouse);
                        }

                        // Record starting positions of all selected chips for movement
                        _selectedChipsOriginalPos = new Vector3[_selectedChips.Count];
                        for (var i = 0; i < _selectedChips.Count; i++)
                            _selectedChipsOriginalPos[i] = _selectedChips[i].transform.position;
                    }
                }
            }
        }

        private void HandleDeletion()
        {
            // Delete any selected chips
            if (InputHelper.AnyOfTheseKeysDown(KeyCode.Backspace, KeyCode.Delete) ||
                Input.GetMouseButton(2))
            {
                for (var i = _selectedChips.Count - 1; i >= 0; i--)
                {
                    DeleteChip(_selectedChips[i]);
                    _selectedChips.RemoveAt(i);
                }

                _newChipsToPlace.Clear();
            }
        }

        private void DeleteChip(Chip.Chip chip)
        {
            if (ONDeleteChip != null) ONDeleteChip.Invoke(chip);

            allChips.Remove(chip);
            Destroy(chip.gameObject);
        }

        private void HandleSelectionBox()
        {
            var mousePos = InputHelper.MouseWorldPos;
            // While holding mouse down, keep drawing selection box
            if (Input.GetMouseButton(0))
            {
                var pos = (Vector3)(_selectionBoxStartPos + mousePos) / 2 + Vector3.back * 0.5f;
                var scale = new Vector3(Mathf.Abs(mousePos.x - _selectionBoxStartPos.x),
                    Mathf.Abs(mousePos.y - _selectionBoxStartPos.y), 1);
                selectionBoxMaterial.color = selectionBoxCol;
                UnityEngine.Graphics.DrawMesh(_selectionMesh, Matrix4x4.TRS(pos, Quaternion.identity, scale),
                    selectionBoxMaterial, 0);
            }

            // Mouse released, so selected all chips inside the selection box
            if (!Input.GetMouseButtonUp(0)) return;

            _currentState = State.None;

            // Select all objects under selection box
            var boxSize = new Vector2(Mathf.Abs(mousePos.x - _selectionBoxStartPos.x),
                Mathf.Abs(mousePos.y - _selectionBoxStartPos.y));
            Collider2D[] allObjectsInBox = { };
            var unused = Physics2D.OverlapBoxNonAlloc((_selectionBoxStartPos + mousePos) / 2, boxSize, 0,
                allObjectsInBox, chipMask);
            _selectedChips.Clear();
            foreach (var item in allObjectsInBox)
                if (item.GetComponent<Chip.Chip>())
                    _selectedChips.Add(item.GetComponent<Chip.Chip>());
        }

        private void HandleChipMovement()
        {
            var mousePos = InputHelper.MouseWorldPos;

            if (Input.GetMouseButton(0))
            {
                // Move selected objects
                var deltaMouse = mousePos - _selectionBoxStartPos;
                for (var i = 0; i < _selectedChips.Count; i++)
                {
                    _selectedChips[i].transform.position = (Vector2)_selectedChipsOriginalPos[i] + deltaMouse;
                    SetDepth(_selectedChips[i], DragDepth + _selectedChipsOriginalPos[i].z);
                }
            }

            // Mouse released, so stop moving chips
            if (Input.GetMouseButtonUp(0))
            {
                _currentState = State.None;

                if (SelectedChipsWithinPlacementArea())
                {
                    const float chipMoveThreshold = 0.001f;
                    var deltaMouse = mousePos - _selectionBoxStartPos;

                    // If didn't end up moving the chips, then select just the one under the mouse
                    if (_selectedChips.Count > 1 && deltaMouse.magnitude < chipMoveThreshold)
                    {
                        var objectUnderMouse = InputHelper.GetObjectUnderMouse2D(chipMask);
                        if (!objectUnderMouse?.GetComponent<Chip.Chip>()) return;

                        _selectedChips.Clear();
                        if (objectUnderMouse is { }) _selectedChips.Add(objectUnderMouse.GetComponent<Chip.Chip>());
                    }
                    else
                    {
                        for (var i = 0; i < _selectedChips.Count; i++)
                            SetDepth(_selectedChips[i], _selectedChipsOriginalPos[i].z);
                    }
                }
                // If any chip ended up outside of placement area, then put all chips back to their original positions
                else
                {
                    for (var i = 0; i < _selectedChipsOriginalPos.Length; i++)
                        _selectedChips[i].transform.position = _selectedChipsOriginalPos[i];
                }
            }
        }

        // Handle placement of newly spawned chips
        private void HandleNewChipPlacement()
        {
            // Cancel placement if esc or right mouse down
            if (InputHelper.AnyOfTheseKeysDown(KeyCode.Escape, KeyCode.Backspace, KeyCode.Delete) ||
                Input.GetMouseButtonDown(1))
            {
                CancelPlacement();
            }
            // Move selected chip/s and place them on left mouse down
            else
            {
                var mousePos = InputHelper.MouseWorldPos;
                float offsetY = 0;

                foreach (var chipToPlace in _newChipsToPlace)
                {
                    chipToPlace.transform.position = mousePos + Vector2.down * offsetY;
                    SetDepth(chipToPlace, DragDepth);
                    offsetY += chipToPlace.BoundsSize.y + chipStackSpacing;
                }

                // Place object
                if (Input.GetMouseButtonDown(0) && SelectedChipsWithinPlacementArea()) PlaceNewChips();
            }
        }

        private void PlaceNewChips()
        {
            var startDepth = allChips.Count > 0 ? allChips[allChips.Count - 1].transform.position.z : 0;
            for (var i = 0; i < _newChipsToPlace.Count; i++)
                SetDepth(_newChipsToPlace[i], startDepth + (_newChipsToPlace.Count - i) * ChipDepth);

            allChips.AddRange(_newChipsToPlace);
            _selectedChips.Clear();
            _newChipsToPlace.Clear();
            _currentState = State.None;
        }

        private void CancelPlacement()
        {
            for (var i = _newChipsToPlace.Count - 1; i >= 0; i--) Destroy(_newChipsToPlace[i].gameObject);
            _newChipsToPlace.Clear();
            _selectedChips.Clear();
            _currentState = State.None;
        }

        private void DrawSelectedChipBounds()
        {
            selectionBoxMaterial.color = SelectedChipsWithinPlacementArea() ? selectionBoxCol : invalidPlacementCol;

            foreach (var item in _selectedChips)
            {
                var pos = item.transform.position + Vector3.forward * -0.5f;
                var sizeX = item.BoundsSize.x + (Pin.radius + selectionBoundsBorderPadding * 0.75f);
                var sizeY = item.BoundsSize.y + selectionBoundsBorderPadding;
                var matrix = Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(sizeX, sizeY, 1));
                UnityEngine.Graphics.DrawMesh(_selectionMesh, matrix, selectionBoxMaterial, 0);
            }
        }

        private bool SelectedChipsWithinPlacementArea()
        {
            var bufferX = Pin.radius + selectionBoundsBorderPadding * 0.75f;
            var bufferY = selectionBoundsBorderPadding;
            var area = chipArea.bounds;

            foreach (var chip in _selectedChips)
            {
                var position = chip.transform.position;
                var left = position.x - (chip.BoundsSize.x + bufferX) / 2;
                var right = position.x + (chip.BoundsSize.x + bufferX) / 2;
                var top = position.y + (chip.BoundsSize.y + bufferY) / 2;
                var bottom = position.y - (chip.BoundsSize.y + bufferY) / 2;

                if (left < area.min.x || right > area.max.x || top > area.max.y || bottom < area.min.y) return false;
            }

            return true;
        }

        private void SetDepth(Chip.Chip chip, float depth)
        {
            var transform1 = chip.transform;
            var position = transform1.position;
            position = new Vector3(position.x, position.y, depth);
            transform1.position = position;
        }

        protected override bool CanReleaseFocus()
        {
            return _currentState != State.PlacingNewChips && _currentState != State.MovingOldChips;
        }

        protected override void FocusLost()
        {
            _currentState = State.None;
            _selectedChips.Clear();
        }

        private enum State
        {
            None,
            PlacingNewChips,
            MovingOldChips,
            SelectingChips
        }
    }
}