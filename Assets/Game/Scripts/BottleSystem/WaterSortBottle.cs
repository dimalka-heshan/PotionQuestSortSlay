using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BottleSystem
{
    /// <summary>
    /// Assets/color-sort is temporary reference only.
    /// Final runtime code should live under Assets/Game.
    /// </summary>
    public class WaterSortBottle : MonoBehaviour
    {
        [Header("Data")]
        public string[] bottleColors = new string[4]; // IDs like "Blue", "Red"
        public int numberOfColorsInBottle = 0;
        public int capacity = 4;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer bottleMaskSR;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Transform leftRotationPoint;
        [SerializeField] private Transform rightRotationPoint;
        [SerializeField] private float timeToRotate = 0.5f;

        [Header("Animation Curves")]
        [SerializeField] private AnimationCurve ScaleAndRotationMutiplaierCurve;
        [SerializeField] private AnimationCurve FillAmountCurve;
        [SerializeField] private AnimationCurve RotaationSpeedMultiplaier;

        [Header("Shader Settings")]
        [SerializeField] private float[] fillAmounts = new float[] { -0.75f, -0.435f, -0.12f, 0.195f, 0.51f };
        [SerializeField] private float[] rotationValues = new float[] { 54, 71, 83, 90 };

        private int rotationIndex;
        private Transform chosenRotationPoint;
        private float directionMultiplaier = 1.0f;
        private Vector3 originalPosition;
        private Quaternion originalRotation;

        public string topColor => (numberOfColorsInBottle > 0) ? bottleColors[numberOfColorsInBottle - 1] : "None";
        public int numberOfTopColorLayers { get; private set; }

        private void Start()
        {
            originalPosition = transform.position;
            originalRotation = transform.rotation;
            UpdateVisuals();
        }

        public void Initialize(string[] initialColors)
        {
            numberOfColorsInBottle = 0;
            for (int i = 0; i < 4; i++)
            {
                if (initialColors != null && i < initialColors.Length && !string.IsNullOrEmpty(initialColors[i]))
                {
                    bottleColors[i] = initialColors[i];
                    numberOfColorsInBottle++;
                }
                else
                {
                    bottleColors[i] = "None";
                }
            }
            UpdateTopColorValue();
            UpdateVisuals();
        }

        public void UpdateTopColorValue()
        {
            if (numberOfColorsInBottle == 0)
            {
                numberOfTopColorLayers = 0;
                return;
            }

            string top = topColor;
            numberOfTopColorLayers = 1;

            for (int i = numberOfColorsInBottle - 2; i >= 0; i--)
            {
                if (bottleColors[i] == top)
                    numberOfTopColorLayers++;
                else
                    break;
            }
        }

        public bool CanPourInto(WaterSortBottle target)
        {
            if (numberOfColorsInBottle == 0) return false;
            if (target.numberOfColorsInBottle >= target.capacity) return false;
            if (target.numberOfColorsInBottle == 0) return true;
            return target.topColor == topColor;
        }

        public int GetPourAmount(WaterSortBottle target)
        {
            int freeSpace = target.capacity - target.numberOfColorsInBottle;
            return Mathf.Min(numberOfTopColorLayers, freeSpace);
        }

        public void StartPour(WaterSortBottle target)
        {
            int amount = GetPourAmount(target);
            StartCoroutine(PourCoroutine(target, amount));
        }

        private IEnumerator PourCoroutine(WaterSortBottle target, int amount)
        {
            WaterSortGameManager.Instance.IsAnimating = true;

            // Determine rotation point and direction
            if (transform.position.x > target.transform.position.x)
            {
                chosenRotationPoint = leftRotationPoint;
                directionMultiplaier = -1.0f;
            }
            else
            {
                chosenRotationPoint = rightRotationPoint;
                directionMultiplaier = 1.0f;
            }

            rotationIndex = 3 - (numberOfColorsInBottle - amount);

            // Move to target
            Vector3 startPos = transform.position;
            Vector3 endPos = (directionMultiplaier > 0) ? target.leftRotationPoint.position : target.rightRotationPoint.position;
            // Adjust endPos so this bottle's rotation point aligns with target's top
            endPos -= (chosenRotationPoint.position - transform.position);
            endPos += Vector3.up * 0.5f; // Hover slightly above

            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * 4;
                transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            // Prepare target colors before animation so it fills with the correct color
            string pouredColor = topColor;
            for (int i = 0; i < amount; i++)
            {
                target.bottleColors[target.numberOfColorsInBottle + i] = pouredColor;
            }
            target.UpdateVisuals();

            // Rotate and Pour
            float rotateT = 0;
            float lastAngle = 0;
            Color shaderPouredColor = WaterSortGameManager.Instance.GetColor(pouredColor);

            if (lineRenderer != null)
            {
                lineRenderer.startColor = shaderPouredColor;
                lineRenderer.endColor = shaderPouredColor;
                lineRenderer.enabled = false;
            }

            while (rotateT < timeToRotate)
            {
                float lerpVal = rotateT / timeToRotate;
                float angle = Mathf.Lerp(0, directionMultiplaier * rotationValues[rotationIndex], lerpVal);
                
                transform.RotateAround(chosenRotationPoint.position, Vector3.forward, lastAngle - angle);
                lastAngle = angle;

                if (bottleMaskSR != null)
                {
                    Material sourceMat = Application.isPlaying ? bottleMaskSR.material : bottleMaskSR.sharedMaterial;
                    sourceMat.SetFloat("_ScaleAndRotationMultiplaier", ScaleAndRotationMutiplaierCurve.Evaluate(angle));
                    
                    float currentFill = sourceMat.GetFloat("_FillAmount");
                    float targetFillValue = FillAmountCurve.Evaluate(angle);

                    if (currentFill > targetFillValue)
                    {
                        if (lineRenderer != null && !lineRenderer.enabled)
                        {
                            lineRenderer.enabled = true;
                            lineRenderer.SetPosition(0, chosenRotationPoint.position);
                            lineRenderer.SetPosition(1, target.transform.position + Vector3.up * 1.5f);
                        }
                        
                        float diff = currentFill - targetFillValue;
                        sourceMat.SetFloat("_FillAmount", targetFillValue);
                        target.FillUp(diff);
                    }
                }

                rotateT += Time.deltaTime * RotaationSpeedMultiplaier.Evaluate(angle);
                yield return null;
            }

            if (lineRenderer != null) lineRenderer.enabled = false;

            // Update Logic Data Final State
            for (int i = 0; i < amount; i++)
            {
                target.numberOfColorsInBottle++;
                bottleColors[numberOfColorsInBottle - 1] = "None";
                numberOfColorsInBottle--;
            }

            UpdateTopColorValue();
            target.UpdateTopColorValue();
            UpdateVisuals();
            target.UpdateVisuals();

            // Rotate back
            rotateT = 0;
            while (rotateT < timeToRotate)
            {
                float lerpVal = rotateT / timeToRotate;
                float angle = Mathf.Lerp(directionMultiplaier * rotationValues[rotationIndex], 0, lerpVal);
                transform.RotateAround(chosenRotationPoint.position, Vector3.forward, lastAngle - angle);
                lastAngle = angle;
                if (bottleMaskSR != null)
                {
                    Material sourceMat = Application.isPlaying ? bottleMaskSR.material : bottleMaskSR.sharedMaterial;
                    sourceMat.SetFloat("_ScaleAndRotationMultiplaier", ScaleAndRotationMutiplaierCurve.Evaluate(angle));
                }
                rotateT += Time.deltaTime;
                yield return null;
            }

            transform.rotation = originalRotation;

            // Move back
            t = 0;
            startPos = transform.position;
            while (t < 1)
            {
                t += Time.deltaTime * 4;
                transform.position = Vector3.Lerp(startPos, originalPosition, t);
                yield return null;
            }
            transform.position = originalPosition;

            WaterSortGameManager.Instance.IsAnimating = false;
            WaterSortGameManager.Instance.CheckWinCondition();
        }

        public void FillUp(float amount)
        {
            if (bottleMaskSR != null)
            {
                Material mat = Application.isPlaying ? bottleMaskSR.material : bottleMaskSR.sharedMaterial;
                float current = mat.GetFloat("_FillAmount");
                mat.SetFloat("_FillAmount", current + amount);
            }
        }

        public void UpdateVisuals()
        {
            if (bottleMaskSR == null) return;

            Material mat = Application.isPlaying ? bottleMaskSR.material : bottleMaskSR.sharedMaterial;
            if (mat == null) return;

            mat.SetFloat("_FillAmount", fillAmounts[numberOfColorsInBottle]);
            
            for (int i = 0; i < 4; i++)
            {
                Color c = Color.clear;
                if (WaterSortGameManager.Instance != null)
                {
                    c = WaterSortGameManager.Instance.GetColor(bottleColors[i]);
                }
                else
                {
                    // Fallback colors for editor
                    switch(bottleColors[i]) {
                        case "Red": c = Color.red; break;
                        case "Blue": c = Color.blue; break;
                        case "Green": c = Color.green; break;
                        case "Yellow": c = Color.yellow; break;
                        case "Purple": c = new Color(0.5f, 0, 0.5f); break;
                        default: c = Color.clear; break;
                    }
                }
                mat.SetColor("_Color0" + (i + 1), c);
            }
        }

        public string DebugColors()
        {
            List<string> activeColors = new List<string>();
            for(int i=0; i<numberOfColorsInBottle; i++) activeColors.Add(bottleColors[i]);
            return "[" + string.Join(", ", activeColors) + "]";
        }

        public void Select()
        {
            StopAllCoroutines();
            StartCoroutine(AnimateMove(originalPosition + Vector3.up * 0.5f));
        }

        public void Deselect()
        {
            StopAllCoroutines();
            StartCoroutine(AnimateMove(originalPosition));
        }

        private IEnumerator AnimateMove(Vector3 target)
        {
            float t = 0;
            Vector3 start = transform.position;
            while (t < 1)
            {
                t += Time.deltaTime * 5;
                transform.position = Vector3.Lerp(start, target, t);
                yield return null;
            }
            transform.position = target;
        }
    }
}
