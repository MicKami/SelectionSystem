using UnityEngine;
using UnityEngine.UI;

public class InvertImageColor : MonoBehaviour
{
	public void Invert(Image image)
	{
		var alpha = image.color.a;
		image.color = Color.white - image.color;
		image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
	}
}
