using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    [field:SerializeField]
    public int SceneIndex { get; set; }
    public void Change_Scene()
    {
        SceneManager.LoadScene(SceneIndex);
    }

	public void Update()
	{
		if(Input.GetKeyDown(KeyCode.Tab))
        {
            Change_Scene();
        }
	}
}
