using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tooltip : MonoBehaviour
{
    public string message;
    private void OnMouseEnter(){
        try
        {
            TooltipManager.instance.SetToolTip(message);
        }
        catch (Exception e)
        {

        }
        
    }
    private void OnMouseExit(){
        try
        {
            TooltipManager.instance.HideToolTip();
        }
        catch (Exception e)
        {

        }
        

    }
}
