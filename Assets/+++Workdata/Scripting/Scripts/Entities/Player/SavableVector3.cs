using UnityEngine;

[System.Serializable]
public struct SavableVector3
{
    public float x;
    public float y;
    public float z;

    public SavableVector3(Vector3 startValue)
    {
        x = startValue.x;
        y = startValue.y;
        z = startValue.z;
    }
    
    public void SetFromVector(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }

    public Vector3 GetVector3()
    {
        return new Vector3(x, y, z);
    }

    //this will automatically convert vector3 variables to a SavableVector3 variable (and vice versa).
    public static implicit operator SavableVector3(Vector3 origin) => new(origin);
    public static implicit operator Vector3(SavableVector3 origin) => origin.GetVector3();
}
