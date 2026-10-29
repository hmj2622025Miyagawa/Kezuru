using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class TitleSceneManager : MonoBehaviour
{
	[SerializeField] private string gameSceneName = "GameScene";
	[SerializeField] private AudioSource buttonAudio;
	[SerializeField] private TMP_Text difficultyText;

	void Start()
	{
		UpdateDifficultyText();
	}

	// EASYを選択
	public void SelectEasy()
	{
		DefficultManager.SelectedDifficulty = 0;
		UpdateDifficultyText();
		Debug.Log("難易度：EASY");
	}

	// NORMALを選択
	public void SelectNormal()
	{
		DefficultManager.SelectedDifficulty = 1;
		UpdateDifficultyText();
		Debug.Log("難易度：NORMAL");
	}

	// HARDを選択
	public void SelectHard()
	{
		DefficultManager.SelectedDifficulty = 2;
		UpdateDifficultyText();
		Debug.Log("難易度：HARD");
	}

	// 選択中の難易度を表示
	private void UpdateDifficultyText()
	{
		if (difficultyText == null)
			return;

		switch (DefficultManager.SelectedDifficulty)
		{
			case 0:
				difficultyText.text = "選択中：EASY";
				break;

			case 2:
				difficultyText.text = "選択中：HARD";
				break;

			default:
				difficultyText.text = "選択中：NORMAL";
				break;
		}
	}

	public void StartGame()
	{
		if (buttonAudio != null && buttonAudio.clip != null)
		{
			buttonAudio.Play();
			StartCoroutine(StartGameAfterSound());
		}
		else
		{
			SceneManager.LoadScene(gameSceneName);
		}
	}

	private IEnumerator StartGameAfterSound()
	{
		yield return new WaitWhile(() => buttonAudio.isPlaying);
		SceneManager.LoadScene(gameSceneName);
	}
}