
namespace WaypointSystem {
    public class NavigationInfo {
        public float TValue;
        public Waypoint waypoint;

        public NavigationInfo(float value, Waypoint wp) {
            TValue = value;
            waypoint = wp;
        }
    }
}