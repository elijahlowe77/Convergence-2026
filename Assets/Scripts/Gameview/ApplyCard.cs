using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ApplyCard : MonoBehaviour
{ 
    public Card CardSO;  
    public GameObject CardBorder; 
    public GameObject CardIcon;  
    public GameObject Cost; 
    public GameObject Description; 
    public GameObject Element; 
    public GameObject CardName; 
    // Start is called before the first frame update
    void Start()
    {
        CardBorder.GetComponent<Image>().sprite = CardSO.cardBorder; 
        CardIcon.GetComponent<Image>().sprite = CardSO.cardIcon; 
        Cost.GetComponent<TextMeshProUGUI>().text = CardSO.cardCost.ToString(); 
        Description.GetComponent<TextMeshProUGUI>().text = CardSO.cardDescription; 
        Element.GetComponent<TextMeshProUGUI>().text = CardSO.cardElement.ToString(); 
        CardName.GetComponent<TextMeshProUGUI>().text = CardSO.cardName; 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
