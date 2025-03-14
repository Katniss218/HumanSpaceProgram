# Career Mode Elements - Scratchpad:

    This document is not strictly documentation. It hopefully should be close, but I'm not perfect and sometimes can't be bothered to update every doc.
#

## Tech Tree:
The tech tree is a graph of interconnected tech nodes.
Each node contains unlockable elements - such as:
* Parts
* Abilities (like being able to terraform a launch site, or something)
* Anything else (that can be coded in c#).

Unlocking a tech node takes time.
The time is `time = constant * discounted_tech_point_cost / number_of_scientists_working_on_the_tech` (or similar, depending on if we want to implement different types/efficiencies of scientists)

#### Unlocking and Experiments:
1. Perform experiments using sensors.
2. Move the data (return or transmit) to a place where scientists can process it.
3. Select a Tech Node to work on.
4. Scientists will now start working on processing the data, converting the science points into tech points of that specific tech node. Default conversion ratio: 1 --> 10

Scientists are assigned to individual experiments, prioritising ones that are already started. You can reassign how many are woeking on what.
Each experiment defines how long it takes to convert 1 sci to x techpoints (how time-consuming its analysis is). 

#### Unlock Requirements:
The tech node has a number of unlock requirements that have to be met before its research can start (using x Tech Points in total to complete).
* Each tech node defines a minimum number of "prior" nodes that need to be unlocked to be able to unlock self.
* Each prior node also can be marked as required, so it will have to be unlocked before, regardless of the number of other unlocks. 

#### Unlock Cost:
Each tech node has a base cost in Tech Points, which can (and often will) be discounted:
- Researching a related technology (e.g., hydrolox engines) makes similar techs cheaper.
- Researching a part can reduce the cost of unlocking related parts.
- Discounts are clamped to a maximum value defined by the tech node.

Combining discounts either additive, or not combined (take largest).
Discounts external to the state of the tree?

A tech node belongs to a node category. The categories must be globally unique/distinct from the node ids. Everything that matches a node matches its id or category. 

## Proc Parts & the tech tree:

Proc parts aren't like in KSP at all. Proc parts are better thought of as an easy way to make custom parts for HSP, that all follow the same scheme, and vary by some parameters. 

## Money:

Money is primarily earned via contracts.
There are also "milestone" type bonus rewards (like "reach 100 km") that once completed can give money or science. These can be implemented using the contract system.

A given pool of money can be restricted to be spent on a given category of activity - such as:
- building rockets
- hiring researchers
- buying propellant
- etc.

## Contracts:

#### Success Criteria:
A contract can have a number of individual 'pieces', that can be completed independently or sequentually.
Each criterion type has a C# class that can determine when it is met.
A boolean expression can be used to combine multiple criteria. When that expression is met, the 'piece' of the contract is completed.

A contract has a deadline. If it is reached, the contract fails and a failure penalty (optional) is deducted.

I think I'll also have a "handoff" system that once you launch a rocket for a customer, you'll need to hand it off to them for the contract to complete (you can no longer use it).

#### Payouts:
Contracts can pay out on advance, complete, and/or at arbitrary number of points in time (pay as you go).
Contracts can pay out money, science points, and/or call any C# method.

#### Contract Negotiation:

Each contract has a `score`, which represents "how good the contract is for the player".
The score is calculated from the success criteria (harder contracts have lower score) and the payouts (higher-paying contracts have a higher score).
The contracts presented to the player should be balanced in such a way that the score fits between the min/max allowed values. These values depend on the current reputation the player has with the agency.
The boolean expressions combine the score differently depending on the type of expression (AND/OR/XOR/etc).

When the player tries to negotiate a contract, and it doesn't fit between the min/max values, the negotiation engine will either reject it or adjust and present it back to the player.
Negotiations can be done before and/or after the contract has been accepted. The likelihood of the agency agreeing to negotiations depend on how high the `score` is - higher score = lower likelihood of being allowed to negotiate.
The player can change any unmet aspect of the contract (criteria and payouts) during the negotiations. 'Pieces' that have been finished can't be changed.

The more reputation you have, the better the contracts. 
Each contract template can specify how much score is associated per requirement/etc.

## Loans:
The player can also take loans.
THey have to be paid back (with interest) later.

## Buying parts:

When you want to make a rocket, you don't buy the parts on demand. 
You can buy parts ahead of time. Buying in bulk reduces costs. 
Parts that you have need to be stored somewhere in the physical game world??? (maybe?) 


