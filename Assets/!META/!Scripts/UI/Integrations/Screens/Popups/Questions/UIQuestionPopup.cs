using System;
using Michsky.MUIP;
using UnityEngine;
using VContainer;

public class UIQuestionPopup : UIPopup
{
    private QuestionService _questionService;

    [SerializeField] private NotificationManager _notificationManager;

    private void Awake()
    {
        if (_notificationManager == null)
        {
            _notificationManager = GetComponentInChildren<NotificationManager>();
        }
    }

    [Inject]
    public void Construct(QuestionService questionService)
    {
        _questionService = questionService;
        _questionService.OnQuestion += OnQuestionExecuted;
    }

    private void OnQuestionExecuted(QuestionPoint questionPoint)
    {
        _notificationManager.title = questionPoint.Title;
        _notificationManager.description = questionPoint.Description;
        _notificationManager.UpdateUI();
        _notificationManager.Open();
    }

    public override void Open()
    {
        base.Open();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public override void Close()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        _questionService.OnQuestion -= OnQuestionExecuted;

        base.Close();
    }
}
