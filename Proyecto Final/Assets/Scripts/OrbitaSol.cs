using UnityEngine;

public class OrbitaSol : MonoBehaviour
{
    [Header("Configuración del Sol")]
    public Transform planetCenter;   // centro del planeta
    public float orbitRadius = 1000f; // distancia desde el planeta
    public float dayDuration = 120f;  // duración del ciclo día-noche (segundos)
    public Gradient lightColor;       // colores del amanecer, día, noche
    public AnimationCurve lightIntensity; // curva de intensidad

    private float time;

    void Start()
    {
        if (planetCenter == null)
            Debug.LogError("Asigna el centro del planeta (planetCenter)");
    }

    void Update()
    {
        if (!planetCenter) return;

        // Avanza el tiempo
        time += Time.deltaTime / dayDuration;
        if (time > 1) time = 0;

        // Rotación orbital (360° alrededor del planeta)
        float angle = time * 360f;
        Vector3 offset = Quaternion.Euler(angle, 0, 0) * Vector3.forward * orbitRadius;
        transform.position = planetCenter.position + offset;
        transform.LookAt(planetCenter.position); // apunta al planeta

        // Ajusta color e intensidad del Sol
        Light sun = GetComponent<Light>();
        if (sun)
        {
            sun.color = lightColor.Evaluate(time);
            sun.intensity = lightIntensity.Evaluate(time);
        }
    }
}
