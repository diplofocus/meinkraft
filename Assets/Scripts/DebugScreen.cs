using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour
{

    World world;
    Text text;

    float frameRate;
    float timer;
    void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        text = GetComponent<Text>();
    }

    void Update()
    {
        string debugText = "Diplodocus";
        debugText += "\n";
        debugText += frameRate + " fps";
        debugText += "\n";
        debugText += "XYZ: "
         + Mathf.FloorToInt(world.player.transform.position.x)
         + " / "
         + Mathf.FloorToInt(world.player.transform.position.y)
         + " / "
         + Mathf.FloorToInt(world.player.transform.position.z);
        debugText += "\n";
        debugText += "Chunk Coords: "
         + world.playerChunkCoord.x
         + " / "
         + world.playerChunkCoord.z;
        debugText += "\n";

        text.text = debugText;

        if (timer > 1f)
        {
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;
        }
    }
}
