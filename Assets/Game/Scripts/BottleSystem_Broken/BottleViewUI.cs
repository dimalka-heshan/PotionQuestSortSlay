using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BottleSystem
{
    public class BottleViewUI : BottleViewBase
    {
        [Header("Hierarchy References")]
        [SerializeField] private RectTransform visualRoot;
        [SerializeField] private List<Image> layerImages = new List<Image>();
        [SerializeField] private Image selectionGlow;
        [SerializeField] private Image pourStream;

        [Header("Settings")]
        [SerializeField] private bool layerImagesAreBottomToTop = true;
        [SerializeField] private float liftHeight = 40f;
        [SerializeField] private float pourRotation = 75f;
        [SerializeField] private float animationDuration = 0.35f;

        private Vector2 originalAnchoredPosition;
        private bool isInitialized = false;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (isInitialized) return;
            if (visualRoot == null) visualRoot = GetComponent<RectTransform>();
            originalAnchoredPosition = visualRoot.anchoredPosition;
            if (selectionGlow != null) selectionGlow.enabled = false;
            if (pourStream != null) pourStream.enabled = false;
            isInitialized = true;
        }

        public override void RefreshVisuals(List<string> colors, int capacity)
        {
            Initialize();

            // colors[0] is bottom, colors[last] is top
            int dataCount = colors != null ? colors.Count : 0;

            for (int i = 0; i < layerImages.Count; i++)
            {
                if (layerImages[i] == null) continue;

                // Keep all slots active but clear if unused
                layerImages[i].gameObject.SetActive(true);
                layerImages[i].raycastTarget = false;

                // Map data index to visual index
                // If layerImages[0] is visually at the bottom, then relativeIndex = i
                int relativeFromBottom = layerImagesAreBottomToTop ? i : (layerImages.Count - 1 - i);

                if (relativeFromBottom < dataCount && colors[relativeFromBottom] != "None")
                {
                    layerImages[i].color = StringToColor(colors[relativeFromBottom]);
                }
                else
                {
                    layerImages[i].color = Color.clear;
                }
            }
        }

        public override void PlaySelect()
        {
            Initialize();
            StopAllCoroutines();
            StartCoroutine(AnimateMove(originalAnchoredPosition + new Vector2(0, liftHeight), 0.15f));
            if (selectionGlow != null) selectionGlow.enabled = true;
        }

        public override void PlayDeselect()
        {
            Initialize();
            StopAllCoroutines();
            StartCoroutine(AnimateMoveAndRotate(originalAnchoredPosition, Quaternion.identity, 0.15f));
            if (selectionGlow != null) selectionGlow.enabled = false;
            if (pourStream != null) pourStream.enabled = false;
        }

        public override void PlayInvalidMove()
        {
            StartCoroutine(ShakeCoroutine());
        }

        public override void PlayCompleted()
        {
            if (selectionGlow != null) StartCoroutine(FlashGlow());
        }

        public override IEnumerator PlayPourTo(BottleViewBase targetView, string colorId, int amount)
        {
            if (!(targetView is BottleViewUI targetUI)) yield break;

            float direction = targetUI.transform.position.x > transform.position.x ? 1f : -1f;
            Vector2 startPos = visualRoot.anchoredPosition;
            
            // Calculate a point above the target bottle
            Vector3 targetWorldPos = targetUI.visualRoot.position;
            Vector2 myLocalTarget = transform.InverseTransformPoint(targetWorldPos + new Vector3(-direction * 30, 180, 0));
            
            Quaternion startRot = visualRoot.localRotation;
            Quaternion endRot = Quaternion.Euler(0, 0, -pourRotation * direction);

            // 1. Move to target
            yield return AnimateMoveAndRotate(myLocalTarget, endRot, animationDuration);

            // 2. Pouring (Stream)
            if (pourStream != null)
            {
                pourStream.color = StringToColor(colorId);
                pourStream.enabled = true;
                // Align stream with rotation
                pourStream.rectTransform.localRotation = Quaternion.Euler(0, 0, 90 * direction);
            }
            yield return new WaitForSeconds(0.4f);
            if (pourStream != null) pourStream.enabled = false;

            // 3. Return
            yield return AnimateMoveAndRotate(originalAnchoredPosition, Quaternion.identity, animationDuration);
            
            PlayDeselect();
        }

        private IEnumerator AnimateMove(Vector2 targetPos, float duration)
        {
            Vector2 startPos = visualRoot.anchoredPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                visualRoot.anchoredPosition = Vector2.Lerp(startPos, targetPos, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            visualRoot.anchoredPosition = targetPos;
        }

        private IEnumerator AnimateMoveAndRotate(Vector2 targetPos, Quaternion targetRot, float duration)
        {
            Vector2 startPos = visualRoot.anchoredPosition;
            Quaternion startRot = visualRoot.localRotation;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                visualRoot.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                visualRoot.localRotation = Quaternion.Lerp(startRot, targetRot, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            visualRoot.anchoredPosition = targetPos;
            visualRoot.localRotation = targetRot;
        }

        private IEnumerator ShakeCoroutine()
        {
            float duration = 0.3f;
            float elapsed = 0f;
            float magnitude = 15f;
            Vector2 basePos = visualRoot.anchoredPosition;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                visualRoot.anchoredPosition = basePos + new Vector2(x, 0);
                elapsed += Time.deltaTime;
                yield return null;
            }
            visualRoot.anchoredPosition = basePos;
        }

        private IEnumerator FlashGlow()
        {
            if (selectionGlow == null) yield break;
            selectionGlow.enabled = true;
            selectionGlow.color = Color.white;
            yield return new WaitForSeconds(0.3f);
            selectionGlow.color = new Color(1, 1, 1, 0.5f);
            yield return new WaitForSeconds(0.3f);
            selectionGlow.enabled = false;
        }

        private Color StringToColor(string colorId)
        {
            switch (colorId)
            {
                case "Red": return new Color(0.9f, 0.1f, 0.1f);
                case "Blue": return new Color(0.1f, 0.4f, 0.9f);
                case "Green": return new Color(0.1f, 0.8f, 0.2f);
                case "Yellow": return new Color(1.0f, 0.9f, 0.1f);
                case "Purple": return new Color(0.6f, 0.1f, 0.8f);
                case "Orange": return new Color(1.0f, 0.6f, 0.1f);
                case "Cyan": return Color.cyan;
                case "Magenta": return Color.magenta;
                case "Gray": return Color.gray;
                case "Pink": return new Color(1.0f, 0.4f, 0.7f);
                default: return Color.white;
            }
        }
    }
}


