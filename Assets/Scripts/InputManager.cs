using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }
}
public class AdvancementManager : MonoBehaviour
{
    public static AdvancementManager Instance { get; set; }
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