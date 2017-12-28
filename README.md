ARKit Globe mobile app for DavidRumsey
=======

The app should show a vary large globe (human size ideally) - as a sphere with the texture on it.
The ideal interaction is that you can walk "in". 

App has a button / trigger for the placement of the globe in space.
It allows change of the size / zoom ( + -) of the globe
Spinning with swipe gesture on iPad (rotation with easing, and/or slow rotation forever)
Transparency settings (if it looks good).

Switching the texture of the globes (there are 4 globes at least)
High details must be available on the texture (you can read place names if you are close).


## Build

* Open Unity project and build it for iOS platform
* Navigate to the generated folder (your chosen name)
* Open Xcode project and navigate to assets, drag and drop AppIcon.appiconset folder from root folder to assets in Xcode
* Go to Build Settings and set Enable Bitcode to NO
* Remove mapbox resources from Build Phases -> Copy Bundle Resources if there are any
* After that it should be ready for building (Product -> Build) or release (Product -> Archive)
