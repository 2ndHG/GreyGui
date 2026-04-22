*Caution: This library is still in development, breaking changes could happen*
# GreyGui - An HTML-inspired GUI Library for MonoGame
![GreyGui Banner](https://raw.githubusercontent.com/2ndHG/GreyGui/main/Github/Banner.png)

With GreyGui, you can create graphical user interface (GUI) in MonoGame with familiar HTML-like syntax.

## Key Features
GreyGui provides minimalism visuals with the following features.

* **Dynamic Scaling** - Elements can auto scale with its container size to fit in any screen resolution
* **Rounded Corner** - Create modern visuals with beautifully rounded rectangles
* **SDF Font** - The text system uses signed distance field (SDF) technology to render sharp text at any font size
* **Explicit Control** - You, as a programer, will control and know when to Update and Draw your elements
* **Customizable** - Load your own Texture2D or just override the default drawing methods

## Syntax Preview
GreyGui simulates HTML syntax by doing some quirky utilizations of C# syntax.
* Element have constructors that includes all their attributes, for instance, `new Panel(size: new Vector2(200, 100))` is just like writing  `<div style="width:200px; height:100px;">`
* Container elements have `SetChildren` method that accepts an array of child nodes and return themselves

Combining these, we have:
```C#
new ListPanel(size: new(300, 200), colorMask: Color.Cyan, borderRadius: 5).SetChildren([
    new RowPanel(widthMode: WidthMode.ParentRatio, widthRatio: 1f, heightMode: HeightMode.ParentRatio, heightRatio: .6f).SetChildren([
        new Text(displayText: "Syntax demo", size: new(50, 40), fontSize: 40),
        new Button(size: new(30, 30), onLeftClicked: HandleMouseClick),
    ]),
    new Image(imageTexture: _buttonTexture)
]);
```

## Hello, GreyGui!
### Installation
Install GreyGui to your MonoGame Project using `dotnet add package GreyGui`
, then you're done!
### Basics
GreyGui must be initialized before we make any usage. We can initialize it in Game.Initialize and pass.

We provide Game instance as a parameter, so that GreyGui can access to the needed resource and context.
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
Okay, and let's create some GreyGuiElements, a `Button` inside a `ListPanel`!
```c#
GreyGuiElement root;
protected override void LoadContent()
{
    root = new ListPanel(size: new(100, 50)).SetChildren([
        new GreyGui.Button( size: new (0, 40), widthMode: WidthMode.ParentRatio, widthRatio:.8f )
    ]);
}
```
The above code firstly generate a ListPanel that is sized 100 x 50; Secondly, we added a Button as its child.

Look at the button's width, that's 0! How can a zero-width button works? But that's fine, because we have set its `widthMode` and `widthRatio`, the width will always be 80% of its parent's width, even when the parent size changes later.

**GreyGuiElements provides various layout and size mode**

### Text System



🛠️ This README is still under constructing... 🛠️