using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class AutoScrollToBottom : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private bool autoScroll = true;
    [SerializeField] private float scrollSpeed = 10f;

    private RectTransform content;
    private bool isAtBottom = true;
    private float previousContentHeight;

    private void Awake()
    {
        if (scrollRect == null)
            scrollRect = GetComponent<ScrollRect>();

        content = scrollRect.content;
        previousContentHeight = content.rect.height;
    }

    private void Update()
    {
        // Проверяем, изменилась ли высота контента
        float currentHeight = content.rect.height;

        if (!Mathf.Approximately(currentHeight, previousContentHeight))
        {
            if (autoScroll && isAtBottom)
            {
                ScrollToBottom();
            }
            previousContentHeight = currentHeight;
        }

        // Проверяем текущую позицию скролла
        CheckScrollPosition();
    }

    // Метод для принудительной прокрутки до конца
    public void ScrollToBottom()
    {
        if (scrollRect.vertical)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }

        if (scrollRect.horizontal)
        {
            scrollRect.horizontalNormalizedPosition = 1f;
        }
    }

    // Плавная прокрутка до конца
    public void SmoothScrollToBottom()
    {
        if (scrollRect.vertical)
        {
            StartCoroutine(SmoothScrollVertical(0f));
        }

        if (scrollRect.horizontal)
        {
            StartCoroutine(SmoothScrollHorizontal(1f));
        }
    }

    private System.Collections.IEnumerator SmoothScrollVertical(float targetPosition)
    {
        while (!Mathf.Approximately(scrollRect.verticalNormalizedPosition, targetPosition))
        {
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(
                scrollRect.verticalNormalizedPosition,
                targetPosition,
                scrollSpeed * Time.deltaTime
            );
            yield return null;
        }

        scrollRect.verticalNormalizedPosition = targetPosition;
    }

    private System.Collections.IEnumerator SmoothScrollHorizontal(float targetPosition)
    {
        while (!Mathf.Approximately(scrollRect.horizontalNormalizedPosition, targetPosition))
        {
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(
                scrollRect.horizontalNormalizedPosition,
                targetPosition,
                scrollSpeed * Time.deltaTime
            );
            yield return null;
        }

        scrollRect.horizontalNormalizedPosition = targetPosition;
    }

    // Проверяем, находится ли пользователь внизу
    private void CheckScrollPosition()
    {
        if (scrollRect.vertical)
        {
            // 0 = низ, 1 = верх
            isAtBottom = scrollRect.verticalNormalizedPosition <= 0.05f;
        }
    }

    // Включает/выключает автоскролл
    public void SetAutoScroll(bool enable)
    {
        autoScroll = enable;

        if (enable)
        {
            ScrollToBottom();
        }
    }

    // Можно вызвать вручную при добавлении нового контента
    public void OnContentChanged()
    {
        if (autoScroll && isAtBottom)
        {
            ScrollToBottom();
        }
    }
}
