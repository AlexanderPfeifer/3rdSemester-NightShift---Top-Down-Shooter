using System.Collections.Generic;
using UnityEngine;

public class RouletteUI : MonoBehaviour
{
    [SerializeField] private float minSpinPower, maxSpinPower;
    [SerializeField] private float minStopPower, maxStopPower;
    [SerializeField] private float maxAngularVelocity = 1440;

    [SerializeField] private List<int> prizeList;

    private Rigidbody2D rb;

    private bool canGetPrize;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        FortuneWheelUpdate();
    }

    private void FortuneWheelUpdate()
    {
        var angularVelocity = rb.angularVelocity;

        if (angularVelocity > 0)
        {
            angularVelocity -= Random.Range(minStopPower, maxStopPower) * Time.deltaTime;
            if (angularVelocity < 5)
            {
                angularVelocity = 0;
            }
            rb.angularVelocity = angularVelocity;

            rb.angularVelocity = Mathf.Clamp(angularVelocity, 0, maxAngularVelocity);

            canGetPrize = true;
        }

        if (angularVelocity > 0 || !canGetPrize) 
            return;
        
        GetRewardPosition();
    }
    
    public void SpinWheel()
    {
        if (rb.angularVelocity > 0) 
            return;
        
        rb.AddTorque(Random.Range(minSpinPower, maxSpinPower));
    }

    private void GetRewardPosition()
    {
        var rotationAngle = transform.eulerAngles.z;

        switch (rotationAngle)
        {
            case > 0 and <= 45:
                //GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 45);
                GetPrize(prizeList[0]);
                break;
            case > 45 and <= 90:
                //GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 90);
                GetPrize(prizeList[1]);
                break;
            case > 90 and <= 135:
                //GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 135);
                GetPrize(prizeList[2]);
                break;
            case > 135 and <= 180:
                //GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 180);
                GetPrize(prizeList[3]);
                break;
            case > 180 and <= 225:
                //GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 225);
                GetPrize(prizeList[4]);
                break;
            case > 225 and <= 270:
                //GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 270);
                GetPrize(prizeList[5]);
                break;
            case > 270 and <= 315:
                //GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 315);
                GetPrize(prizeList[6]);
                break;
            case > 315 and <= 360:
                //GetComponent<RectTransform>().eulerAngles = new Vector3(0, 0, 0);
                GetPrize(prizeList[7]);
                break;
        }
    }

    private void GetPrize(int score)
    {
        Debug.Log(score);
        canGetPrize = false;
    }
}
