
should allow adding things essentially inside the unity physics step

without any issues with call order and method registration order.


ability to add things to happen during unity physics step, as a distinct thing opposed to before/after
- "during" should still allow to choose before/after the actual step.




API:

allow adding something to the inside of something else
- that something else should be moved inside a nested structure, but the access should be the same.

how to reference the place?

```csharp
InsertSystemAfter<FixedUpdate>( ..., typeof( PhysicsProcessing ) );

AddSystem<FixedUpdate, PhysicsProcessing>( ... );

// this should still work after
InsertSystemAfter<FixedUpdate>( ..., typeof( PhysicsProcessing ) );

InsertSystemAfter<FixedUpdate, PhysicsProcessing, PhysicsProcessing>();
```


FixedUpdate {
    1
    PhysicsProcessing
    PhysicsProcessing2D
    2
}

FixedUpdate {
    1
    PhysicsProcessing -- this just as a marker?
    {
        PhysicsProcessing -- this actually has the update method?
    }
    PhysicsProcessing2D
    2
}

this could be topo sorted as well, just sort individual levels

Does it need to be this flexible?

maybe just define a few steps where things can be added before/after/INSIDE?


use individual callback methods instead of systems, as we want a higher level abstraction that's easier to use (less boilerplate)

the system keeps track of the systems inside of itself.


















