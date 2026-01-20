using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Element 
{ 
    None, 
    Fire, 
    Cold, 
    Lightning, 
    Poison
}

public class Card : ScriptableObject
{
    public string cardName; 
    public int cardCost; 
    public Sprite cardBorder; 
    public Sprite cardIcon; 
    public string cardDescription;  
    public Element cardElement; 

}
