using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lifetime : MonoBehaviour
{
    [SerializeField] float maxLifetime = 5.0f;
    float lifetime = 5.0f;
    // Start is called before the first frame update
    void OnEnable()
    {
        IEnumerator Life()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                lifetime -= Time.deltaTime;
                if (lifetime <= 0.0f)
                {
                    lifetime = maxLifetime;
                    gameObject.SetActive(false);
                }
            }
        }
        StartCoroutine(Life());
    }
    public void ResetLife()
    {
        lifetime = maxLifetime;
    }
}
