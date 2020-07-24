from envs.environment import UnityEnvironment
from envs.side_channel.engine_configuration_channel import EngineConfigurationChannel
import random
import numpy as np
from pprint import pprint

env_name = "battle-battlev2.app"
behaviour_name = "DwarfBehaviour?team=0"
agent_num = 2
RUN_STEP = 10000

channel = EngineConfigurationChannel()
channel.set_configuration_parameters(time_scale = 9.0, width=640, height=480)

env = UnityEnvironment(file_name=env_name, side_channels=[channel])

behavior_names = env.behavior_specs.keys()

class AgentAction():
    def __init__(self):
        pass 

    def get_action(self, state, reward):
        # Random action
        angle=random.uniform(-1,1)
        force=random.uniform(-1,1)
        return np.array([angle,force])
    
# Define agent 
AgentAction = AgentAction()

if __name__=="__main__":
    env.reset()
    ENV_INFO = env.behavior_specs #환경 정보 불러오기
    pprint(ENV_INFO)
    print('Training begins')
    step = 0
    last_step = 0
    episode = 0
    
    while step < RUN_STEP:
        done=False
        avg_reward = np.zeros(agent_num)
        while not done:
            info = env.get_steps(behaviour_name)
            DecisionSteps = info[0] #Decision을 요청하는 애들의 정보
            TerminalSteps = info[1] #에피소드가 끝난 애들의 정보
            if(len(TerminalSteps)>0):
                done=True           
            for agent in DecisionSteps.agent_id:
                reward = DecisionSteps[agent].reward
                observation = DecisionSteps[agent].obs
                env.set_action_for_agent(behaviour_name, agent, AgentAction.get_action(observation, reward))
            #for inactive_agent in TerminalSteps.agent_id: (비활성화 에이전트를 다룰때 사용)
            if len(DecisionSteps.reward)>0:
                avg_reward = np.add(avg_reward,DecisionSteps.reward)
            step += 1
            env.step()
        #env.reset()
        episode +=1
        print(f'Step:{step}/Episode:{episode}/Avg.Reward:{np.divide(avg_reward, step-last_step)}')
        last_step=step
    env.close()