using UnityEngine;

public class ShowExample : MonoBehaviour
{
    [SerializeField]
    private Transform cameraTransform = null;
    [SerializeField]
    private Transform targetTransform = null;

    [SerializeField]
    private float cameraAngleTime = 1f;
    [SerializeField]
    private float cameraMinDistance = 3f;
    [SerializeField]
    private float cameraMaxDistance = 30f;
    [SerializeField]
    private float cameraDistanceTime = 1f;
    [SerializeField]
    private float cameraSwayHeight = 2f;
    [SerializeField]
    private float cameraSwayTime = 1f;

    private void Update()
    {
        float angle = Mathf.Repeat(Time.time / cameraAngleTime, Mathf.PI * 2f);

        float distanceT = Mathf.Max(Mathf.Sin(Time.time / cameraDistanceTime), 0f);
        float distance = Mathf.Lerp(cameraMinDistance, cameraMaxDistance, distanceT);

        float altitude = Mathf.Cos(Time.time / cameraSwayTime) * cameraSwayHeight * distance;

        cameraTransform.position = targetTransform.position +
            new Vector3(Mathf.Cos(angle) * distance, altitude, Mathf.Sin(angle) * distance);

        cameraTransform.rotation = Quaternion.LookRotation(targetTransform.position - cameraTransform.position);
    }
}
