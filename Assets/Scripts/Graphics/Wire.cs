using System.Collections.Generic;
using Chip;
using UnityEngine;
using Utility;

namespace Graphics
{
    public class Wire : MonoBehaviour
    {
        private const float ThicknessMultiplier = 0.1f;

        public Material simpleMat;
        public Color editCol;

        public Palette palette;

        //public Color
        public Color placedCol;
        public float curveSize = 0.5f;
        public int resolution = 10;
        public float thickness = 1;

        public float selectedThickness = 1.2f;

        // [HideInInspector] 
        public Pin startPin;

        // [HideInInspector] 
        public Pin endPin;

        public bool simActive;
        private float _depth;
        private List<Vector2> _drawPoints;
        private float _length;

        private LineRenderer _lineRenderer;
        private Material _mat;
        private bool _selected;
        private EdgeCollider2D _wireCollider;

        private bool _wireConnected;
        public List<Vector2> anchorPoints { get; private set; }

        public Pin ChipInputPin => startPin.pinType == Pin.PinType.ChipInput ? startPin : endPin;

        public Pin ChipOutputPin => startPin.pinType == Pin.PinType.ChipOutput ? startPin : endPin;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
        }

        private void Start()
        {
            _lineRenderer.material = simpleMat;
            _mat = _lineRenderer.material;
        }

        private void LateUpdate()
        {
            SetWireCol();
            if (_wireConnected)
            {
                float depthOffset = 5;

                transform.localPosition = Vector3.forward * (_depth + depthOffset);
                UpdateWirePos();
                //transform.position = new Vector3 (transform.position.x, transform.position.y, inputPin.sequentialState * -0.01f);
            }

            _lineRenderer.startWidth = (_selected ? selectedThickness : thickness) * ThicknessMultiplier;
            _lineRenderer.endWidth = (_selected ? selectedThickness : thickness) * ThicknessMultiplier;
        }

        public void TellWireSimIsOff()
        {
            simActive = false;
        }

        public void tellWireSimIsOn()
        {
            simActive = true;
        }

        public void SetAnchorPoints(Vector2[] newAnchorPoints)
        {
            anchorPoints = new List<Vector2>(newAnchorPoints);
            UpdateSmoothedLine();
            UpdateCollider();
        }

        public void SetDepth(int numWires)
        {
            _depth = numWires * 0.01f;
            transform.localPosition = Vector3.forward * _depth;
        }

        private void UpdateWirePos()
        {
            const float maxSqrError = 0.00001f;
            // How far are start and end points from the pins they're connected to (chip has been moved)
            var startPointError = (Vector2)startPin.transform.position - anchorPoints[0];
            var endPointError = (Vector2)endPin.transform.position - anchorPoints[anchorPoints.Count - 1];

            if (startPointError.sqrMagnitude > maxSqrError || endPointError.sqrMagnitude > maxSqrError)
            {
                // If start and end points are both same offset from where they should be, can move all anchor points (entire wire)
                if ((startPointError - endPointError).sqrMagnitude < maxSqrError &&
                    startPointError.sqrMagnitude > maxSqrError)
                    for (var i = 0; i < anchorPoints.Count; i++)
                        anchorPoints[i] += startPointError;

                anchorPoints[0] = startPin.transform.position;
                anchorPoints[anchorPoints.Count - 1] = endPin.transform.position;
                UpdateSmoothedLine();
                UpdateCollider();
            }
        }

        private void SetWireCol()
        {
            if (_wireConnected)
            {
                var onCol = palette.onCol;
                var offCol = palette.offCol;

                // High Z
                if (ChipOutputPin.State == -1)
                {
                    onCol = palette.highZCol;
                    offCol = palette.highZCol;
                }

                if (simActive)
                {
                    if (startPin.wireType != Pin.WireType.Simple)
                        _mat.color = ChipOutputPin.State == 0 ? offCol : palette.busColor;
                    else
                        _mat.color = ChipOutputPin.State == 0 ? offCol : onCol;
                }
                else
                {
                    _mat.color = offCol;
                }
            }
            else
            {
                _mat.color = Color.black;
            }
        }

        public void Connect(Pin inputPin, Pin outputPin)
        {
            ConnectToFirstPin(inputPin);
            Place(outputPin);
        }

        public void ConnectToFirstPin(Pin pin)
        {
            startPin = pin;
            _lineRenderer = GetComponent<LineRenderer>();
            _mat = simpleMat;
            _drawPoints = new List<Vector2>();

            var transform1 = transform;

            transform1.localPosition = new Vector3(0, 0, transform1.localPosition.z);

            _wireCollider = GetComponent<EdgeCollider2D>();

            anchorPoints = new List<Vector2>();
            var position = pin.transform.position;

            anchorPoints.Add(position);
            anchorPoints.Add(position);
            UpdateSmoothedLine();
            _mat.color = editCol;
        }

        public void ConnectToFirstPinViaWire(Pin pin, Wire parentWire, Vector2 inputPoint)
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _mat = simpleMat;
            _drawPoints = new List<Vector2>();
            startPin = pin;
            var transform1 = transform;

