using UnityEngine;


namespace TNHTweaker.Logger
{
    static class TNHTweakerLogger
    {

        public static bool LogGeneral = false;
        public static bool LogCharacter = false;
        public static bool LogFile = false;
        public static bool LogPatrol = false;


        public enum LogType
        {
            General,
            Character,
            File,
            Patrol
        }

        public static void Log(string log, LogType type)
        {
            if (LogGeneral)
            {
                if(type == LogType.General)
                {
                    Debug.Log(log);
                }
                else if(type == LogType.Character && LogCharacter)
                {
                    Debug.Log(log);
                }
                else if (type == LogType.File && LogFile)
                {
                    Debug.Log(log);
                }
                else if (type == LogType.Patrol && LogPatrol)
                {
                    Debug.Log(log);
                }
            }
        }


    }
}
