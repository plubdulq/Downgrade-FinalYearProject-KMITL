using UnityEngine;
namespace WaypointSystem {
    public class AgentExample : MonoBehaviour {
        public WaypointAgent agent;
        void Awake() {

            Instruction join = Instruction.Create(InstructionType.JoinPath);
            Instruction speedChange = Instruction.Create(InstructionType.ChangeSpeed, 5);
            Instruction next = Instruction.Create(InstructionType.GoNthWaypoints, 11);
            Instruction wait = Instruction.Create(InstructionType.WaitSeconds, 1);
            Instruction loop = Instruction.Create(InstructionType.GoToInstructionIndex, 1);
            agent.InstructionList.Add(join);
            agent.InstructionList.Add(speedChange);
            agent.InstructionList.Add(wait);
            agent.InstructionList.Add(next);
            agent.InstructionList.Add(loop);
            agent.OnObjectiveReached += AgentOnObjectiveReached;
            agent.ExecuteNextInstruction();

        }

        void AgentOnObjectiveReached() {
            //Debug.Log("the example has reached its destination");
        }
    }
}