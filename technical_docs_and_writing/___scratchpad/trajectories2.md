
Now, Trajectories as it stands is good for simulating motion, but where it lacks is in being able to simulate other things, and in storing arbitrary data associated with each body/step.
I don't want to hardcode the new data inside the state vector struct. 
I'd like a more modular approach. And that approach will have to be cache-friendly, and easy to use.
Generally speaking, these functions, providers, etc. are "pure" and shouldn't use data from outside the simulator.




split ephemerides into multiple smaller ones?
- motion
- mass
- propellants
- other?



