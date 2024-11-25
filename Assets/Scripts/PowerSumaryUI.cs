using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class PowerSummaryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI powerGenerationText;
    [SerializeField] private TextMeshProUGUI powerConsumptionText;
    [SerializeField] private TextMeshProUGUI powerBalanceText;
    
    public void UpdatePowerDisplay(float totalGeneration, float totalConsumption)
    {
        float balance = totalGeneration - totalConsumption;
        
        powerGenerationText.text = $"{totalGeneration:F1} MW";
        powerConsumptionText.text = $"{totalConsumption:F1} MW";
        powerBalanceText.text = $"{balance:F1} MW";
        
        // Optional: Color the balance text based on positive/negative
        powerBalanceText.color = balance >= 0 ? Color.green : Color.red;
    }
}

