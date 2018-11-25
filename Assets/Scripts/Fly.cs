using UnityEngine;

public class Fly : MonoBehaviour {

    public Vector3 Velocity = new Vector3(0.1f, 0, 0);

    void Update ()
    {
        float dt = Time.deltaTime;
        transform.Translate(Velocity * dt);
    }
}
