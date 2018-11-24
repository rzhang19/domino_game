using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BH
{
    /// <summary>
    /// Draws the selection rectangle and returns a list of the selected dominos, if any.
    /// Code heavily based off of https://www.habrador.com/tutorials/select-units-within-rectangle/.
    /// </summary>
    public class SelectionRectController : MonoBehaviour
    {
        //The selection rect we draw when we drag the mouse to select units
        public RectTransform selectionRectTrans;

        //To determine if we are clicking with left mouse or holding down left mouse
        bool isClicking = false;
        bool isHoldingDown = false;
        bool startedTimer = false;
        float delay = 0.1f;
        float clickTime = 0f;

        //The start and end coordinates of the rect we are making
        Vector3 rectStartPos;
        Vector3 rectEndPos;

        //If it was possible to create a rect
        bool hasCreatedRect;

        //The selection rect's 4 corner positions
        Vector3 TL, TR, BL, BR;

        void Awake()
        {
            selectionRectTrans.gameObject.SetActive(false);
        }

        /// <summary>
        /// Attempts to draw the selection rectangle by clicking and dragging a button of the client's choice. 
        /// Detects if any Selectables are within the rectangle.
        /// <param name="allUnits">List of all Selectables currently in the map.</param>
        /// <param name="mousePos">The current mouse position. Determines rectangle boundaries.</param>
        /// <param name="mouseDown">True if the desired mouse click is down.</param>
        /// <param name="mouseUp">True if the desired mouse click is up.</param>
        /// </summary>
        /// <returns>
        ///     A <c>List&lt;Selectable&gt;</c> representing the selectables within the rectangle. Empty if no such selectables.
        /// </returns>
        public List<Selectable> AttemptMassSelection(List<Selectable> allUnits, Vector3 mousePos, bool mouseDown, bool mouseUp)
        {
            List<Selectable> selectedUnits = new List<Selectable>();

            //Click the mouse button
            if (!startedTimer && mouseDown)
            {
                clickTime = Time.time;
                rectStartPos = mousePos;
                startedTimer = true;
            }
            //Release the mouse button
            if (startedTimer && mouseUp)
            {   
                //Deactivate the rect selection image
                selectionRectTrans.gameObject.SetActive(false);
                
                //Detect single click. Not using right now
                if (Time.time - clickTime <= delay)
                {
                    isClicking = true;
                }

                // Select units within the created rectangle
                if (hasCreatedRect)
                {
                    hasCreatedRect = false;

                    //Clear the list with the selected units
                    selectedUnits.Clear();

                    //Select the units
                    foreach (Selectable currentUnit in allUnits)
                    {
                        //Is this unit within the rect
                        if (IsWithinPolygon(currentUnit.transform.position))
                        {
                            selectedUnits.Add(currentUnit);
                        }
                    }
                }

                // Reset everything
                isHoldingDown = false;
                isClicking = false;
                startedTimer = false;
            }
            
            //Holding down the mouse button
            if (startedTimer && !isHoldingDown && Time.time - clickTime > delay)
            {
                isHoldingDown = true;
            }

            //Drag the mouse to select all units within the rect
            if (isHoldingDown)
            {
                //Activate the rect selection image
                if (!selectionRectTrans.gameObject.activeInHierarchy)
                {
                    selectionRectTrans.gameObject.SetActive(true);
                }

                //Get the latest coordinate of the rect
                rectEndPos = mousePos;

                //Display the selection rect with a GUI image
                DisplayRect();

                /*
                //Highlight the units within the selection rect, but don't select the units
                for (int i = 0; i < allUnits.Length; i++)
                {
                    GameObject currentUnit = allUnits[i];

                    //Is this unit within the rect
                    if (IsWithinPolygon(currentUnit.transform.position))
                    {
                        currentUnit.GetComponent<MeshRenderer>().material = highlightMaterial;
                    }
                    //Otherwise deactivate
                    else
                    {
                        currentUnit.GetComponent<MeshRenderer>().material = normalMaterial;
                    }
                }
                */
            }

            /*
            //Clicking once should dismiss rectangle selection
            if (isClicking)
            {
                //Deselect all units
                for (int i = 0; i < selectedUnits.Count; i++)
                {
                    selectedUnits[i].GetComponent<MeshRenderer>().material = normalMaterial;
                }

                //Clear the list with selected units
                selectedUnits.Clear();

                selectionRectTrans.gameObject.SetActive(false);
            }
            */
            return selectedUnits;
        }

        //Is a unit within a polygon determined by 4 corners?
        bool IsWithinPolygon(Vector3 unitPos)
        {
            bool isWithinPolygon = false;
            Vector3 newTL = Camera.main.WorldToViewportPoint(TL);
            Vector3 newBL = Camera.main.WorldToViewportPoint(BL);
            Vector3 newTR = Camera.main.WorldToViewportPoint(TR);
            Vector3 targetPos = Camera.main.WorldToViewportPoint(unitPos);

            Rect selectRect = new Rect(newTL.x, newTL.y, newTR.x-newTL.x, newBL.y-newTL.y);
            if (selectRect.Contains(targetPos, true))
            {
                isWithinPolygon = true;
            }
            return isWithinPolygon;
        }

        //Display the selection with a GUI rect
        void DisplayRect()
        {
            //The start position of the rect is in 3d space, or the first coordinate will move
            //as we move the camera which is not what we want
            Vector3 rectStartScreen = rectStartPos;

            rectStartScreen.z = 0f;

            //Change the size of the rect
            float sizeX = Mathf.Abs(rectStartScreen.x - rectEndPos.x);
            float sizeY = Mathf.Abs(rectStartScreen.y - rectEndPos.y);

            //Set the size of the rect
            selectionRectTrans.sizeDelta = new Vector2(sizeX, sizeY);

            //Get the middle position of the rect
            Vector3 middle = (rectStartScreen + rectEndPos) / 2f;

            //Set the middle position of the GUI rect
            selectionRectTrans.position = middle;

            //The problem is that the corners in the 2d rect is not the same as in 3d space
            //To get corners, we have to fire a ray from the screen
            //We have 2 of the corner positions, but we don't know which,  
            //so we can figure it out or fire 4 raycasts
            TL = new Vector3(middle.x - sizeX / 2f, middle.y + sizeY / 2f, 0f);
            TR = new Vector3(middle.x + sizeX / 2f, middle.y + sizeY / 2f, 0f);
            BL = new Vector3(middle.x - sizeX / 2f, middle.y - sizeY / 2f, 0f);
            BR = new Vector3(middle.x + sizeX / 2f, middle.y - sizeY / 2f, 0f);

            //From screen to world
            RaycastHit hit;
            var layermask = (1 << 9) | (1 << 10);
            int i = 0;
            //Fire ray from camera
            if (Physics.Raycast(Camera.main.ScreenPointToRay(TL), out hit, Mathf.Infinity, layermask))
            {
                TL = hit.point;
                i++;
            }
            if (Physics.Raycast(Camera.main.ScreenPointToRay(TR), out hit, Mathf.Infinity, layermask))
            {
                TR = hit.point;
                i++;
            }
            if (Physics.Raycast(Camera.main.ScreenPointToRay(BL), out hit, Mathf.Infinity, layermask))
            {
                BL = hit.point;
                i++;
            }
            if (Physics.Raycast(Camera.main.ScreenPointToRay(BR), out hit, 200f, layermask))
            {
                BR = hit.point;
                i++;
            }

            //Finding 4 points means we created a rect
            hasCreatedRect = i == 4? true : false;
        }
    }
}