using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPS : MonoBehaviour
{
    private const int FRAME_BUFFER_SIZE = 120;
    float[] frameBuffer;
    int frameBufferIndex = 0;
    float frameRate = 0;

    private void Start()
    {
        frameBuffer = new float[FRAME_BUFFER_SIZE];
    }

    private void Update()
    {
        if(frameBufferIndex == 0)
        {
            frameRate = 0;
            for (int i = 0; i < FRAME_BUFFER_SIZE; i++)
            {
                frameRate += 1f / frameBuffer[i];
            }
            frameRate /= FRAME_BUFFER_SIZE;
        }
        frameBuffer[frameBufferIndex] = Time.deltaTime;
        frameBufferIndex = (frameBufferIndex + 1) % FRAME_BUFFER_SIZE;
    }
    private void OnGUI()
    {
        GUILayout.Label(frameRate.ToString("000"));
    }
}
