using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GeneratorUI : MonoBehaviour
{
    [Header("Acceleration")]
    [SerializeField] private AnimationCurve accelerationCurve;
    [SerializeField] private float acceleration = .375f;
    [SerializeField] private GameObject firstGeneratorSelected;

    private float finalAcc;
    private Slider generatorSlider;
    private float fillTime;

    private void OnEnable()
    {
        Player.Instance.isInteracting = true;
        EventSystem.current.SetSelectedGameObject(firstGeneratorSelected);
    }

    private void Start()
    {
        generatorSlider = GetComponentInChildren<Slider>();
    }

    private void Update()
    {
        SliderFillOverTime();   
    }

    //Fills the slider over time and decreases it when maxed out
    private void SliderFillOverTime()
    {
        finalAcc = acceleration * accelerationCurve.Evaluate(generatorSlider.value);
        
        fillTime += finalAcc * Time.deltaTime;

        generatorSlider.value = Mathf.PingPong(fillTime, generatorSlider.maxValue);
    }

    //Tries to start engine when button got clicked. When over 0.9f, can activate the ride
    public void StartGeneratorEngine()
    {
        AudioManager.Instance.Play("GeneratorButtonClick");

        if (generatorSlider.value > 0.9f)
        {
            Player.Instance.generatorIsActive = true;
            gameObject.SetActive(false);
            Player.Instance.SearchInteractionObject(Player.Instance.generatorLayer).GetComponent<Generator>().SetFortuneWheel();
        }
        else
        {
            fillTime = 0;
            generatorSlider.maxValue = 0;
            generatorSlider.maxValue = 1;
        }
    }

    //When disabled, then resets every value
    private void OnDisable()
    {
        Player.Instance.isInteracting = false;
        fillTime = 0;
        generatorSlider.maxValue = 0;
        generatorSlider.maxValue = 1;
    }
}
