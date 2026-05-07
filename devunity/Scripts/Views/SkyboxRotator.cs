using UnityEngine;

namespace ChibitsLink.Views
{
    public class SkyboxRotator : MonoBehaviour
    {
        [Header("Rotation Settings")]
        public float rotationSpeed = 10f;
        
        private float _currentRotation = 0f;

        private void Update()
        {
            _currentRotation += rotationSpeed * Time.deltaTime;
            RenderSettings.skybox.SetFloat("_Rotation", _currentRotation);
        }
    }
}
