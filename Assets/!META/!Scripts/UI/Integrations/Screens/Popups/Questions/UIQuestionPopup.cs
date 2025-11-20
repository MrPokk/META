using Michsky.UI.Heat;
using UnityEngine;
using VContainer;

public class UIQuestionPopup : UIPopup
{
    [SerializeField] private ButtonManager _buttonFloorPrefab;
    [SerializeField] private Transform _buttonContainer;

    private QuestionService _questionService;

    [Inject]
    public void Construct(QuestionService questionService)
    {
        _questionService = questionService;
        _questionService.OnQuestion += OnQuestionExecuted;

        CreateButtons();
    }

    private void OnQuestionExecuted(QuestionPoint questionPoint)
    {
        Close();
    //  SceneNetworkProvider.ChangeScene(questionPoint.SceneType);
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

    private void CreateButtons()
    {
        if (!_buttonContainer || !_buttonFloorPrefab || _questionService == null) return;

        foreach (var questionPoint in _questionService.GetQuestions())
        {
            var buttonObj = Instantiate(_buttonFloorPrefab, _buttonContainer);

            buttonObj.onClick.AddListener(() => _questionService.ExecuteQuestion(questionPoint));
        }
    }
}
