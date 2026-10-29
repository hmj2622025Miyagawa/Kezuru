using UnityEngine;

public class GameTimer : MonoBehaviour
{

	private static float startTime;
	public static float ClearTime { get; private set; }
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	public static void StartTimer()
	{
		startTime = Time.time;
		ClearTime = 0f;
	}

	public static void StopTimer()
	{
		ClearTime = Time.time - startTime;
	}
}
