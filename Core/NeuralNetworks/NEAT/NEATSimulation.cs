﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace Project1.Core.NeuralNetworks.NEAT
{
    public class NEATSimulation : Simulation
    {
        public NeatHost neatHost;
        private int inputSize;
        private int outputSize;
        private int maxClients;
        private bool isSimulating = true;
        public bool globalSharing = true;
        private bool pressedG = false;

        public NEATSimulation(int inputSize, int outputSize, int GenerationSize, float MutationRate = 0.01F, float MaxSimulationTime = 1) : base(GenerationSize, MutationRate, MaxSimulationTime)
        {
            this.inputSize = inputSize;
            this.outputSize = outputSize;
            maxClients = GenerationSize;

            neatHost = new NeatHost(inputSize, outputSize, GenerationSize, this);
        }
        public override void Deploy()
        {
            neatHost.Reset(inputSize, outputSize, maxClients);
            //base.Deploy();
        }
        public override void Update()
        {
            Time+= Game1.delta;
            int inActivity = 0;

            if (!pressedG && Keyboard.GetState().IsKeyDown(Keys.G))
            {
                pressedG = true;
                globalSharing = !globalSharing;
            }

            if (!Keyboard.GetState().IsKeyDown(Keys.G))
            {
                pressedG = false;
            }

            foreach (GeneticAgent agent in Agents.ToArray())
            {
                if (agent is ContinuousGeneticAgent r)
                {
                    r.Update();
                }
                else
                {
                    inActivity++;
                }
            }


            if ((Time >= MaxSimulationTime || inActivity == Agents.Count) && globalSharing)
            {
                foreach (GeneticAgent agent in Agents)
                {
                    /*if (agent is ContinuousGeneticAgent r)
                    {
                        if (r.IsActive()) r.Kill();
                    }
                    else */agent.CalculateCurrentFitness();
                }


                BestAgent = FindBestAgent();
                neatHost.Evolve();
                Time = 0;
                Generation++;
            }
        }
    }
}
