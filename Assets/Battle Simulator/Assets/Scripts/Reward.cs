using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reward {
   Queue<float> bag=new Queue<float>();
    public Reward() {
    }
    public void AddReward(float r) {
        bag.Enqueue(r);
    }
    public void Apply(AgentScript Agent) {
        float allReward=0;
        foreach(float i in bag.ToArray()) {
            allReward+=i;
        }
        Agent.AddReward(allReward);
        bag.Clear();
    }
    public virtual void RewardAtStep(AgentScript unit) {
    }
    public virtual void RewardAtDie(AgentScript unit) {
    }
    public virtual void RewardAtEpisodeEnds() {
    }
    public virtual void RewardAtRemained() {
    }
}
