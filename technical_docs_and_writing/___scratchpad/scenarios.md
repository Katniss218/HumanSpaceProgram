# Scenarios

    This document is not strictly documentation. It hopefully should be close, but I'm not perfect and sometimes can't be bothered to update every doc.
#

A scenario basically *is* a save, but it doesn't belong to a timeline, it's just a raw save.

#### Starting the game:
- Click the start new game button.
- Select the starting scenario, name, and description for your timeline.
- Click start (accept).

scenarios will have icons (png) that will be displayed on the selection screen

GameData/<mod_id>/Scenarios/<scenario_id>/...

scenario schema:
```csharp
public class ScenarioMetadata
{
    public NamespacedID ID { get; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Sprite Icon { get; set; }
}
```

Scenarios should be parameterizable
- a tabbed list of parameters when the timeline is created/loaded for the first time, that are passed into the event.
- so basically a scenario should have its own settings pages, but they only store the data for use by the 'create scenario' event listeners.

#### Timeline Start event/method:
The `TimelineManager.CreateNew( ... )` method needs to provide a scenario id to load.
- Should the listeners be responsible for that or the method itself?


initially, scenarios are created using C# code.

a scenario is authored in a (paused?) gameplay scene, or in a scenario editor or somewhere idk
- need to ensure that the initialization (awake/start/etc) happens correctly.
after that it is saved to a scenario json


The current timeline/save in the timelinemanager should always be nonnull





