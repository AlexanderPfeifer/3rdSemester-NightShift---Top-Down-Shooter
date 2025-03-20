using System.Collections;
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

    [Header("Fill")]
    [SerializeField] private float activateGeneratorFillAmount;
    [SerializeField] private Image generatorFillImage;
    [SerializeField] private float reduceFillSpeedMultiplier;
    private float fillTime;
    private bool resetDone = true;
    
    private void OnEnable()
    {
        PlayerBehaviour.Instance.isInteracting = true;
        EventSystem.current.SetSelectedGameObject(firstGeneratorSelected);
    }

    private void Update()
    {
        SliderFillOverTime();   
    }

    private void SliderFillOverTime()
    {
        if (resetDone)
        {
            finalAcc = acceleration * accelerationCurve.Evaluate(generatorFillImage.fillAmount);
            fillTime += finalAcc * Time.deltaTime;

            generatorFillImage.fillAmount = Mathf.PingPong(fillTime, 1);
        }
    }

    public void StartGeneratorEngine()
    {
        AudioManager.Instance.Play("GeneratorButtonClickDown");

        if (generatorFillImage.fillAmount > activateGeneratorFillAmount)
        {
            gameObject.SetActive(false);
            if (PlayerBehaviour.Instance.GetInteractionObjectInRange(PlayerBehaviour.Instance.generatorLayer, out Collider2D _generator))
            {            
                _generator.GetComponent<Generator>().SetUpFightArena();
            }
        }
        else
        {
            StartCoroutine(SmoothlyReduceFill(1f)); // Adjust duration as needed
        }
    }
    
    IEnumerator SmoothlyReduceFill(float duration)
    {
        resetDone = false;

        float _startFillTime = fillTime;
        float _startFillAmount = generatorFillImage.fillAmount;
        float _elapsedTime = 0f;

        while (_elapsedTime < duration)
        {
            _elapsedTime += Time.deltaTime * reduceFillSpeedMultiplier;
            float _t = _elapsedTime / duration; 

            fillTime = Mathf.Lerp(_startFillTime, 0, _t);
            generatorFillImage.fillAmount = Mathf.Lerp(_startFillAmount, 0, _t);

            yield return null; 
        }

        fillTime = 0;
        generatorFillImage.fillAmount = 0;
        resetDone = true;
    }

    public void GeneratorButtonUp()
    {
        AudioManager.Instance.Play("GeneratorButtonClickUp");
    }

    private void OnDisable()
    {
        PlayerBehaviour.Instance.isInteracting = false;
        fillTime = 0;
        generatorFillImage.fillAmount = 0;
    }
}
