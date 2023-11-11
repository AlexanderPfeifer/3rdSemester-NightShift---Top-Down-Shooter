using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Roulette : MonoBehaviour
{
    [SerializeField] private float minSpinPower, maxSpinPower;
    [SerializeField] private float minStopPower, maxStopPower;
    [SerializeField] private float maxAngularVelocity = 0;
    private const float StartingAngle = 22;

    [SerializeField] private int firstPrize;
    [SerializeField] private int secondPrize;
    [SerializeField] private int thirdPrize;
    [SerializeField] private int fourthPrize;
    [SerializeField] private int fifthPrize;
    [SerializeField] private int sixthPrize;
    [SerializeField] private int seventhPrize;
    [SerializeField] private int eighthPrize;

    [SerializeField] private List<int> prizeList;

    private Rigidbody2D rb;
    private bool isRotating = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private float t;

    private void Update()
    {
        FortuneWheelUpdate();
    }

    private void FortuneWheelUpdate()
    {
        var angularVelocity = rb.angularVelocity;

        if (rb.angularVelocity > 0)
        {
            angularVelocity -= Random.Range(minStopPower, maxStopPower) * Time.deltaTime;
            rb.angularVelocity = angularVelocity;

            rb.angularVelocity = Mathf.Clamp(angularVelocity, 0, maxAngularVelocity);
        }

        if (angularVelocity != 0 || !isRotating) 
            return;
        
        t += 1 * Time.deltaTime;
        
        if (t !>= 0.5f) 
            return;
        
        GetRewardPosition();
        isRotating = false;
        t = 0;
    }
    
    public void SpinWheel()
    {
        if (isRotating) 
            return;
        
        rb.AddTorque(Random.Range(minSpinPower, maxSpinPower));
        isRotating = true;
    }

    private void GetRewardPosition()
    {
        var rotationAngle = transform.eulerAngles.z;

        switch (rotationAngle)
        {
            case > 0+StartingAngle and <= 45+22:
                //GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 45);
                GetPrize(firstPrize);
                break;
            case > 45+StartingAngle and <= 90+22:
                //GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 90);
                GetPrize(secondPrize);
                break;
            case > 90+StartingAngle and <= 135+22:
                //GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 135);
                GetPrize(thirdPrize);
                break;
            case > 135+StartingAngle and <= 180+22:
                //GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 180);
                GetPrize(fourthPrize);
                break;
            case > 180+StartingAngle and <= 225+22:
                //GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 225);
                GetPrize(fifthPrize);
                break;
            case > 225+StartingAngle and <= 270+22:
                //GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 270);
                GetPrize(sixthPrize);
                break;
            case > 270+StartingAngle and <= 315+22:
                //GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 315);
                GetPrize(seventhPrize);
                break;
            case > 315+StartingAngle and <= 360+22:
                //GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 0);
                GetPrize(eighthPrize);
                break;
        }
    }

    private static void GetPrize(int score)
    {
        Debug.Log(score);
    }
}
