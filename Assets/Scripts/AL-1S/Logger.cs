#define HIMARI
#undef HIMARI


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;



    static class Logger
    {
        static public void Log(string log)
        {
#if HIMARI
            Debug.Log($"[로그] [{DateTime.Now:HH:mm:ss}] : {log}");
#endif
        }
        static public void LogWarning(string log)
        {
#if SEX
            Debug.Log($"[경고] [{DateTime.Now:HH:mm:ss}] : {log}");
#endif
        }
    }

