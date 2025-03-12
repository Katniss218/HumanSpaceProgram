# Settings:

    This document is not strictly documentation. It hopefully should be close, but I'm not perfect and sometimes can't be bothered to update every doc.
#

Settings are grouped into 'Settings pages'

Each page is an entry in the settings.json file.

```json
[
    {   // page #1
        "$type": "TYPE",
        // data ...
    },
    {   // page #2
        // ...
    }
]
```

The pages are a logical grouping of settings.
Each page will be a tab in the settings.

Each of these pages will later also be displayed in the settings menu.

Pages can contain a bunch of different types of entries: toggles, floats, keybinds, lists, enums, and whatever else really
as long as you can render a UI component for it and save/load it, it is valid.



## TODO:

I want mods to be able to extend existing pages (for some of them, like keybinds)
i.e. mods append to the keybinds page instead of creating their own.
the backend page can be separate, that doesn't matter.



add a contextual input that will change its appearance based on the type



