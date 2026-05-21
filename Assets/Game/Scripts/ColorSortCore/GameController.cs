using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ColorSortCore
{
public class GameController : MonoBehaviour
{
    public BottleController FirstBottle;
    public BottleController SecondBottle;
    public List<BottleController> bottles = new List<BottleController>();

    private bool allFull = false; 

    [Header("Colors")]
    [SerializeField] private Color redColor = Color.red;
    [SerializeField] private Color blueColor = Color.blue;
    [SerializeField] private Color greenColor = Color.green;
    [SerializeField] private Color yellowColor = Color.yellow;
    [SerializeField] private Color purpleColor = new Color(0.5f, 0, 0.5f);
    [SerializeField] private Color noneColor = Color.clear;

    public GameObject LevelCompleted;

    private float bottleUp = 0.3f; 
    private float bottleDown = -0.3f; 

    public Color GetColorByName(string colorName)
    {
        switch (colorName)
        {
            case "Red": return redColor;
            case "Blue": return blueColor;
            case "Green": return greenColor;
            case "Yellow": return yellowColor;
            case "Purple": return purpleColor;
            default: return noneColor;
        }
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x , mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if(hit.collider != null)
            {
                BottleController clickedBottle = hit.collider.GetComponent<BottleController>();
                if(clickedBottle != null)
                {
                    if(FirstBottle == null)
                    {
                        FirstBottle = clickedBottle;
                        if(FirstBottle.numberOfColorsInBottle != 0)
                        {
                            FirstBottle.transform.position = new Vector3(FirstBottle.transform.position.x, FirstBottle.transform.position.y + bottleUp, FirstBottle.transform.position.z);
                        }
                    }
                    else
                    {
                        if(FirstBottle == clickedBottle)
                        {
                            if(FirstBottle.numberOfColorsInBottle != 0)
                            {
                                FirstBottle.transform.position = new Vector3(FirstBottle.transform.position.x, FirstBottle.transform.position.y + bottleDown, FirstBottle.transform.position.z);
                            }
                            FirstBottle = null;
                        }
                        else
                        {
                            SecondBottle = clickedBottle;
                            FirstBottle.bottleControllerRef = SecondBottle;
                            FirstBottle.UpdateTopColorValue();
                            SecondBottle.UpdateTopColorValue();

                            if(SecondBottle.FillBottleCheck(FirstBottle.topColorName) == true)
                            {
                                FirstBottle.startColorTransfer();
                                FirstBottle = null;
                                SecondBottle = null;
                            }
                            else {
                                if(FirstBottle.numberOfColorsInBottle != 0)
                                {
                                    FirstBottle.transform.position = new Vector3(FirstBottle.transform.position.x, FirstBottle.transform.position.y + bottleDown, FirstBottle.transform.position.z);
                                }
                                FirstBottle = null;
                                SecondBottle = null;
                            }
                        }
                    }
                }
            }
            else 
            {      
                if(FirstBottle != null && FirstBottle.numberOfColorsInBottle != 0)
                {
                    FirstBottle.transform.position = new Vector3(FirstBottle.transform.position.x, FirstBottle.transform.position.y + bottleDown, FirstBottle.transform.position.z);
                }
               FirstBottle = null;
               SecondBottle = null;
            }
        }

        if(allFull == false) StartCoroutine(AllBottlesAreFull());
    }

    IEnumerator AllBottlesAreFull() 
    {
        if(bottles.Count > 0 && bottles.All( y => y.numberOfColorsInBottle == 0 || y.numberOfTopColorLayer == 4))
        {
            allFull = true;
            yield return new WaitForSeconds(1f);
            Win();
        }
    }

    private void Win()
    {
        if(allFull == true)
        {
            if(LevelCompleted != null && LevelCompleted.activeSelf == false) LevelCompleted.SetActive(true);
            Debug.Log("Level Solved!");
        }   
    }
}
}