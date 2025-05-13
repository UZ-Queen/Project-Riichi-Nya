using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public static class InputPreset{
    public static KeyCode left = KeyCode.A;
    public static KeyCode right = KeyCode.D;
    // public static string
    public static KeyCode discard = KeyCode.W;
    public static KeyCode discardTsumoTile = KeyCode.Q;

    public static KeyCode riichi = KeyCode.R;
    public static KeyCode tsumoAgari = KeyCode.Space;
}