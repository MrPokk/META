using UnityEngine;
using VContainer;

public class UIQuestionPopup : UIPopup
{
    private QuestionService _questionService;

    [Inject]
    public void Construct(QuestionService questionService)
    {
        _questionService = questionService;
        _questionService.OnQuestion += OnQuestionExecuted;
    }

    private void OnQuestionExecuted(QuestionPoint questionPoint)
    {
        Close();
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
