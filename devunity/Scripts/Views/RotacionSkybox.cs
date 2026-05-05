using UnityEngine;

public class RotacionSkybox : MonoBehaviour
{
    public float velocidadRotacion = 10f;
    private float rotacionActual = 0f;

    void Update()
    {
        rotacionActual += velocidadRotacion * Time.deltaTime;
        RenderSettings.skybox.SetFloat("_Rotation", rotacionActual);
    }
}
