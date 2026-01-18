using UnityEngine;

public class ShadowFollower : MonoBehaviour
{
    public enum FollowMode
    {
        Position,
        Scale,
        Both,
        ScaleOnlyKeepOffset
    }

    [Header("Follow Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private FollowMode followMode = FollowMode.Position;

    [Header("Additional Offsets")]
    [SerializeField] private Vector3 positionOffset = Vector3.zero;
    [SerializeField] private Vector3 scaleOffset = Vector3.one;

    [Header("Scale Mode Settings")]
    [SerializeField] private bool useLocalScale = true;
    [SerializeField] private bool applyInitialOffset = false;

    private Vector3 initialPositionOffset;
    private Vector3 initialLocalPosition;
    private Vector3 initialScale;
    private bool isInitialized = false;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        if (target == null)
        {
            Debug.LogWarning("Target is not assigned in ShadowFollower on " + gameObject.name);
            return;
        }

        initialLocalPosition = transform.localPosition;
        initialPositionOffset = transform.localPosition - target.localPosition;
        initialScale = transform.localScale;
        isInitialized = true;
    }

    void Update()
    {
        if (target == null || !isInitialized) return;

        switch (followMode)
        {
            case FollowMode.Position:
                UpdatePosition();
                break;

            case FollowMode.Scale:
                UpdateScale();
                break;

            case FollowMode.Both:
                UpdatePosition();
                UpdateScale();
                break;

            case FollowMode.ScaleOnlyKeepOffset:
                UpdateScale();
                break;
        }
    }

    void UpdatePosition()
    {
        if (applyInitialOffset)
        {
            transform.localPosition = target.localPosition + initialPositionOffset + positionOffset;
        }
        else
        {
            transform.localPosition = target.localPosition + positionOffset;
        }
    }

    void UpdateScale()
    {
        if (useLocalScale)
        {
            transform.localScale = Vector3.Scale(
                target.localScale,
                Vector3.Scale(initialScale, scaleOffset)
            );
        }
        else
        {
            transform.localScale = Vector3.Scale(
                target.lossyScale,
                Vector3.Scale(initialScale, scaleOffset)
            );
        }
    }

    [ContextMenu("Update Following")]
    public void ManualUpdate()
    {
        if (!isInitialized) Initialize();
        Update();
    }

    [ContextMenu("Save Current Offset")]
    public void SaveCurrentOffset()
    {
        if (target == null) return;

        initialPositionOffset = transform.localPosition - target.localPosition;
        initialLocalPosition = transform.localPosition;
        Debug.Log("Offset saved: " + initialPositionOffset);
    }

    [ContextMenu("Reset to Initial Offset")]
    public void ResetToInitialOffset()
    {
        if (target == null) return;

        transform.localPosition = initialLocalPosition;
        transform.localScale = initialScale;
        Debug.Log("Offset reset");
    }
}
