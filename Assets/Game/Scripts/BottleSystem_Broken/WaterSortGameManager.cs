using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BottleSystem
{
    /// <summary>
    /// Assets/color-sort is temporary reference only.
    /// Final runtime code should live under Assets/Game.
    /// </summary>
    public class WaterSortGameManager : MonoBehaviour
{
        public static WaterSortGameManager Instance;

        [Header("Settings")]
        [SerializeField] private GameObject bottlePrefab;
        [SerializeField] private Transform bottleContainer;
        [SerializeField] private LayerMask bottleLayer;

        [Header("Colors")]
        [SerializeField] private Color redColor = Color.red;
        [SerializeField] private Color blueColor = Color.blue;
        [SerializeField] private Color greenColor = Color.green;
        [SerializeField] private Color yellowColor = Color.yellow;
        [SerializeField] private Color purpleColor = new Color(0.5f, 0, 0.5f);
        [SerializeField] private Color noneColor = Color.clear;

        private List<WaterSortBottle> allBottles = new List<WaterSortBottle>();
        private WaterSortBottle selectedBottle;
        public bool IsAnimating { get; set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // For testing purposes, we can manually initialize if needed, 
            // but the prompt asked for a clean setup in SampleScene.
        }

        private void Update()
        {
            if (IsAnimating) return;

            if (Input.GetMouseButtonDown(0))
            {
                HandleInput();
            }
        }

        private void HandleInput()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero, 10f, bottleLayer);

            if (hit.collider != null)
            {
                WaterSortBottle clickedBottle = hit.collider.GetComponent<WaterSortBottle>();
                if (clickedBottle != null)
                {
                    if (selectedBottle == null)
                    {
                        if (clickedBottle.numberOfColorsInBottle > 0)
                        {
                            selectedBottle = clickedBottle;
                            selectedBottle.Select();
                        }
                    }
                    else if (selectedBottle == clickedBottle)
                    {
                        selectedBottle.Deselect();
                        selectedBottle = null;
                    }
                    else
                    {
                        if (selectedBottle.CanPourInto(clickedBottle))
                        {
                            selectedBottle.StartPour(clickedBottle);
                            selectedBottle = null;
                        }
                        else
                        {
                            // Shake or reject invalid move
                            selectedBottle.Deselect();
                            selectedBottle = null;
                        }
                    }
                }
            }
            else
            {
                if (selectedBottle != null)
                {
                    selectedBottle.Deselect();
                    selectedBottle = null;
                }
            }
        }

        public Color GetColor(string colorId)
        {
            switch (colorId)
            {
                case "Red": return redColor;
                case "Blue": return blueColor;
                case "Green": return greenColor;
                case "Yellow": return yellowColor;
                case "Purple": return purpleColor;
                default: return noneColor;
            }
        }

        public void CheckWinCondition()
        {
            bool won = true;
            foreach (var bottle in allBottles)
            {
                if (bottle.numberOfColorsInBottle == 0) continue;
                
                if (bottle.numberOfColorsInBottle != bottle.capacity)
                {
                    won = false;
                    break;
                }

                string first = bottle.bottleColors[0];
                for (int i = 1; i < bottle.capacity; i++)
                {
                    if (bottle.bottleColors[i] != first)
                    {
                        won = false;
                        break;
                    }
                }
                
                if (!won) break;
            }

            if (won)
            {
                Debug.Log("Puzzle Solved!");
            }
        }

        public void RegisterBottle(WaterSortBottle bottle)
        {
            if (!allBottles.Contains(bottle))
                allBottles.Add(bottle);
        }
    }
}
