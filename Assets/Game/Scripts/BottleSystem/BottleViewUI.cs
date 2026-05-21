using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BottleSystem
{
    public class BottleViewUI : BottleViewBase
    {
[SerializeField] private RectTransform visualRoot;
        [SerializeField] private List<Image> layerImages = new List<Image>();
        [SerializeField] private Image selectionGlow;
        [SerializeField] private Image pourStream;
        [SerializeField] private bool layerImagesAreBottomToTop = false;
        
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

        public override void PlaySelect()
        {
            Initialize();
            visualRoot.anchoredPosition = originalAnchoredPosition + new Vector2(0, 40);
            if (selectionGlow != null) selectionGlow.enabled = true;
        }

        public override void PlayDeselect()
        {
            Initialize();
            visualRoot.anchoredPosition = originalAnchoredPosition;
            visualRoot.localRotation = Quaternion.identity;
            if (selectionGlow != null) selectionGlow.enabled = false;
            if (pourStream != null) pourStream.enabled = false;
        }

        public override void PlayInvalidMove()
        {
            StartCoroutine(ShakeCoroutine());
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

        public override IEnumerator PlayPourTo(BottleViewBase targetView, string colorId, int amount)
        {
            if (!(targetView is BottleViewUI targetUI)) yield break;

            float duration = 0.45f;
            float elapsed = 0f;
            
            float direction = targetUI.transform.position.x > transform.position.x ? 1f : -1f;
            Quaternion startRot = visualRoot.localRotation;
            Quaternion endRot = Quaternion.Euler(0, 0, -70f * direction);

            Vector2 startPos = visualRoot.anchoredPosition;
            Vector3 targetWorldPos = targetUI.visualRoot.position;
            Vector2 myLocalTarget = transform.InverseTransformPoint(targetWorldPos + new Vector3(-direction * 50, 200, 0));

            if (pourStream != null)
            {
                pourStream.color = StringToColor(colorId);
                pourStream.rectTransform.localRotation = Quaternion.Euler(0, 0, 90 * direction);
            }

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                visualRoot.localRotation = Quaternion.Lerp(startRot, endRot, t);
                visualRoot.anchoredPosition = Vector2.Lerp(startPos, myLocalTarget, t);
                
                if (t > 0.5f && pourStream != null) pourStream.enabled = true;
                
                elapsed += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(0.2f);

            if (pourStream != null) pourStream.enabled = false;

            elapsed = 0f;
            while (elapsed < 0.25f)
            {
                float t = elapsed / 0.25f;
                visualRoot.localRotation = Quaternion.Lerp(endRot, Quaternion.identity, t);
                visualRoot.anchoredPosition = Vector2.Lerp(myLocalTarget, originalAnchoredPosition, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            PlayDeselect();
        }

        public override void RefreshVisuals(List<string> colors, int capacity)
        {
            Initialize();

            for (int i = 0; i < layerImages.Count; i++)
            {
                if (layerImages[i] == null) continue;
                layerImages[i].color = Color.clear;
                layerImages[i].gameObject.SetActive(false);
                layerImages[i].raycastTarget = false;
            }

            int count = colors != null ? colors.Count : 0;

            for (int i = 0; i < count && i < layerImages.Count; i++)
            {
                int visualIndex = layerImagesAreBottomToTop ? i : layerImages.Count - 1 - i;

                Image img = layerImages[visualIndex];
                if (img == null) continue;

                img.color = StringToColor(colors[i]);
                img.gameObject.SetActive(true);
                img.raycastTarget = false;
            }
        }

        private Color StringToColor(string colorId)
        {
            switch (colorId)
            {
                case "Red": return Color.red;
                case "Blue": return Color.blue;
                case "Green": return Color.green;
                case "Yellow": return Color.yellow;
                case "Purple": return new Color(0.5f, 0f, 0.5f);
                default: return Color.white;
            }
        }

        public override void PlayCompleted()
        {
            if (selectionGlow != null) StartCoroutine(FlashGlow());
        }

        private IEnumerator FlashGlow()
        {
            selectionGlow.enabled = true;
            selectionGlow.color = Color.white;
            yield return new WaitForSeconds(0.3f);
            selectionGlow.color = new Color(1, 1, 1, 0.5f);
            yield return new WaitForSeconds(0.3f);
            selectionGlow.enabled = false;
        }
    }
}


