#define HIMARI


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;



    static public class MyLogger
    {
        static public void Log(string log)
        {
#if HIMARI
            Debug.Log($"[로그] [{DateTime.Now:HH:mm:ss}] : {log}");
#endif
        }
        static public void LogWarning(string log)
        {
#if HIMARI
            Debug.LogWarning($"[경고] [{DateTime.Now:HH:mm:ss}] : {log}");
#endif
        }
                static public void LogError(string log)
        {
#if HIMARI
            Debug.LogError($"[치명적 경고] [{DateTime.Now:HH:mm:ss}] : {log}");
#endif
        }
    }

