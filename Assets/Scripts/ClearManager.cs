using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearManager : MonoBehaviour
{

	[SerializeField] private string GameScene = "GameScene";
	[SerializeField] private string TitleScene = "TitleScene";

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	public void RetryGame()
	{
		SceneManager.LoadScene(GameScene);
	}

	public void ReturnToTitle()
	{
		SceneManager.LoadScene(TitleScene);
	}
}
