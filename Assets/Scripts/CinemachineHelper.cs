using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public class CinemachineHelper : MonoBehaviour {
    [SerializeField] CinemachineStoryboard storyboard;
    [SerializeField] float timeToFade = 5f;
    [SerializeField] float additionalWaitTime = 2f;
    [SerializeField] UnityEvent fadeInStartEvents;
    [SerializeField] UnityEvent fadeInEndEvents;
    [SerializeField] UnityEvent fadeInAfterEvents;
    [SerializeField] UnityEvent fadeOutStartEvents;
    [SerializeField] UnityEvent fadeOutEndEvents;

    private void OnEnable() {
        fadeIn();
    }

    public void fadeIn() {
        StartCoroutine(FadeIn());
    }
    public void fadeOut() {
        StartCoroutine(FadeOut());
    }
    public void moveCamera(Transform target) {
        StartCoroutine(MoveCamera(target));
    }
    public void teleportCamera(Transform target) {
        transform.position = target.position;
        transform.rotation = target.rotation;
    }

    IEnumerator FadeIn() {
        fadeInStartEvents.Invoke();
        while(storyboard.m_Alpha != 0f) {
            storyboard.m_Alpha = Mathf.MoveTowards(storyboard.m_Alpha, 0f, Time.fixedDeltaTime / timeToFade);
            yield return new WaitForFixedUpdate();
        }
        fadeInEndEvents.Invoke();
        yield return new WaitForSeconds(additionalWaitTime);
        fadeInAfterEvents.Invoke();
    }
    IEnumerator FadeOut() {
        fadeOutStartEvents.Invoke();
        while(storyboard.m_Alpha != 1f) {
            storyboard.m_Alpha = Mathf.MoveTowards(storyboard.m_Alpha, 1f, Time.fixedDeltaTime / timeToFade);
            yield return new WaitForFixedUpdate();
        }
        fadeOutEndEvents.Invoke();
    }
    IEnumerator MoveCamera(Transform target) {
        while((transform.position - target.position).magnitude > 0.1f) {
            transform.position = Vector3.MoveTowards(transform.position, target.position, Vector3.Magnitude(target.position - transform.position) * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
    }

}
