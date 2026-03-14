using System;
namespace MyProject.Models
{
    [Serializable]
    public class DeviceDetail
    {
        public string deviceModel;
        public float basePower;
        public float maxPower;
        public float userLoadRatio;
        public float placementFactor;
        public string rackUnitSize;
        public float ambientHeat;
    }

    [Serializable]
    public class DeviceList
    {
        public string device_type;
        public DeviceDetail[] total_models;
    }

    [Serializable]
    public class DeviceModelList
    {
        public DeviceList[] devices;
    }
}
