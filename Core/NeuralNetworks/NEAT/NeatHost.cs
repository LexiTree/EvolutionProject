﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvoSim.Helpers;
using EvoSim.ProjectContent.CellStuff.SightRayStuff;

namespace EvoSim.Core.NeuralNetworks.NEAT
{
    public class NeatHost
    {
        public static readonly int MAX_NODES = (int)Math.Pow(2, 20);

        public NEATSimulation simulation;

        public Dictionary<ConnectionGene, ConnectionGene> allConnections = new Dictionary<ConnectionGene, ConnectionGene>();
        public List<NodeGene> allNodes = new List<NodeGene>();
        public List<Species> species = new List<Species>();

        public int maxClients;
        public int outputSize;
        public int inputSize;

        public double C1 = 1;
        public double C2 = 1;
        public double C3 = 0.5f;

        public double CP = 6f;

        public double WEIGHT_SHIFT_STRENGTH = 20f;
        public double WEIGHT_RANDOM_STRENGTH = 10f;

        public double PROBABILITY_MUTATE_LINK = 1.8f;
        public double PROBABILITY_MUTATE_NODE = 2.6f;
        public double PROBABILITY_MUTATE_WEIGHT_SHIFT = 1.3f;
        public double PROBABILITY_MUTATE_WEIGHT_RANDOM = 1.3f;
        public double PROBABILITY_MUTATE_WEIGHT_TOGGLE_LINK = 0.6f;

        public float SURVIVORS = 0.8f;

        public int STALESPECIES = 20;

        public NeatHost(int inputSize, int outputSize, int clients, NEATSimulation parent)
        {
            simulation = parent;
        }

        public Genome GenerateEmptyGenome()
        {
            Genome g = new Genome(this);
            for (int i = 0; i < inputSize + outputSize; i++)
            {
                g.nodes.Add(getNode(i + 1));
            }
            g.GenerateCalculator();

            return g;
        }

        public List<GeneticAgent> GetClients() => simulation.Agents;

        public void Reset(int inputSize, int outputSize, int maxClients)
        {
            this.inputSize = inputSize;
            this.outputSize = outputSize;
            this.maxClients = maxClients;

            allConnections.Clear();
            allNodes.Clear();
            simulation.Agents.Clear();

            for (int i = 0; i < inputSize; i++)
            {
                NodeGene n = getNode();
                n.x = 0.1f;
                n.y = (i + 1) / (float)(inputSize + 1);
            }

            for (int i = 0; i < outputSize; i++)
            {
                NodeGene n = getNode();
                n.x = 0.9f;
                n.y = (i + 1) / (float)(inputSize + 1);
            }

            for (int i = 0; i < this.maxClients; i++)
            {
                Genome g = GenerateEmptyGenome();
                //g.FullyConnect();
                NeatAgent a = simulation.InitialiseAgent(g) as NeatAgent;
                a.SetGenome(g);
                GetClients().Add(a);
            }
        }

        public NeatAgent GetClient(int i) => GetClients()[i] as NeatAgent;

        public NodeGene getNode()
        {
            NodeGene n = new NodeGene(allNodes.Count + 1);
            allNodes.Add(n);
            return n;
        }

        public NodeGene getNode(int id)
        {
            if (id <= allNodes.Count) return allNodes[id - 1];
            return getNode();
        }

        public static ConnectionGene getConnection(ConnectionGene connection)
        {
            ConnectionGene c = new ConnectionGene(connection.from, connection.to);
            c.setInovationNumber(connection.getInovationNumber());
            c.weight = connection.weight;
            c.enabled = connection.enabled;

            return c;
        }

        public ConnectionGene getConnection(NodeGene node1, NodeGene node2)
        {
            ConnectionGene connectionGene = new ConnectionGene(node1, node2);

            if (allConnections.ContainsKey(connectionGene))
            {
                connectionGene.setInovationNumber(allConnections[connectionGene].getInovationNumber());
            }
            else
            {
                connectionGene.setInovationNumber(allConnections.Count + 1);
                allConnections.Add(connectionGene, connectionGene);
            }

            return connectionGene;
        }

        public void Evolve()
        {
            GenerateSpecies();
            Kill();
            RemoveExtinctSpecies();
            Reproduce();
            Mutate();

            foreach (NeatAgent n in GetClients())
            {
                if (n.Dna == null || n.Dna is not Genome)
                    continue;
                n.GenerateCalculator();
            }
        }

        public void GenerateSpecies()
        {
            foreach (Species s in species)
            {
                s.Reset();
            }

            List<GeneticAgent> agents = GetClients();

            foreach (GeneticAgent a in agents)
            {
                NeatAgent nA = a as NeatAgent;
                if (nA.GetSpecies() != null) continue;

                bool found = false;
                foreach (Species s in species)
                {
                    if (s.Add(nA))
                    {
                        found = true;
                        break;
                    }

                }

                if (!found)
                {
                    species.Add(new Species(nA));
                }
            }

            foreach (Species s in species)
            {
                s.EvaluateScore();
            }

            foreach (GeneticAgent a in agents)
            {
                NeatAgent nA = a as NeatAgent;
                if (nA.IsActive())
                    nA.Initialise();
            }
        }

        public void Kill()
        {
            foreach (Species s in species)
            {
                s.Kill(1 - SURVIVORS);
            }
        }

        public void RemoveExtinctSpecies()
        {
            for (int i = species.Count - 1; i >= 0; i--)
            {
                if (species[i].Size() <= 1)
                {
                    species[i].staleness++;

                    if (species[i].staleness >= STALESPECIES)
                    {
                        species[i].GoExtinct();
                        species.RemoveAt(i);
                    }
                }
                else
                {
                    species[i].staleness = 0;
                }
            }
        }

        public Species PickWeightedSpecies()
        {
            float totalFitness = 0;

            foreach (Species s in species) totalFitness += (float)s.score;

            float r = Main.random.NextFloat(totalFitness);
            int index = 0;
            while (r > 0 && index < species.Count)
            {
                r -= (float)species[index].score;
                index++;
            }

            return species[Math.Max(0, index - 1)];
        }

        public void Reproduce()
        {
            foreach (GeneticAgent a in GetClients())
            {
                NeatAgent nA = a as NeatAgent;

                if (nA.GetSpecies() == null)
                {
                    if (nA is SightRay ray)
                        ray.debugInfoRepopulation = 5;
                    if (species.Count > 0)
                    {
                        Species s = PickWeightedSpecies();
                        nA.SetGenome(s.Breed());
                        nA.Mutate();
                        s.ForceAdd(nA);

                        if (nA is SightRay ray2)
                            ray2.debugInfoRepopulation = 10;
                    }
                }
            }
        }

        public void Mutate()
        {
            foreach (GeneticAgent a in GetClients())
            {
                NeatAgent nA = a as NeatAgent;
                if (nA.Dna != null && nA.Dna is Genome)
                    nA.Mutate();
            }
        }
    }
}
