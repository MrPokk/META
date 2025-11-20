using System;
using System.Collections.Generic;

public class QuestionService
{
    public event Action<QuestionPoint> OnQuestion;

    private readonly List<QuestionPoint> _questions = new();

    public void RegisterQuestion(QuestionPoint questionPoint)
    {
        _questions.Add(questionPoint);
    }

    public void UnregisterQuestion(QuestionPoint questionPoint)
    {
        _questions.Remove(questionPoint);
    }

    public IReadOnlyList<QuestionPoint> GetQuestions()
    {
        return _questions;
    }

    public void ExecuteQuestion(QuestionPoint questionPoint)
    {
        OnQuestion?.Invoke(questionPoint);
    }
}
