using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.SceneManagement;

public class DroneSpawner : MonoBehaviour
{
    public GameObject dronePrefab;
    public float animationTime = 2f;
    public float spawnRadius = 2f;

    public AudioClip explosionSound;

    public Transform kronusTarget;
    public Transform lyrionTarget;
    public Transform mystaraTarget;
    public Transform eclipsiaTarget;
    public Transform fioraTarget;

    public GameObject kronusExplosion;
    public GameObject lyrionExplosion;
    public GameObject mystaraExplosion;
    public GameObject eclipsiaExplosion;
    public GameObject fioraExplosion;

    IEnumerator Start()
    {
        int dronesKronus = PlayerPrefs.GetInt("spawn_kronus", 0);
        int dronesLyrion = PlayerPrefs.GetInt("spawn_lyrion", 0);
        int dronesMystara = PlayerPrefs.GetInt("spawn_mystara", 0);
        int dronesEclipsia = PlayerPrefs.GetInt("spawn_eclipsia", 0);
        int dronesFiora = PlayerPrefs.GetInt("spawn_fiora", 0);

        if (dronesKronus > 0)
            StartCoroutine(SpawnDrones(kronusTarget, dronesKronus, kronusExplosion, true));
        if (dronesLyrion > 0)
            StartCoroutine(SpawnDrones(lyrionTarget, dronesLyrion, lyrionExplosion, true));
        if (dronesMystara > 0)
            StartCoroutine(SpawnDrones(mystaraTarget, dronesMystara, mystaraExplosion, true));
        if (dronesEclipsia > 0)
            StartCoroutine(SpawnDrones(eclipsiaTarget, dronesEclipsia, eclipsiaExplosion, true));
        if (dronesFiora > 0)
            StartCoroutine(SpawnDrones(fioraTarget, dronesFiora, fioraExplosion, true));

        yield return new WaitForSeconds(animationTime + 3f);

        SceneManager.LoadScene("WaitingForAll");
    }

    IEnumerator SpawnDrones(Transform target, int count, GameObject explosionEffect, bool deactivateTargetAfter)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 start = Camera.main.ViewportToWorldPoint(new Vector3(Random.Range(0f, 1f), -0.1f, 10f));
            start.z = 0;
            GameObject drone = Instantiate(dronePrefab, start, Quaternion.identity);

            Vector3 end = target.position + Random.insideUnitSphere * spawnRadius;
            end.z = 0;

            float delay = Random.Range(0f, 0.5f);

            drone.transform.DOMove(end, animationTime)
                .SetEase(Ease.InOutCubic)
                .SetDelay(delay)
                .OnComplete(() =>
                {
                    if (explosionEffect != null)
                    {
                        Instantiate(explosionEffect, drone.transform.position, Quaternion.identity);
                    }
                    if (explosionSound != null)
                    {
                        AudioSource.PlayClipAtPoint(explosionSound, drone.transform.position);
                    }
                    Destroy(drone);
                });

            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(animationTime + 0.5f);

        if (deactivateTargetAfter && target != null)
        {
            target.gameObject.SetActive(false);
        }
    }
}
