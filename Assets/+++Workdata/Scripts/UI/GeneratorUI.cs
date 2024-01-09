using System;
using UnityEngine;
using UnityEngine.UI;

public class GeneratorUI : MonoBehaviour
{
    [Header("Acceleration")]
    [SerializeField] private AnimationCurve accelerationCurve;
    [SerializeField] private float acceleration = .375f;
    [SerializeField] private GameObject wheelOfFortune;

    private float finalAcc;
    private Slider generatorSlider;
    private float fillTime;
    private Player player;

    private void Awake()
    {
        player = FindObjectOfType<Player>();
    }

    private void OnEnable()
    {
        player.isInteracting = true;
    }

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
        finalAcc = acceleration * accelerationCurve.Evaluate(generatorSlider.value);
        
        fillTime += finalAcc * Time.deltaTime;

        generatorSlider.value = Mathf.PingPong(fillTime, generatorSlider.maxValue);
    }

    public void StartGeneratorEngine()
    {
        if (generatorSlider.value > 0.9f)
        {
            player.generatorIsActive = true;
            wheelOfFortune.SetActive(true);
            gameObject.SetActive(false);
        }
        else
        {
            fillTime = 0;
            generatorSlider.maxValue = 0;
            generatorSlider.maxValue = 1;
        }
    }

    private void OnDisable()
    {
        player.isInteracting = false;
        fillTime = 0;
        generatorSlider.maxValue = 0;
        generatorSlider.maxValue = 1;
    }
}
