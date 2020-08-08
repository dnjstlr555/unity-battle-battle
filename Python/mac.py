# Parameter Setting 
obs_size = [80,80,1]
state_size = 3
action_size = 4 

run_step = 1000
test_step = 1000

train_mode = False 

# Number of Agents
env_config = {"Num_Agent": 3}

# Unity Environment Path 
game = "Predator_Prey"
env_name = "../env/" + game + "/Mac/" + game

import numpy as np
import sys
import random
from mlagents.envs import UnityEnvironment
import numpy as np

print("Python version:")
print(sys.version)

# check Python version
if (sys.version_info[0] < 3):
    raise Exception("ERROR: ML-Agents Toolkit (v0.3 onwards) requires Python 3")

# Unity environment setting (file_name)
env = UnityEnvironment(file_name=env_name)

# Brain setting 
brain_name = env.brain_names[0]
brain = env.brains[brain_name]

# 환경 설정 (env_config)에 따라 유니티 환경 리셋 및 학습 모드 설정
env_info = env.reset(train_mode=train_mode, config=env_config)[brain_name]
num_agent = np.array(env_info.vector_observations).shape[0]

# Agent class -> 알고리즘을 위한 다양한 함수 정의 
CONST_PREY=0
CONST_PREDATOR=1
class Agent():
    def __init__(self):
        pass 

    def get_action(self, state):
        # Random action
        role=state[2]
        if role==CONST_PREY:
            return np.random.randint(0, action_size)
        elif role==CONST_PREDATOR:
            return np.random.randint(0, action_size)
        else:
            return 0
    
# Define agent 
agent = Agent()

if __name__=="__main__":
    step = 0
    prev_step = 0
    episode = 0
    score_list = []
    print("Run models until {} steps / Test up models until {} steps".format(run_step, run_step + test_step))
    while step < run_step + test_step:
        # Initialize state, score, done
        state_array = np.array(env_info.vector_observations)
        score = np.zeros([num_agent])
        done = False

        env_info = env.reset(train_mode=train_mode)[brain_name]
        
        score_reward=np.array([0.0,0.0,0.0,0.0])
        # 한 에피소드를 진행하는 반복문 
        while not done:
            if step >= run_step and train_mode != False:
                train_mode = False
                print("run step riched, testing the model now")

            # 행동 결정 및 유니티 환경에 행동 적용
            action_list = []

            for i in range(num_agent): 
                #action_list.append(agent.get_action(state_array[i,:]))
                action_list.append(episode%4)
            
            # 다음 상태, 보상, 게임 종료 정보 취득 
            env_info = env.step(action_list)[brain_name]
            
            next_state_array = np.array(env_info.vector_observations)
            reward_array = np.array(env_info.rewards)
            done_array = np.array(env_info.local_done)

            done = False 
            if True in done_array:
                done = True
            if step-prev_step >= 200:
                done = True

            score_reward += reward_array
            
            # Update state  
            state_array = next_state_array
            step += 1

        score += reward_array
        episode += 1

        # Print progress 
        print("Step: {} / For {} / Episode: {} / Raw: {} / Pred. Score: {} / Prey Score: {} ".format(step, step-prev_step, episode, np.round(score, 2), round(score_reward[[0,2,3]].sum(),2), round(score_reward[1],2)))
        prev_step=step
    print("End of series")
    env.close()