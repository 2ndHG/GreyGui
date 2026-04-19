# GreyGui - An HTML-inspired GUI Library for MonoGame
![GreyGui Banner](Banner.png)

With GreyGui, you can create graphical user interface (GUI) in MonoGame with familiar HTML-like syntax.

## Key Features
GreyGui provides minimalism visuals with the following features.

* **Dynamic Scaling** - Elements can auto scale with its container size to fit in any screen resolution
* **Rounded Corner** - Create modern visuals with beautifully rounded rectangles
* **SDF Font** - The text system uses signed distance field (SDF) technology to render sharp text at any font size
* **Explicit Control** - You, as a programer, will control and know when to Update and Draw your elements
* **Customizable** - Load your own Texture2D or just override the default drawing methods

## Syntax Overview
GreyGui simulate HTML syntax by doing some quirky utilizations of C# syntax.
* Element have constructors that includes all their attributes, for instance, `new Panel(size: new Vector2(200, 100))`, just like writing  `<div style="width:200px; height:100px;">` to an HTML element
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

## Getting Started
🛠️ This README is still under constructing... 🛠️