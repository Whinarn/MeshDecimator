#region License
/*
MIT License

Copyright(c) 2017-2018 Mattias Edlund

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion

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
