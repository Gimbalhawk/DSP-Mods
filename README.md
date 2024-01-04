AutoLogisticsDroneSetup
===========

Automatically adds drones, shuttles, or logistics bots from your inventory or storage on the local planet when building a new logistics station. It's especially helpful now that we have blueprints so you don't need to click through and manually add shuttles to every station in your planet-wide factory.

Configuration
-------

This mod is configurable. You can edit your settings through the mod manager, but there's also an in-game config menu for changing things on the go. Simply click the config button on the left side of the transportation build menu (hit "6" to open it) and it will open the config menu.

Storage priorities: Changes the order that the mod looks for drones between your inventory, storage boxes, or items stored in stations. Lower numbers take precedence, but you can also set a storage option to a negative number to disable it.


Changelist
--------

### 1.7.0

* Tweaked code to pull from the end of your inventory, not the start. Should be a little nicer if you don't use DSPAutoSorter. Also allowed it to pull from the player's logistic storage and made it work with architect mode from CheatEnabler

### 1.6.0

* Added a config menu to let you change your settings while you play

### 1.5.0

* Added support for logistics bots

### 1.4.1

* Fixed errors caused by latest game update

### 1.4.0

* You can now choose the priority of each kind of storage (player inventory, storage chests or stations) to control which gets pulled from first

### 1.3.0

* It will now attempt to pull drones from storage boxes and stations on the local planet. This can be disabled in the options

### 1.2.0

* Added options to set how many drones/vessels we add instead of only using the maximum. Kept the old options for backwards compatibility

### 1.1.0 

* Changed the way I detect a new station being built to prevent occasional failures

### 1.0.0 

* Initial Release