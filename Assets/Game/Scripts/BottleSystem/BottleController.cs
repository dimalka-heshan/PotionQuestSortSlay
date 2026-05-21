using System.Collections.Generic;
using UnityEngine;

namespace BottleSystem
{
    public class BottleController : MonoBehaviour
    {
        public int bottleIndex;
        public int capacity = 4;
        
        [Header("Data")]
        [SerializeField] private List<string> colors = new List<string>();
        
        [Header("Visuals")]
        [SerializeField] private BottleViewBase bottleView;

        public void Initialize(int index, int capacity, List<string> initialColors)
        {
            this.bottleIndex = index;
            this.capacity = capacity;
            this.colors = new List<string>();
            
            if (initialColors != null)
            {
                foreach (var c in initialColors)
                {
                    if (!string.IsNullOrEmpty(c) && c != "Empty" && c != "None")
                    {
                        this.colors.Add(c);
                    }
                }
            }
            
            if (bottleView == null) bottleView = GetComponentInChildren<BottleViewBase>();
            RefreshView();
            
            Debug.Log($"[Bottle {bottleIndex} Init] {DebugColors()} Capacity: {capacity}");
        }

        public bool IsEmpty() => colors.Count == 0;
        public bool IsFull() => colors.Count >= capacity;

        public string GetTopColor()
        {
            if (IsEmpty()) return null;
            return colors[colors.Count - 1];
        }

        public int GetTopColorGroupCount()
        {
            if (IsEmpty()) return 0;
            string topColor = GetTopColor();
            int count = 0;
            for (int i = colors.Count - 1; i >= 0; i--)
            {
                if (colors[i] == topColor)
                    count++;
                else
                    break;
            }
            return count;
        }

        public int GetAvailableSpace() => capacity - colors.Count;

        public bool CanPourInto(BottleController target)
        {
            if (target == null || target == this) return false;
            if (IsEmpty()) return false;
            if (target.IsFull()) return false;
            if (target.IsEmpty()) return true;
            return target.GetTopColor() == GetTopColor();
        }

        public int CalculatePourAmountTo(BottleController target)
        {
            if (!CanPourInto(target)) return 0;

            int sourceTopCount = GetTopColorGroupCount();
            int targetSpace = target.GetAvailableSpace();
            return Mathf.Min(sourceTopCount, targetSpace);
        }

        public bool PourTo(BottleController target)
        {
            int amount = CalculatePourAmountTo(target);
            if (amount <= 0) return false;

            string colorToPour = GetTopColor();
            for (int i = 0; i < amount; i++)
            {
                RemoveTopColor();
                target.AddColor(colorToPour);
            }
            return true;
        }

        public string RemoveTopColor()
        {
            if (IsEmpty()) return null;
            int lastIndex = colors.Count - 1;
            string top = colors[lastIndex];
            colors.RemoveAt(lastIndex);
            return top;
        }

        public void AddColor(string colorId)
        {
            if (IsFull() || string.IsNullOrEmpty(colorId)) return;
            colors.Add(colorId);
        }

        public bool IsCompleted()
        {
            if (colors.Count == 0) return false;
            if (colors.Count != capacity) return false;
            
            string first = colors[0];
            foreach (var c in colors)
            {
                if (c != first) return false;
            }
            return true;
        }

        public string DebugColors()
        {
            return "[" + string.Join(", ", colors) + "]";
        }

        public string GetColorsDebugString() => DebugColors();

        public void RefreshView()
        {
            if (bottleView != null)
            {
                bottleView.RefreshVisuals(colors, capacity);
            }
        }

        // View Proxy Methods
        public void Select() => bottleView?.PlaySelect();
        public void Deselect() => bottleView?.PlayDeselect();
        public void NotifyInvalid() => bottleView?.PlayInvalidMove();
        public void NotifyComplete() => bottleView?.PlayCompleted();
        
        public System.Collections.IEnumerator AnimatePourTo(BottleController target, int amount)
        {
            if (bottleView != null && target.bottleView != null)
            {
                yield return bottleView.PlayPourTo(target.bottleView, GetTopColor(), amount);
            }
        }
    }
}
