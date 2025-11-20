using UnityEngine;
using VContainer;

public class QuestionPoint : MonoBehaviour
{
    private QuestionService _questionService;

    [Inject]
    public void Construct(QuestionService questionService)
    {
        _questionService = questionService;
    }

    private void Start()
    {
        _questionService?.RegisterQuestion(this);
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
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IUsingQuestions>(out var questions))
        {
            questions.ExitQuestion(this);
        }
    }
}
