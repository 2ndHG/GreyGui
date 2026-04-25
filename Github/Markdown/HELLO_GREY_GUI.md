# Hello, GreyGui!
## Installation
Install GreyGui to your MonoGame Project using `dotnet add package GreyGui`
, then you're done!

## Basics
### Initialization
GreyGui must be initialized before we make any usage. We can initialize it by calling `GreyGuiCore.Initialize` in Game.Initialize (or other place).

We will provide the Game instance as a parameter, so that GreyGui can access to the needed resource and context.
```C#
// Game.Initialize()
protected override void Initialize()
{
    // Other initialization logics
    // ...
    GreyGuiCore.Initialize(this);
    base.Initialize();
}
```

### Create GreyGuiElements
Now, let's create some GreyGuiElements! A `ListPanel` contains a `Button` could be good, let's look at the following code.
```c#
GreyGuiElement root;
protected override void LoadContent()
{
    root = new ListPanel(size: new (100, 50)).SetChildren([
        new GreyGui.Button( size: new (0, 40), widthMode: WidthMode.ParentRatio, widthRatio:.8f )
    ]);
}
```
This code generates a ListPanel sized by 100 x 50, and then added a Button as its child.

We initialize the elements' attributes via their constructors, for example, the Button's width will always be 80% of its parent's width because of our `widthMode` and `widthRatio` settings (so the 0 assign to size.X does not affect!).

These attribute can be dynamically changed later.

**GreyGuiElements provides various layout and sizing methods**

### Text System



🛠️ This README is still under constructing... 🛠️
