using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TypingIntro : MonoBehaviour
{
    [Header("UI References")]
    public GameObject introPanel;
    public CanvasGroup panelCanvasGroup;
    public TextMeshProUGUI introText;
    // public TextMeshProUGUI introText; 

    public Button closeButton;

    [Header("Typewriter Settings")]
    [TextArea(3, 10)]
    public string fullText = "Chào mừng bạn đến với thế giới của chúng tôi. Cuộc hành trình bắt đầu từ đây...";
    public float typingSpeed = 0.05f;

    [Header("Audio Settings")]
    public AudioSource audioSource;     // AudioSource để phát âm thanh
    public AudioClip typingLoop;        // Âm thanh lặp (ví dụ tiếng gõ máy chữ)

    [Header("Fade Settings")]
    public float fadeDuration = 1f;

    void Start()
    {
        introPanel.SetActive(true);
        introText.text = "";
        closeButton.gameObject.SetActive(false);

        StartCoroutine(FadeIn());
        StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        // Bắt đầu phát âm thanh lặp
        if (typingLoop != null && audioSource != null)
        {
            audioSource.clip = typingLoop;
            audioSource.loop = true;
            audioSource.Play();
        }

        foreach (char c in fullText)
        {
            introText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        // Khi chạy hết chữ thì dừng âm thanh
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Hiện nút sau khi chữ chạy xong
        closeButton.gameObject.SetActive(true);
        closeButton.onClick.AddListener(() => StartCoroutine(FadeOut()));
    }

    IEnumerator FadeIn()
    {
        panelCanvasGroup.alpha = 0;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            panelCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
    }

    IEnumerator FadeOut()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            panelCanvasGroup.alpha = 1 - Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        introPanel.SetActive(false);
    }

}