            transform1.localPosition = new Vector3(0, 0, transform1.localPosition.z);

            _wireCollider = GetComponent<EdgeCollider2D>();

            anchorPoints = new List<Vector2>();

            // Find point on wire nearest to input point
            var closestPoint = Vector2.zero;
            var smallestDst = float.MaxValue;
            var closestI = 0;
            for (var i = 0; i < parentWire.anchorPoints.Count - 1; i++)
            {
                var a = parentWire.anchorPoints[i];
                var b = parentWire.anchorPoints[i + 1];
                var pointOnWire = MathUtility.ClosestPointOnLineSegment(a, b, inputPoint);
                var sqrDst = (pointOnWire - inputPoint).sqrMagnitude;

                if (!(sqrDst < smallestDst)) continue;

                smallestDst = sqrDst;
                closestPoint = pointOnWire;
                closestI = i;
            }

            for (var i = 0; i <= closestI; i++) anchorPoints.Add(parentWire.anchorPoints[i]);
            anchorPoints.Add(closestPoint);
            if (Input.GetKey(KeyCode.LeftAlt)) anchorPoints.Add(closestPoint);
            anchorPoints.Add(inputPoint);

            UpdateSmoothedLine();
            _mat.color = editCol;
        }

        // Connect the input pin to the output pin
        public void Place(Pin pinEnd)
        {
            endPin = pinEnd;
            anchorPoints[anchorPoints.Count - 1] = pinEnd.transform.position;
            UpdateSmoothedLine();

            _wireConnected = true;
            UpdateCollider();
        }

        // Update position of wire end point (for when initially placing the wire)
        public void UpdateWireEndPoint(Vector2 endPointWorldSpace)
        {
            anchorPoints[anchorPoints.Count - 1] = ProcessPoint(endPointWorldSpace);
            UpdateSmoothedLine();
        }

        // Add anchor point (for when initially placing the wire)
        public void AddAnchorPoint(Vector2 pointWorldSpace)
        {
            anchorPoints[anchorPoints.Count - 1] = ProcessPoint(pointWorldSpace);
            anchorPoints.Add(ProcessPoint(pointWorldSpace));
        }

        private void UpdateCollider()
        {
            _wireCollider.points = _drawPoints.ToArray();
            _wireCollider.edgeRadius = thickness * ThicknessMultiplier;
        }

        private void UpdateSmoothedLine()
        {
            _length = 0;
            GenerateDrawPoints();

            _lineRenderer.positionCount = _drawPoints.Count;
            var lastLocalPos = Vector2.zero;
            for (var i = 0; i < _lineRenderer.positionCount; i++)
            {
                Vector2 localPos = transform.parent.InverseTransformPoint(_drawPoints[i]);
                _lineRenderer.SetPosition(i, new Vector3(localPos.x, localPos.y, -0.01f));

                if (i > 0) _length += (lastLocalPos - localPos).magnitude;
                lastLocalPos = localPos;
            }
        }

        public void SetSelectionState(bool selected)
        {
            _selected = selected;
        }

        private Vector2 ProcessPoint(Vector2 endPointWorldSpace)
        {
            if (!Input.GetKey(KeyCode.LeftShift)) return endPointWorldSpace;

            var a = anchorPoints[anchorPoints.Count - 2];
            var b = endPointWorldSpace;
            var mid = (a + b) / 2;

            var xAxisLonger = Mathf.Abs(a.x - b.x) > Mathf.Abs(a.y - b.y);
            return xAxisLonger ? new Vector2(b.x, a.y) : new Vector2(a.x, b.y);
        }

        private void GenerateDrawPoints()
        {
            _drawPoints.Clear();
            _drawPoints.Add(anchorPoints[0]);

            for (var i = 1; i < anchorPoints.Count - 1; i++)
            {
                var targetPoint = anchorPoints[i];
                var targetDir = (anchorPoints[i] - anchorPoints[i - 1]).normalized;
                var dstToTarget = (anchorPoints[i] - anchorPoints[i - 1]).magnitude;
                var dstToCurveStart = Mathf.Max(dstToTarget - curveSize, dstToTarget / 2);

                var nextTarget = anchorPoints[i + 1];
                var nextTargetDir = (anchorPoints[i + 1] - anchorPoints[i]).normalized;
                var nextLineLength = (anchorPoints[i + 1] - anchorPoints[i]).magnitude;

                var curveStartPoint = anchorPoints[i - 1] + targetDir * dstToCurveStart;
                var curveEndPoint = targetPoint + nextTargetDir * Mathf.Min(curveSize, nextLineLength / 2);

                // Bezier
                for (var j = 0; j < resolution; j++)
                {
                    var t = j / (resolution - 1f);
                    var a = Vector2.Lerp(curveStartPoint, targetPoint, t);
                    var b = Vector2.Lerp(targetPoint, curveEndPoint, t);
                    var p = Vector2.Lerp(a, b, t);

                    if ((p - _drawPoints[_drawPoints.Count - 1]).sqrMagnitude > 0.001f) _drawPoints.Add(p);
                }
            }

            _drawPoints.Add(anchorPoints[anchorPoints.Count - 1]);
        }
    }
}