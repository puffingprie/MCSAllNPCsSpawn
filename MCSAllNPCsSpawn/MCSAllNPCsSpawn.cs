using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;

namespace MCSAllNPCsSpawn
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class MCSAllNPCsSpawn : BaseUnityPlugin
    {
        public const string pluginGuid = "arx.mcs.allnpcsspawn";
        public const string pluginName = "MCSAllNPCsSpawn";
        public const string pluginVersion = "1.0.0.0";
        void Start() { }
    }
}
