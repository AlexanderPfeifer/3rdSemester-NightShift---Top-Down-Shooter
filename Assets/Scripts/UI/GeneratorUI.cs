using UnityEngine;
using UnityEngine.UI;

public class GeneratorUI : MonoBehaviour
{
    [SerializeField] private AnimationCurve accelarationCurve;
    
    private Slider generatorSlider;

    private float fillTime;

    float acceleration = .375f;

    private void Start()
    {
        generatorSlider = GetComponentInChildren<Slider>();
    }

    private void Update()
    {
        RandomSliderFill();   
    }

    private void RandomSliderFill()
    {
        float finalAcc = acceleration * accelarationCurve.Evaluate(generatorSlider.value);
        
        fillTime += finalAcc * Time.deltaTime;

        generatorSlider.value = Mathf.PingPong(fillTime, generatorSlider.maxValue);
    }
}
