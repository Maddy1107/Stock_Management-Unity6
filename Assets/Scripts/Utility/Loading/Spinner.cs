using UnityEngine;

public class UISpinner : MonoBehaviour
{
    [SerializeField] private float speed = 180f; // degrees per second

    private void Update()
    {
        transform.Rotate(0f, 0f, -speed * Time.deltaTime);
    }
}
