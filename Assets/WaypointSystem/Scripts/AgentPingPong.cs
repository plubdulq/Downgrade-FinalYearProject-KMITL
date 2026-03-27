using UnityEngine;
namespace WaypointSystem {
    public class AgentPingPong : MonoBehaviour {
        public WaypointAgent agent;

        public int waypointsBeforeChange = 2;
        void Awake() {

            Instruction join = Instruction.Create(InstructionType.JoinPath);
            Instruction speedChange = Instruction.Create(InstructionType.ChangeSpeed, 5);
            Instruction next = Instruction.Create(InstructionType.GoNthWaypoints, waypointsBeforeChange);
            Instruction wait = Instruction.Create(InstructionType.WaitSeconds, 1);
            Instruction speedChangeNegative = Instruction.Create(InstructionType.ChangeSpeed, -5);
            Instruction previous = Instruction.Create(InstructionType.GoNthWaypoints, -waypointsBeforeChange);
            Instruction wait2 = Instruction.Create(InstructionType.WaitSeconds, 1);
            Instruction loop = Instruction.Create(InstructionType.GoToInstructionIndex, 1);

            agent.InstructionList.Add(join);
            agent.InstructionList.Add(speedChange);
            agent.InstructionList.Add(next);
            agent.InstructionList.Add(wait);
            agent.InstructionList.Add(speedChangeNegative);
            agent.InstructionList.Add(previous);
            agent.InstructionList.Add(wait);
            agent.InstructionList.Add(loop);
            agent.OnObjectiveReached += AgentOnObjectiveReached;
            agent.ExecuteNextInstruction();

        }

        void AgentOnObjectiveReached() {
            //Debug.Log("the example has reached its destination");
        }
    }
}