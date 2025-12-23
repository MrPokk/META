using UnityEngine;
using VContainer;
using Gley.Localization;

public class QuestionPoint : LocalizedUIElement
{
    private QuestionService _questionService;

    public string Title => GetLocalizedTitle();
    public string Description => GetLocalizedDescription();

    [Inject]
    public void Construct(QuestionService questionService)
    {
        _questionService = questionService;
    }

    protected override void OnValidate()
    {
        _useMultipleWord = true;
        if (_wordIDs.Count >= 2)
        {
            return;
        }
        _wordIDs.Add(WordIDs.EnterID);
        _wordIDs.Add(WordIDs.ExitID);
        base.OnValidate();
    }

    protected override void InitializeComponents()
    {
        _questionService?.RegisterQuestion(this);
        UpdateCurrentValues();
    }

    private void OnDestroy()
    {
        _questionService?.UnregisterQuestion(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IUsingQuestions>(out var questions))
        {
            questions.EnterQuestion(this);
            _questionService.ExecuteQuestion(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IUsingQuestions>(out var questions))
        {
            questions.ExitQuestion(this);
        }
    }

    public override void SetText(string text)
    {
        UpdateCurrentValues();
    }

    private string GetLocalizedTitle()
    {
        return GetLocalizedTextSingle(0);
    }

    private string GetLocalizedDescription()
    {
        return GetLocalizedTextSingle(1);
    }

    private void UpdateCurrentValues()
    {
        GetLocalizedTitle();
        GetLocalizedDescription();
    }
}
