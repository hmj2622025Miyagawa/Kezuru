using UnityEngine;

public class DefficultManager : MonoBehaviour
{
	public static int SelectedDifficulty = 1;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public static float GetStoneScale()
	{
		switch (SelectedDifficulty)
		{
			case 0:
				return 0.7f;

			case 2:
				return 1.5f;

			default:
				return 1.0f;
		}
	}
}
