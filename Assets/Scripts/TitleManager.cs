using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{

	[SerializeField] private string GameScene = "GameScene";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void StartGame()
	{
		SceneManager.LoadScene(GameScene);
	}

}
