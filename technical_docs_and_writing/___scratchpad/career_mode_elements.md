# Career Mode Elements - Scratchpad:

    This document is not strictly documentation. The contents might not align with the game in the future.
#



Money:
Money is earned via contracts. 
Contracts can pay out at any arbitrary number of points in time. Not just on advance/complete (pay as you go). 
A contract has a deadline, beyond which it will not pay. If the deadline is reached, the contract fails and a penalty (optional) is deducted from the player. 
The player can also take loans.
There are also "milestone" type bonus rewards (like "reach 100 km") that once completed can give money or science
A contract can have a number of individual pieces that can be completed individually, sequentually, etc. 
Contracts can be negotiated by the player, to adjust/change the requirements (criteria).
Each criterion type has a c# class that is responsible for checking whether it is currently met. 
There's a negotiation engine responsible for generating offers and accepting/rejecting player offers. 
The contract has a total score. It represents "how good the contract is for the player". The max allowed score depends on your reputation. The negotiations change the score. 
There's maximum score that can be accepted. Contracts above this score will be rejected or have another proposal given by the engine. 
Money gotten from a contract might be locked to be spent on a specific type of activity. 
The more reputation you have, the better the contracts. 
Each contract template can specify how much score is associated per requirement/etc.
I think I'll also have a "handoff" system that once you launch a rocket for a customer, you'll hand it off to them and you can no longer use it yourself



