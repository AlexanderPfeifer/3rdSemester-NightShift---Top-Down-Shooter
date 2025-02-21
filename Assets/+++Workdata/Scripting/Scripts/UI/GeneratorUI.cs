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
    [SerializeField] private Image generatorFillImage;
    private float fillTime;

    private void OnEnable()
    {
        Player.Instance.isInteracting = true;
        EventSystem.current.SetSelectedGameObject(firstGeneratorSelected);
    }

    private void Update()
    {
        SliderFillOverTime();   
    }

    //Fills the slider over time and decreases it when maxed out
    private void SliderFillOverTime()
    {
        finalAcc = acceleration * accelerationCurve.Evaluate(generatorFillImage.fillAmount);
        
        fillTime += finalAcc * Time.deltaTime;

        generatorFillImage.fillAmount = Mathf.PingPong(fillTime, 1);
    }

    //Tries to start engine when button got clicked. When over 0.9f, can activate the ride
    public void StartGeneratorEngine()
    {
        AudioManager.Instance.Play("GeneratorButtonClickDown");

        if (generatorFillImage.fillAmount > 0.9f)
        {
            Player.Instance.generatorIsActive = true;
            gameObject.SetActive(false);
            Player.Instance.SearchInteractionObject(Player.Instance.generatorLayer).GetComponent<Generator>().SetFortuneWheel();
        }
        else
        {
            fillTime = 0;
            generatorFillImage.fillAmount = 0;
        }
    }

    public void GeneratorButtonUp()
    {
        AudioManager.Instance.Play("GeneratorButtonClickUp");
    }

    //When disabled, then resets every value
    private void OnDisable()
    {
        Player.Instance.isInteracting = false;
        fillTime = 0;
        generatorFillImage.fillAmount = 0;
    }
}
