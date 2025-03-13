using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Displays an animated scanning indicator text in the UI.
/// </summary>
public class ScanningIndicator : MonoBehaviour
{
    [SerializeField] private Text _scanningText;
    [SerializeField] private float _animationInterval = 0.5f;
    [SerializeField] private string _baseText = "Scanning for devices";
    [SerializeField] private int _maxDots = 3;
    
    private Coroutine _animationCoroutine;
    
    private void OnEnable()
    {
        // Start the animation when the object is enabled
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
        }
        _animationCoroutine = StartCoroutine(AnimateScanningText());
    }
    
    private void OnDisable()
    {
        // Stop the animation when the object is disabled
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
            _animationCoroutine = null;
        }
    }
    
    private IEnumerator AnimateScanningText()
    {
        int dotCount = 0;
        
        while (true)
        {
            string dots = new string('.', dotCount);
            _scanningText.text = $"{_baseText}{dots}";
            
            dotCount = (dotCount + 1) % (_maxDots + 1);
            
            yield return new WaitForSeconds(_animationInterval);
        }
    }
}