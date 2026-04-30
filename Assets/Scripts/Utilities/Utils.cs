using UnityEngine;

public class Utils
{
    public static Vector3 GetVectorFromAngle(float angle) {
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }
    
    public static float GetAngleFromVectorFloat(Vector3 vector) { 
        //verify
        return Mathf.Atan2(vector.y, vector.x) * (180f / Mathf.PI);
    }
    
}