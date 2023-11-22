using UnityEngine;
using UnityEngine.UI;

public class GeneratorUI : MonoBehaviour
{
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
        fillTime += acceleration * Time.deltaTime;
            if (generatorSlider.value >= 0.5)
            {
                acceleration = .5f;
                if (generatorSlider.value >= 0.75)
                {
                    acceleration = 1;
                }
            }

            generatorSlider.value = Mathf.PingPong(fillTime, generatorSlider.maxValue);
    }
}
