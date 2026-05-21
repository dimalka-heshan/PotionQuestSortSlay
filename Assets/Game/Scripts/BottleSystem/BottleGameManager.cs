using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BottleSystem
{
    public class BottleGameManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int startLevel = 1;
        [SerializeField] private GameObject bottlePrefab;
        [SerializeField] private Transform bottleContainer;

        [Header("UI References")]
        [SerializeField] private List<BottleController> bottles = new List<BottleController>();
        [SerializeField] private TextMeshProUGUI movesText;
        [SerializeField] private TextMeshProUGUI enemyNameText;

        [Header("Runtime State")]
        private BottleController selectedBottle;
        private int moveCount;
        private bool isGameOver = false;
        private bool isAnimating = false;
        private LevelData currentLevelData;
        private LevelLoader loader;

        private void Awake()
        {
            loader = GetComponent<LevelLoader>();
            if (loader == null) loader = gameObject.AddComponent<LevelLoader>();
            
            if (bottleContainer == null) {
                GameObject area = GameObject.Find("BottleArea");
                if (area != null) bottleContainer = area.transform;
            }
        }

        private void Start()
        {
            LoadLevel(startLevel);
        }

        public void LoadLevel(int levelNumber)
        {
            currentLevelData = loader.LoadLevel(levelNumber);
            if (currentLevelData == null) return;

            SetupLevel();
        }

        private void SetupLevel()
        {
            isGameOver = false;
            isAnimating = false;
            selectedBottle = null;
            moveCount = currentLevelData.moveLimit;
            UpdateMovesUI();

            if (enemyNameText != null)
            {
                enemyNameText.text = !string.IsNullOrEmpty(currentLevelData.enemyName) 
                    ? currentLevelData.enemyName 
                    : currentLevelData.enemyId;
            }

            int requiredBottles = currentLevelData.bottles.Length;
            
            // Reuse or Instantiate bottles
            List<BottleController> existingBottles = new List<BottleController>();
            foreach (Transform t in bottleContainer) {
                var bc = t.GetComponent<BottleController>();
                if (bc != null) existingBottles.Add(bc);
            }

            if (existingBottles.Count < requiredBottles && bottlePrefab != null) {
                int toCreate = requiredBottles - existingBottles.Count;
                for (int i = 0; i < toCreate; i++) {
                    GameObject go = Instantiate(bottlePrefab, bottleContainer);
                    existingBottles.Add(go.GetComponent<BottleController>());
                }
            }

            bottles.Clear();
            for (int i = 0; i < existingBottles.Count; i++) {
                if (i < requiredBottles) {
                    existingBottles[i].gameObject.SetActive(true);
                    
                    string[] rawColors = currentLevelData.bottles[i] != null ? currentLevelData.bottles[i].colors : null;
                    List<string> colors = rawColors != null ? new List<string>(rawColors) : new List<string>();
                    existingBottles[i].Initialize(i, currentLevelData.bottleCapacity, colors);
                    
                    bottles.Add(existingBottles[i]);

                    Button btn = existingBottles[i].GetComponent<Button>();
                    if (btn != null) {
                        btn.onClick.RemoveAllListeners();
                        BottleController captured = existingBottles[i];
                        btn.onClick.AddListener(() => OnBottleClicked(captured));
                    }
                } else {
                    existingBottles[i].gameObject.SetActive(false);
                }
            }
            
            Debug.Log($"[GameManager] Level {currentLevelData.levelNumber} Setup Complete.");
        }

        public void OnBottleClicked(BottleController clickedBottle)
        {
            if (isGameOver || isAnimating || clickedBottle == null) return;

            if (selectedBottle == null)
            {
                // First Selection
                if (clickedBottle.IsEmpty()) return;
                
                selectedBottle = clickedBottle;
                selectedBottle.Select();
            }
            else
            {
                if (selectedBottle == clickedBottle)
                {
                    // Deselect
                    selectedBottle.Deselect();
                    selectedBottle = null;
                }
                else
                {
                    // Second Selection - Attempt Pour
                    int pourAmount = selectedBottle.CalculatePourAmountTo(clickedBottle);
                    if (pourAmount > 0)
                    {
                        StartCoroutine(HandlePourSequence(selectedBottle, clickedBottle, pourAmount));
                        selectedBottle = null;
                    }
                    else
                    {
                        // Invalid move - shake and deselect source
                        clickedBottle.NotifyInvalid();
                        selectedBottle.NotifyInvalid();
                        selectedBottle.Deselect();
                        selectedBottle = null;
                    }
                }
            }
        }

        private IEnumerator HandlePourSequence(BottleController source, BottleController target, int amount)
        {
            isAnimating = true;

            string colorName = source.topColor;
            Debug.Log($"[POUR] {source.bottleIndex} -> {target.bottleIndex} | Color: {colorName}, Amount: {amount}");

            // Visual Animation
            yield return source.AnimatePourTo(target, amount);
            
            // Logic Update
            source.PourTo(target);
            
            // Refresh Visuals
            source.RefreshView();
            target.RefreshView();

            moveCount--;
            UpdateMovesUI();

            if (target.IsCompleted())
            {
                target.NotifyComplete();
            }

            CheckWinCondition();
            isAnimating = false;
        }

        private void UpdateMovesUI()
        {
            if (movesText != null) movesText.text = $"Moves: {moveCount}";
        }

        private void CheckWinCondition()
        {
            bool allSolved = true;
            foreach (var bottle in bottles)
            {
                if (bottle.IsEmpty()) continue;
                if (!bottle.IsCompleted()) {
                    allSolved = false;
                    break;
                }
            }

            if (allSolved)
            {
                isGameOver = true;
                if (movesText != null) movesText.text = "SOLVED!";
            }
            else if (moveCount <= 0)
            {
                isGameOver = true;
                if (movesText != null) movesText.text = "FAILED!";
            }
        }
    }
}



