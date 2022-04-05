using BepInEx;
using System.Collections;
using UnityEngine;

namespace SkToolboxMiniMetro
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    [BepInDependency("com.Skrip.SkToolbox")]
    class SkBepInExLoader : BaseUnityPlugin
    {
        public const string
            MODNAME = "SkToolboxMiniMetro",
            AUTHOR = "Skrip",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "1.0.1.0";
    }
}