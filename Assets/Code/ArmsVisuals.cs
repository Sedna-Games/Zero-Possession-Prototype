using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmsVisuals : MonoBehaviour
{
    [SerializeField] AnimationCurve lerpCurve = null;
    [SerializeField] Transform lerpPoint = null;
    public float lerpSpeed = 1.0f;

    [Header("References")]
    [SerializeField] WeaponManager weaponManager = null;

    Vector3 _originalPos = Vector3.zero;
    bool _enableLerping = true;

    private void Start()
    {
        void OnBeginAttack()
        {
            _enableLerping = false;
        }
        void OnFinishAttack()
        {
            _enableLerping = true;
        }
        weaponManager.OnBeginAttack.AddListener(OnBeginAttack);
        weaponManager.OnFinishAttack.AddListener(OnFinishAttack);
    }

    private void OnEnable()
    {
        _originalPos = transform.localPosition;
        IEnumerator Lerp()
        {
            float x = 0.0f;
            while (true)
            {
                while (x < 1.0f && _enableLerping)
                {
                    yield return new WaitForEndOfFrame();
                    x += Time.deltaTime * lerpSpeed;
                    transform.localPosition = Vector3.Lerp(_originalPos, lerpPoint.localPosition, lerpCurve.Evaluate(x));
                }
                x = 1.0f;
                while (x > 0.0f && _enableLerping)
                {
                    yield return new WaitForEndOfFrame();
                    x -= Time.deltaTime * lerpSpeed;
                    transform.localPosition = Vector3.Lerp(_originalPos, lerpPoint.localPosition, lerpCurve.Evaluate(x));
                }
                x = 0.0f;
                while (!_enableLerping)
                {
                    yield return new WaitForEndOfFrame();
                    transform.localPosition = _originalPos;
                }
            }
        }
        StartCoroutine(Lerp());
    }

    public void SetLerping(bool set)
    {
        _enableLerping = set;
    }
}
