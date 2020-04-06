using UnityEngine;

public class Fly : MonoBehaviour
{
    [SerializeField]
    private float _speed = 2f;

    [SerializeField]
    private float _rotation = 0f;

    public float Speed
    {
        set
        {
            if (_speed != value)
            {
                _speed = value;
            }
        }
    }
    
    public float Rotation
    {
        set
        {
            if (_rotation != value)
            {
                _rotation = value;
                transform.rotation = transform.rotation * Quaternion.Euler(0, 0, _rotation);
            }
        }
    }

    private void OnEnable()
    {
        var rigidbody = GetComponent<Rigidbody2D>();
        transform.rotation = transform.rotation * Quaternion.Euler(0, 0, _rotation);
        rigidbody.velocity =  transform.right * _speed;        
    }

    private void Update()
    {
        var rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.velocity =  transform.right * _speed;
    }
}