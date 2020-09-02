run_step = 9999999999999

train_mode = False 

env_name = "macosv3.app" 
train_mode = True  

import numpy as np
import sys
import random
from mlagents.envs import UnityEnvironment


env = UnityEnvironment(file_name=None)


brain_name = env.brain_names[0]
brain = env.brains[brain_name]

action_size = brain.vector_action_space_size[0]
obs_size = brain.vector_observation_space_size
# Agent class
class Agent():
    def __init__(self):
        pass 

    def get_action(self, obs, reward):
        return np.random.randn(1,action_size)

agent = Agent()

if __name__=="__main__":
    step = 0
    last_step = 0
    episode = 0
    print(f"Run step:{run_step}")

    while step<run_step:
        reset_info = env.reset(train_mode=train_mode)[brain_name]
        all_done = {}
        for agent_id in reset_info.agents:
            all_done.update({agent_id:False})
        if all_done=={}:
            print("all_done was empty!!!!")
        prev_info = reset_info
        done = False
        while not done:
            action=np.zeros((0,action_size))
            for i, agent_id in enumerate(prev_info.agents):
                reward = prev_info.rewards[i]
                obs = prev_info.vector_observations[i]
                action = np.append(action, agent.get_action(obs, reward), axis = 0)
            try:
                env_info = env.step(action)[brain_name]
            except Exception as inst:
                print("exception occured")
                print(f"prevdone:{prev_info.local_done}/now done:{done}/all_done:{all_done}")
                raise

            for i, agent_id in enumerate(env_info.agents):
                all_done.update({agent_id:env_info.local_done[i]})
            print(f"{env_info.rewards} / {prev_info.rewards}")
            done = ([]==env_info.local_done) or (list(all_done.values())==[True]*len(reset_info.agents))
                
            prev_info=env_info
            step+=1
        episode+=1
        print(f"Step:{step}/Episode:{episode}")
        last_step=step
    env.close()