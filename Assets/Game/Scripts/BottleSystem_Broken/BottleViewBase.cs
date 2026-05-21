using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BottleSystem
{
    public abstract class BottleViewBase : MonoBehaviour
    {
        public abstract void PlaySelect();
        public abstract void PlayDeselect();
        public abstract void PlayInvalidMove();
        public abstract IEnumerator PlayPourTo(BottleViewBase targetView, string colorId, int amount);
        public abstract void RefreshVisuals(List<string> colors, int capacity);
        public abstract void PlayCompleted();
    }
}
