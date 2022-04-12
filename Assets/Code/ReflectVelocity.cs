using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//made because the bullets use trigger volumes
public class ReflectVelocity : MonoBehaviour
{
    //https://forum.unity.com/threads/object-reflecting-off-a-wall.74060/
    [SerializeField] new Rigidbody rigidbody = null;
    [SerializeField] List<string> tagsToLookFor = new List<string>();
    float _maxCooldown = 0.5f;
    float _cooldown = 0.0f;

    bool OnCooldown => _cooldown > 0.0f;
    private void OnEnable()
    {
        _cooldown = _maxCooldown;
        IEnumerator Cooldown()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                _cooldown -= Time.deltaTime;
            }
        }
        StartCoroutine(Cooldown());
    }

    public void Reflect()
    {
        RaycastHit hit;
        if (Physics.Raycast(rigidbody.position, rigidbody.velocity.normalized, out hit) && tagsToLookFor.Contains(hit.transform.tag))
        {
            var dir = (rigidbody.position + rigidbody.velocity) - rigidbody.position;
            dir = -dir.normalized * rigidbody.velocity.magnitude;
            rigidbody.velocity = dir;
            GetComponent<DamageDoer>().OverrideIgnoreTags();
        }

    }
}
