using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BottleSystem
{
    public class BottleController : MonoBehaviour
    {
        public int bottleIndex;
        public int capacity = 4;
        
        [Header("Data")]
        public string[] colors = new string[4];
        public int numberOfColorsInBottle = 0;
        public string topColor;
        public int numberOfTopColorLayer = 0;

        [Header("Visuals")]
        [SerializeField] private BottleViewUI bottleView;

        public void Initialize(int index, int capacity, List<string> initialColors)
        {
            this.bottleIndex = index;
            this.capacity = capacity;
            this.numberOfColorsInBottle = 0;
            
            for (int i = 0; i < 4; i++)
            {
                if (initialColors != null && i < initialColors.Count)
                {
                    colors[i] = initialColors[i];
                    numberOfColorsInBottle++;
                }
                else
                {
                    colors[i] = "None";
                }
            }

            if (bottleView == null) bottleView = GetComponentInChildren<BottleViewUI>();
            UpdateTopColorValue();
            RefreshView();
            
            Debug.Log($"[Bottle {bottleIndex} Init] {DebugColors()} Count: {numberOfColorsInBottle}");
        }

        public bool IsEmpty() => numberOfColorsInBottle == 0;
        public bool IsFull() => numberOfColorsInBottle >= capacity;

        public int UpdateTopColorValue()
        {
            if (numberOfColorsInBottle == 0)
            {
                topColor = "None";
                numberOfTopColorLayer = 0;
                return 0;
            }

            topColor = colors[numberOfColorsInBottle - 1];
            numberOfTopColorLayer = 1;

            for (int i = numberOfColorsInBottle - 2; i >= 0; i--)
            {
                if (colors[i] == topColor)
                    numberOfTopColorLayer++;
                else
                    break;
            }

            return numberOfTopColorLayer;
        }

        public string GetTopColor()
        {
            if (numberOfColorsInBottle == 0) return null;
            return colors[numberOfColorsInBottle - 1];
        }

        public bool FillBottleCheck(string colorToCheck)
        {
            if (numberOfColorsInBottle == 0) return true;
            if (numberOfColorsInBottle == capacity) return false;
            return topColor == colorToCheck;
        }

        public int CalculatePourAmountTo(BottleController target)
        {
            if (target == null || target == this) return 0;
            if (IsEmpty()) return 0;
            if (!target.FillBottleCheck(topColor)) return 0;

            return Mathf.Min(numberOfTopColorLayer, target.capacity - target.numberOfColorsInBottle);
        }

        public bool PourTo(BottleController target)
        {
            int amount = CalculatePourAmountTo(target);
            if (amount <= 0) return false;

            string colorToTransfer = topColor;
            for (int i = 0; i < amount; i++)
            {
                // Remove from source
                colors[numberOfColorsInBottle - 1] = "None";
                numberOfColorsInBottle--;
                
                // Add to target
                target.colors[target.numberOfColorsInBottle] = colorToTransfer;
                target.numberOfColorsInBottle++;
            }

            UpdateTopColorValue();
            target.UpdateTopColorValue();
            return true;
        }

        public bool IsCompleted()
        {
            if (numberOfColorsInBottle != capacity) return false;
            string first = colors[0];
            if (first == "None") return false;
            for (int i = 1; i < capacity; i++)
            {
                if (colors[i] != first) return false;
            }
            return true;
        }

        public void RefreshView()
        {
            if (bottleView != null)
            {
                bottleView.RefreshVisuals(new List<string>(colors), capacity);
            }
        }

        public string DebugColors()
        {
            List<string> activeColors = new List<string>();
            for(int i=0; i<numberOfColorsInBottle; i++) activeColors.Add(colors[i]);
            return "[" + string.Join(", ", activeColors) + "]";
        }

        public void Select() => bottleView?.PlaySelect();
        public void Deselect() => bottleView?.PlayDeselect();
        public void NotifyInvalid() => bottleView?.PlayInvalidMove();
        public void NotifyComplete() => bottleView?.PlayCompleted();

        public IEnumerator AnimatePourTo(BottleController target, int amount)
        {
            if (bottleView != null && target.bottleView != null)
            {
                yield return bottleView.PlayPourTo(target.bottleView, topColor, amount);
            }
        }
    }
}
