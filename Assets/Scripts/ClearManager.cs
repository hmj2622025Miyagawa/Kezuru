using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class ClearSceneManager : MonoBehaviour
{
	[SerializeField] private string gameSceneName = "GameScene";
	[SerializeField] private string titleSceneName = "TitleScene";
	[SerializeField] private AudioSource buttonAudio;
	[SerializeField] private TMP_Text clearTimeText;

	private bool isLoading = false;

	private void Start()
	{
		if (clearTimeText != null)
		{
			clearTimeText.text = "クリアタイム：" + GameTimer.ClearTime.ToString("F2") + "秒";
		}
	}

	public void RetryGame()
	{
		if (!isLoading)
		{
			StartCoroutine(LoadSceneAfterSound(gameSceneName));
		}
	}

	public void ReturnToTitle()
	{
		if (!isLoading)
		{
			StartCoroutine(LoadSceneAfterSound(titleSceneName));
		}
	}

	private IEnumerator LoadSceneAfterSound(string sceneName)
	{
		isLoading = true;

		if (buttonAudio != null && buttonAudio.clip != null)
		{
			buttonAudio.Play();
			yield return new WaitWhile(() => buttonAudio.isPlaying);
		}

		SceneManager.LoadScene(sceneName);
	}
}