using System.Collections.Generic;
using UnityEngine;
namespace WaypointSystem
{
    public interface ICurve
    {
        /// Returns sampled points along the curve from 'from' to 'to'
        List<Vector3> GetSampledPoints(Vector3 from, Vector3 to, float resolution);


    }

}