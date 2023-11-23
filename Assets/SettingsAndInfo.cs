using System;
using System.Globalization;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsAndInfo : MonoBehaviour
{
    [SerializeField] private Slider _minShipCount;
    [SerializeField] private Slider _maxShipCount;
    [SerializeField] private Slider _shipSpeed;
    [SerializeField] private Button _toggleBermudianTriangle;
    
    [SerializeField] private TMP_Text _textMagazine;
    [SerializeField] private TMP_Text _exceptionMessage;
    [SerializeField] private TMP_Text _timerLabel;
    [SerializeField] private BermudianTriangle _bermudianTriangle;
    
    public int MinShipCount => (int)_minShipCount.value;
    public int MaxShipCount => (int)_maxShipCount.value;
    public float ShipSpeed => _shipSpeed.value;
    
    //Displays a timer on the screen
    void Update()
    {
        _timerLabel.text = Math.Round(Time.unscaledTime).ToString(CultureInfo.InvariantCulture);
    }
    
    //Subscribing button _toggleBermudian  on ToggleBermudianTriangle method after click
    private void OnEnable()
    {
        _toggleBermudianTriangle.onClick.AddListener(ToggleBermudianTriangle);
    }    
    
    //Subscribing button _toggleBermudian  on ToggleBermudianTriangle method after click
    private void OnDisable()
    {
        _toggleBermudianTriangle.onClick.RemoveListener(ToggleBermudianTriangle);
    }

    //Show message on the screen
    public async void ShowErrorMessage(string message)
    {
        _exceptionMessage.text = message;
        
        await UniTask.Delay(3000);
        
        _exceptionMessage.text = string.Empty;
    }

    //Adding message about enter/exit bermudian triangle
    public void AddTextToMagazine(string text)
    {
       _textMagazine.text += $"Ship #{text}: {Math.Round(Time.unscaledTime, 2).ToString(CultureInfo.InvariantCulture)} \r\n";
    }
    
    //Turn on/off bermudian triangle in lake
    private void ToggleBermudianTriangle()
    {
        _bermudianTriangle.gameObject.SetActive(!_bermudianTriangle.gameObject.activeSelf);
    }
}
