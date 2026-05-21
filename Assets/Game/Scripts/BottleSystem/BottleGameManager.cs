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
                if (!string.IsNullOrEmpty(currentLevelData.enemyName))
                    enemyNameText.text = currentLevelData.enemyName;
                else
                    enemyNameText.text = currentLevelData.enemyId;
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
                    
                    List<string> initialColors = currentLevelData.bottles[i].colors != null 
                        ? new List<string>(currentLevelData.bottles[i].colors) 
                        : new List<string>();
                        
                    existingBottles[i].Initialize(i, currentLevelData.bottleCapacity, initialColors);
                    bottles.Add(existingBottles[i]);

                    Button btn = existingBottles[i].GetComponent<Button>();
                    if (btn != null) {
                        btn.onClick.RemoveAllListeners();
                        BottleController captured = existingBottles[i];
                        btn.onClick.AddListener(() => OnBottleClicked(captured));
                    }
                    
                    Debug.Log($"[GameManager] Bottle {i} Setup: IsEmpty={existingBottles[i].IsEmpty()}, IsFull={existingBottles[i].IsFull()}, Colors={existingBottles[i].DebugColors()}");
                } else {
                    existingBottles[i].gameObject.SetActive(false);
                }
            }
            
            Debug.Log($"[GameManager] Level {currentLevelData.levelNumber} Setup Complete. Enemy: {(enemyNameText != null ? enemyNameText.text : "N/A")}");
        }

        public void OnBottleClicked(BottleController bottle)
        {
            if (isGameOver || isAnimating || bottle == null) return;

            if (selectedBottle == null)
            {
                if (bottle.IsEmpty()) return;
                
                selectedBottle = bottle;
                selectedBottle.Select();
            }
            else
            {
                if (selectedBottle == bottle)
                {
                    selectedBottle.Deselect();
                    selectedBottle = null;
                }
                else
                {
                    StartCoroutine(HandlePourSequence(selectedBottle, bottle));
                    selectedBottle = null;
                }
            }
        }

        private IEnumerator HandlePourSequence(BottleController source, BottleController target)
        {
            isAnimating = true;

            if (source == null || target == null)
            {
                isAnimating = false;
                yield break;
            }

            int pourAmount = source.CalculatePourAmountTo(target);
            if (pourAmount > 0)
            {
                yield return source.AnimatePourTo(target, pourAmount);
                source.PourTo(target);
                
                Debug.Log("[After Pour] Source " + source.bottleIndex + ": " + source.DebugColors());
                Debug.Log("[After Pour] Target " + target.bottleIndex + ": " + target.DebugColors());

                source.RefreshView();
target.RefreshView();

                moveCount--;
                UpdateMovesUI();

                if (target.IsCompleted())
                {
                    target.NotifyComplete();
                }

                source.Deselect();
                CheckWinCondition();
            }
            else
            {
                target.NotifyInvalid();
                source.NotifyInvalid();
                source.Deselect();
            }

            isAnimating = false;
        }

        private void UpdateMovesUI()
        {
            if (movesText != null)
            {
                movesText.text = $"Moves: {moveCount}";
            }
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



