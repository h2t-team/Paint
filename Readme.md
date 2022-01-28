# Project paint

> Application for drawing graphic object.

## Group information

| Student's ID | Full name |
| ------ | ------ |
| 19120079 | Đoàn Thế Huy |
| 19120081 | Nguyễn Gia Huy |
| 19120126 | Nguyễn Việt Minh Tâm |

## Instructions

- User interface: 
![image](https://drive.google.com/uc?export=view&id=1n2y3nCwel7kW0CTnB06Gc_-16ndEZs-m)
- Install:
`git clone https://github.com/h2t-team/Paint.git`

1. Run Paint.exe in Release folder
2. Draw:
    - Default graphic object is Line. Can choose another graphic object in shapes box.
    - Drag and drop to draw chosen graphic object. Shape preview will show follow mouse.
3. Add style (color, pen width, stroke type):
    - Can choose style before drawing or when finish drawing one object.
    - Colors: Default color is black. When click on drop down arrow, colors box will appear to choose, also can custom new color.
    - Width: When click on drop down arrow, pen width list will appear. There are 3 choices: 1px (Default), 3px, 5px.
    - Stroke: When click on drop down arrow, stroke type list will appear. There are 4 choices: Solid (Default), Dash, Dot, Dash Dot
4. Tools:
    - Fill: When click on Fill tool and click on somewhere in canvas, the area inside the graphic object (except border) will be filled with the chosen color. If the selected position is not inside any objects, the background will be filled. 
    - Text: Drag and drop to to create a text area then type the text.
5. Undo, Redo:
    - Click on Undo button to remove the previous action.
    - Click on Redo button to repeat the last action done.
6. Zoom: Adjust zoom slider at the bottom right to zoom in or zoom out (From 25% to 800%).
6. Open image: Click File -> Open -> Choose image file -> Open
7. Save binary file:
    - Click File -> Save -> Choose binary format -> Save.
    - Open and continue working by click File -> Open -> Choose binary file -> Open
8. Save png/jpg:
    - Click File -> Save As -> Choose png/jpg format -> Save.
    - Open and continue working by click File -> Open -> Choose png/jpg file -> Open

## Technical details

- Basic wpf controls:
    - TextBlock, Button, DropDownButton
    - Window
    - ...
- C# OOP.
- Create interface layouts using containers:
    - Grid, Canvas, StackPanel
    - GroupBox
    - StatusBar
    - ScrollViewer
    - Menu
    - ...
- Design patterns: Factory.
- Plugin architecture: load graphic objects DLL plugins.

## Features have done

- ### Core requirements (7/7 points)

1. Dynamically load all graphic objects that can be drawn from external DLL files.
2. Can choose which object to draw.
3. Can see the preview of the object that want to draw.
4. Can finish the drawing preview and their change becomes permanent with previously drawn objects.
5. The list of drawn objects can be saved and loaded again for continuing later (own defined binary format).
6. Save and load all drawn objects as an image in png/jpg format (rasterization).

- ### Graphic objects

1. Line: controlled by two points, the starting point, and the endpoint.
2. Rectangle: controlled by two points, the left top point, and the right bottom point.
3. Square: controlled by two points, the left top point, and the right bottom point.
4. Ellipse: controlled by two points, the left top point, and the right bottom point.
4. Circle: controlled by two points, the left top point, and the right bottom point.
6. Diamond: controlled by two points, the left top point, and the right bottom point.
7. Hexagon: controlled by two points, the left top point, and the right bottom point.
8. Triangle: controlled by two points, the left top point, and the right bottom point.
9. Right Triangle: controlled by two points, the left top point, and the right bottom point.

- ## Improvements (Bonus points)

1. Allow the user to change the color, pen width, stroke type.
2. Adding text to the list of drawable objects.
3. Adding image to the canvas.
4. Reduce flickering when drawing preview by using buffer to redraw all the canvas
5. Zooming
6. Undo, Redo
7. Fill color
8. After drawing can edit (change color, pen width, stroke type)
9. Use MaterialDesign to create intuitive and beautiful UI.

## Video Demo

[Youtube Link](https://www.youtube.com/watch?v=d-yWmAWn2z4&ab_channel=Huy%C4%90o%C3%A0n)

## Expected Grade

| Student's ID | Full name | Working time | Expected Grade
| ------ | ------ | ------ | ------ |
| 19120079 | Đoàn Thế Huy |||
| 19120081 | Nguyễn Gia Huy |||
| 19120126 | Nguyễn Việt Minh Tâm |||
