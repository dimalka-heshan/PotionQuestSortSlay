using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace ColorSortCore
{
public class BottleController : MonoBehaviour
{
    [SerializeField] public string[] bottleColorNames = new string[4];
    [SerializeField] public SpriteRenderer bottleMaskSR;

    [SerializeField] AnimationCurve ScaleAndRotationMutiplaierCurve;
    [SerializeField] AnimationCurve FillAmountCurve;
    [SerializeField] AnimationCurve RotaationSpeedMultiplaier;

    [SerializeField] public float[] fillAmounts = new float[] { -0.75f, -0.435f, -0.12f, 0.195f, 0.51f };
    [SerializeField] public float[] rotationValues = new float[] { 54, 71, 83, 90 };

    private int rotationIndex;

    [Range(0,4)] [SerializeField] public int numberOfColorsInBottle = 4;

    public string topColorName; 
    public Color topColor;
    public int numberOfTopColorLayer = 0;

    public BottleController bottleControllerRef;

    private int numberOfColorsToTranfer = 0;
    private int numberOfLayersToTranfer = 0;

    public Transform leftRotationPoint;
    public Transform rightRotationPoint;
    private Transform chosenRotationPoint;

    private float directionMultiplaier = 1.0f;

    Vector3 startPosition;
    Vector3 endPosition;
    Vector3 originalPosition;

    public LineRenderer lineRenderer;

    [SerializeField] float timeToRotate = 1.0f;

    private GameController gameController;
    public AudioSource boilingSound;
    private GameObject[] levelbottles;
    private float addedAmount;

    void Start()
    {
        gameController = UnityEngine.Object.FindFirstObjectByType<GameController>();
        originalPosition = transform.position;

        if (bottleMaskSR != null)
            bottleMaskSR.material.SetFloat("_FillAmount", fillAmounts[numberOfColorsInBottle]);

        UpdateColorsOnShader();
        UpdateTopColorValue();
    }

    void Update()
    {
        numberOfColorsInBottle = Mathf.Clamp(numberOfColorsInBottle, 0, 4);
    }

    public void startColorTransfer()
    {
        LockAll();
        chosenRotationPointAndDirection();

        numberOfColorsToTranfer = Mathf.Min(numberOfTopColorLayer, 4 - bottleControllerRef.numberOfColorsInBottle);
        numberOfLayersToTranfer = Mathf.Min(numberOfTopColorLayer, 4 - bottleControllerRef.numberOfColorsInBottle);

        for(int i = 0 ; i < numberOfColorsToTranfer ; i++)
        {
            bottleControllerRef.bottleColorNames[bottleControllerRef.numberOfColorsInBottle + i ] = topColorName;
        }

        bottleControllerRef.UpdateColorsOnShader();
        CalculateRotationIndex(4 - bottleControllerRef.numberOfColorsInBottle);

        if (GetComponent<SpriteRenderer>() != null) GetComponent<SpriteRenderer>().sortingOrder += 2;
        if (bottleMaskSR != null) bottleMaskSR.sortingOrder += 2;

        StartCoroutine(MoveBottle()); 
    }

    public void UpdateColorsOnShader()
    {
        if (bottleMaskSR == null || gameController == null) return;
        for (int i = 0; i < 4; i++)
        {
            string colorName = (i < bottleColorNames.Length) ? bottleColorNames[i] : "None";
            Color c = gameController.GetColorByName(colorName);
            bottleMaskSR.material.SetColor("_Color0" + (i + 1), c);
        }
    }

    IEnumerator MoveBottle()
    {
        startPosition = transform.position;
        if(chosenRotationPoint == leftRotationPoint) endPosition = bottleControllerRef.rightRotationPoint.position;
        else endPosition = bottleControllerRef.leftRotationPoint.position;

        float t1 = 0;
        while(t1 <= 1)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, t1);
            t1 += Time.deltaTime * 2;
            yield return new WaitForEndOfFrame();
        }
        transform.position = endPosition;
        StartCoroutine(RotateBottle());
    }

    IEnumerator RotateBottle()
    {
        float t = 0f;
        float angleVlaue;
        float lastAngleValue  = 0f;

        while(t < timeToRotate)
        {
            float lerpValue = t / timeToRotate;
            angleVlaue = Mathf.Lerp(0.0f, directionMultiplaier * rotationValues[rotationIndex], lerpValue);
            transform.RotateAround(chosenRotationPoint.position, Vector3.forward, lastAngleValue - angleVlaue);

            if (bottleMaskSR != null)
            {
                bottleMaskSR.material.SetFloat("_ScaleAndRotationMultiplaier", ScaleAndRotationMutiplaierCurve.Evaluate(angleVlaue));
                if(fillAmounts[numberOfColorsInBottle] > FillAmountCurve.Evaluate(angleVlaue))
                {
                    if(lineRenderer != null && lineRenderer.enabled == false)
                    {
                        PlayBoilingSound();
                        Color c = gameController.GetColorByName(topColorName);
                        lineRenderer.startColor = c;
                        lineRenderer.endColor = c;
                        lineRenderer.SetPosition(0, chosenRotationPoint.position);
                        lineRenderer.SetPosition(1, chosenRotationPoint.position - Vector3.up * 1.45f);  
                        lineRenderer.enabled = true;
                    }
                    bottleMaskSR.material.SetFloat("_FillAmount", FillAmountCurve.Evaluate(angleVlaue)); 
                    addedAmount = FillAmountCurve.Evaluate(lastAngleValue) - FillAmountCurve.Evaluate(angleVlaue);
                    bottleControllerRef.FillUp(addedAmount);
                }
            }
            t += Time.deltaTime * RotaationSpeedMultiplaier.Evaluate(angleVlaue);
            lastAngleValue = angleVlaue;
            yield return new WaitForEndOfFrame();
        }

        angleVlaue = directionMultiplaier * rotationValues[rotationIndex];
        if (bottleMaskSR != null)
        {
            bottleMaskSR.material.SetFloat("_ScaleAndRotationMultiplaier", ScaleAndRotationMutiplaierCurve.Evaluate(angleVlaue));
            bottleMaskSR.material.SetFloat("_FillAmount", FillAmountCurve.Evaluate(angleVlaue));
        }

        numberOfColorsInBottle -= numberOfColorsToTranfer;
        bottleControllerRef.numberOfColorsInBottle += numberOfColorsToTranfer;
        if (lineRenderer != null) lineRenderer.enabled = false;
        if (boilingSound != null) boilingSound.Stop();

        StartCoroutine(RotateBottlrBack());
    }

    IEnumerator RotateBottlrBack()
    {
        float t = 0f;
        float angleVlaue;
        float lastAngleValue = directionMultiplaier * rotationValues[rotationIndex];
        while(t < timeToRotate)
        {
            StartCoroutine(FixAmount());
            float lerpValue = t / timeToRotate;
            angleVlaue = Mathf.Lerp(directionMultiplaier * rotationValues[rotationIndex], 0f, lerpValue);
            transform.RotateAround(chosenRotationPoint.position, Vector3.forward, lastAngleValue - angleVlaue);
            if (bottleMaskSR != null)
                bottleMaskSR.material.SetFloat("_ScaleAndRotationMultiplaier", ScaleAndRotationMutiplaierCurve.Evaluate(angleVlaue));
            lastAngleValue = angleVlaue;
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        UpdateTopColorValue();
        angleVlaue = 0;
        transform.eulerAngles = new Vector3(0, 0, angleVlaue);
        if (bottleMaskSR != null)
            bottleMaskSR.material.SetFloat("_ScaleAndRotationMultiplaier", ScaleAndRotationMutiplaierCurve.Evaluate(angleVlaue));
        StartCoroutine(MoveBottleBack());
    }

    IEnumerator MoveBottleBack()
    {
        startPosition = transform.position;
        endPosition = originalPosition;
        float t2 = 0;
        while(t2 <= 1)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, t2);
            t2 += Time.deltaTime * 2;
            yield return new WaitForEndOfFrame();
        }
        transform.position = endPosition;
        if (GetComponent<SpriteRenderer>() != null) GetComponent<SpriteRenderer>().sortingOrder -= 2;
        if (bottleMaskSR != null) bottleMaskSR.sortingOrder -= 2;
        UnlockAll();
        StartCoroutine(LockBottle());
    }

    public int UpdateTopColorValue()
    {
        if(numberOfColorsInBottle != 0)
        {
            numberOfTopColorLayer = 1;
            topColorName = bottleColorNames[numberOfColorsInBottle - 1];
            topColor = gameController != null ? gameController.GetColorByName(topColorName) : Color.clear;
            for (int i = numberOfColorsInBottle - 2; i >= 0; i--)
            {
                if (bottleColorNames[i] == topColorName) numberOfTopColorLayer++;
                else break;
            }
            rotationIndex = Mathf.Clamp(3 - (numberOfColorsInBottle - numberOfTopColorLayer), 0, 3);
        }
        else
        {
            topColorName = "None";
            topColor = Color.clear;
            numberOfTopColorLayer = 0;
        }
        return numberOfTopColorLayer;
    }   

    public bool FillBottleCheck(string colorNameToCheck)
    {
        if(numberOfColorsInBottle == 0) return true;
        if(numberOfColorsInBottle == 4) return false;
        return topColorName == colorNameToCheck;
    }

    private void CalculateRotationIndex(int numberOfEmptyspacesInSecondBottle)
    {
        rotationIndex = Mathf.Clamp(3 - (numberOfColorsInBottle - Mathf.Min(numberOfEmptyspacesInSecondBottle, numberOfTopColorLayer)), 0, 3);
    }

    public void FillUp(float fillAmounToAdd)
    {
        if (bottleMaskSR != null)
            bottleMaskSR.material.SetFloat("_FillAmount", bottleMaskSR.material.GetFloat("_FillAmount") + fillAmounToAdd - 0.001f);
    }

    private void chosenRotationPointAndDirection()
    {
        if(transform.position.x > bottleControllerRef.transform.position.x)
        {
            chosenRotationPoint = leftRotationPoint;
            directionMultiplaier = -1.0f;
        }
        else
        {
            chosenRotationPoint = rightRotationPoint;
            directionMultiplaier = 1.0f;
        }
    }

    IEnumerator LockBottle()
    {
        yield return new WaitForEndOfFrame();
        if(numberOfTopColorLayer == 4 && numberOfColorsInBottle == 4)
        {
            if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;
            tag = "Locked Bottle";
        }
    }

    private void PlayBoilingSound() { if (boilingSound != null) boilingSound.Play(); }

    private void LockAll()
    {
        levelbottles = GameObject.FindGameObjectsWithTag("bottle");
        foreach (GameObject b in levelbottles) { if (b.GetComponent<Collider2D>() != null) b.GetComponent<Collider2D>().enabled = false; }
    }

    private void UnlockAll()
    {
        levelbottles = GameObject.FindGameObjectsWithTag("bottle");
        foreach (GameObject b in levelbottles) { if (b.GetComponent<Collider2D>() != null && b.tag != "Locked Bottle") b.GetComponent<Collider2D>().enabled = true; }
    }

    IEnumerator FixAmount()
    {
        yield return new WaitForEndOfFrame();
        if (bottleControllerRef == null || bottleControllerRef.bottleMaskSR == null) yield break;
        float fill = bottleControllerRef.bottleMaskSR.material.GetFloat("_FillAmount");
        if(fill > 0.3f) bottleControllerRef.bottleMaskSR.material.SetFloat("_FillAmount", 0.51f);
        else if(fill > -0.07f) bottleControllerRef.bottleMaskSR.material.SetFloat("_FillAmount", 0.195f);
        else if(fill > -0.385f) bottleControllerRef.bottleMaskSR.material.SetFloat("_FillAmount", -0.12f);
        else if(fill > -0.70f) bottleControllerRef.bottleMaskSR.material.SetFloat("_FillAmount", -0.435f);
    }
}
}