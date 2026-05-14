using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FadingHint : MonoBehaviour
{
    [SerializeField] private float displayTime = 2f;
    [SerializeField] private float fadeTime = 1f;
    [SerializeField] private Image[] images;
    [SerializeField] private TextMeshProUGUI[] texts;

    private float timer;

    private void Start()
    {
        timer = displayTime +  fadeTime;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        
        if (timer <= 0f)
        {
            gameObject.SetActive(false);
            return;
        }

        if (timer <= fadeTime)
        {
            float alpha = timer / fadeTime;
            foreach (var image in images)
            {
                Color c = image.color;
                c.a = alpha;
                image.color = c;
            }

            foreach (var text in texts)
            {
                Color c = text.color;
                c.a = alpha;
                text.color = c;
            }
        }
    }
}
