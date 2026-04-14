# GreyGui - An HTML inspired GUI Library for MonoGame
![GreyGui Banner](/Github/Banner.png)

With GreyGui, you can create graphical user interface (GUI) in MonoGame with familiar HTML-like syntax.

## Key Features
GreyGui provides minimalism visuals with the following features.

* **Dynamic Scaling** - Elements can auto scale with its container size to fit in any screen resolution
* **Rounded Corner** - Create modern visuals with beautifully rounded rectangles
* **SDF Font** - The text system uses signed distance field (SDF) technology to render sharp text at any font size
* **Explicit Control** - You, as a programer, will control and know when to Update and Draw your elements
* **Customizable** - Load your own Texture2D or just override the default drawing methods

## Getting Started
🛠️ This README is still under constructing... 🛠️


## How To Make A GreyGuiElement

### Focus
Focus is a global state remarks what element is the system currently focusing. It is stored in a static field, `GuiUpdate.FocusedElement`, so only one of all elements can be the focused element.

1. Clear the currently focusing element:

    a. Logically, set `GuiUpdate.FocusedElement` to `null`

    b. Think of **whether this action terminates other element's focus**, 