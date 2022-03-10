# RailMaker
A Rail generation tool for TSG, v 1.0.0

## Download
You can either clone this project directly into your Asset folder or import the package.

## Usage
### Creating a Rail
#### From Scratch:
Click the Railmaker/Create Rail Button under Tools in the Menu bar. This should create a new Rail object.
Specify a Material and click 'Regenerate' in the Inspector.
#### Copy a Prefab:
Go to the folder 'Prefabs' in the Railmaker folder. Select a prefab and drag it into the scene view.
**Important!** Unpack the new GameObject by pressing right click -> Prefab -> unpack completely. This makes it so there is no link to the prefab.
Then, simply press 'Regenerate' in the Inspector.

### Inspector
#### Rail Settings
Here you can configure your overall Rail Parameters, like cross-section, radius, material, etc.
##### Shape Type
Defines the shape of the cross-section of the rail, the options are:
- Circle
- Square
- Empty: doesn't generate a model, only colliders, useful for when you want to use a custom model.
- Custom: Not implemented
##### Rail Material
Defines the Material of the mesh.
##### Max Arc Length Per Segment
Defines the fineness for the model and number of colliders on a rounded rail, lower value -> more subdivisions / colliders. The game doesn't like lots of rail colliders, so try to increase this value if you have problems with random bails.
##### Rail Radius
Defines the radius of the rail, rainbow rails seem to behave better with a lower Rail radius.
##### Extra Collider Length
Adds a bit of length to the capsule colliders, useful in concave rails because it reduces the chance of random bails. Usually a value similar to the radius works well.
**Keep this value 0 for convex rails, like rainbow rails!**
#### Points
The points describe the path of your rail, you need at least two. You can only view the properties and edit the point you have selected. 
The selected point is colored green in the scene View. To navigate between the points, press the Next / Previous Buttons or drag a different point in the scene View.
##### Position
Position of the selected point relative to the GameObject.
##### Has Radius
Checking 'Has Radius' allows you to set the radius of this point. Not having 'Has Radius' checked results in a sharp corner.
##### Angle
This allows you to angle the colliders, useful for S-Rails.
##### Buttons
Navigate between the points by pressing Previous / Next.
Use 'Insert Point' to add a new Point after the selected one and 'Add new Point' to add a new Point after the last one. You automatically select the newly created point.
'Delete' deletes the selected point. :)
#### Cosmetics
Allows you to edit the look of the rail, not many options currently.
##### Posts
Add a post at the beginning or the end of the rail.
Post Radius is not implemented yet.
##### UV
Tiling allows you to set the tiling of the V coordinate.
#### Regenerate
Check 'Enable Auto Regenerate' to automatically update the rail if you change a parameter, so you don't have to press 'Regenerate' every time. Maybe leave it unchecked if your PC struggles with Auto mode.
'Regenerate' regenerates the rail. :)

### Scene View
You can see your points in the sceneview as Vector handlers. Simply drag them around to change your rail. The point you move gets selected in the Inspector.

## Trouble Shooting
If your rail doesn't generate properly:
- try to delete all children of your rail GameObject and press 'Regenerate'
- set the railshape to something different and back again
- make sure your rail GameObject isn't a Prefab.
- Ask me in the TSG Discord. :)
