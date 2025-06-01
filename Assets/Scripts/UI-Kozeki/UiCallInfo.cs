using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UiCallInfo : MonoBehaviour
{
    [SerializeField] private GameObject uiRiichiNya;
    [SerializeField] private GameObject uiTsumoNya;
    bool beforeRiichiable = false;
    bool beforeTsumoable = false;

    void OnEnable()
    {
        // uiRiichiNya?.gameObject.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="riichiiable"></param>
    /// <param name="tsumoble"></param>
    /// <returns>true if the value is changed(이전 값과 다르면 참 반환)</returns>
    public bool UpdateInfo(bool riichiiable, bool tsumoble)
    {
        uiRiichiNya.SetActive(riichiiable);
        uiTsumoNya.SetActive(tsumoble);

        if (riichiiable == beforeRiichiable && tsumoble == beforeTsumoable)
        {

            return false;
        }
        beforeRiichiable = riichiiable;
        beforeTsumoable = tsumoble;
        return true;
    }

}
