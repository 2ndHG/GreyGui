*Caution: This library is still in development, breaking changes could happen*
# GreyGui - An HTML-inspired GUI Library for MonoGame
![GreyGui Banner](https://raw.githubusercontent.com/2ndHG/GreyGui/main/Github/Image/Banner.png)

With GreyGui, you can create graphical user interface (GUI) in MonoGame with familiar HTML-like syntax.

Demo picture:
![GreyGui Banner](https://raw.githubusercontent.com/2ndHG/GreyGui/main/Github/Image/GreyGuiDemo.png)

## Key Features
GreyGui provides minimalistic visuals with the following features.

* **Dynamic Scaling** - Elements can auto-scale with their container size to fit in any screen resolution
* **Rounded Corner** - Create modern visuals with beautifully rounded rectangles
* **SDF Font** - The text system uses signed distance field (SDF) technology to render sharp text at any font size
* **Explicit Control** - You, as a programmer, will control and know when to Update and Draw your elements
* **Customizable** - Load your own Texture2D or just override the default drawing methods

## Syntax Preview
GreyGui simulates HTML syntax by doing some quirky utilizations of C# syntax.
* Elements have constructors that include all their attributes, for instance, `new Panel(size: new Vector2(200, 100))` is just like writing  `<div style="width:200px; height:100px;">`
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

To dive in, head to [Hello GreyGui](https://github.com/2ndHG/GreyGui/blob/main/Github/Markdown/HELLO_GREY_GUI.md) to learn the basics 🐈