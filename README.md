# PhatStats
A .NET Console app to gather OctoPrint stats to be overlayed on OBS

This is a pretty simple app I put together to run my 3D printers live stream. It has the ability to start and stop the stream as well as get the printer data every second and write it to a text file.

## Required Software
### Visual Studio 2019 - https://visualstudio.microsoft.com/downloads/
### OBS - https://obsproject.com/
### OctoPrint - https://octoprint.org/download/
### OctoPrint Plugin Filament Manager - https://plugins.octoprint.org/plugins/filamentmanager/

## Optional
### PSU Control - https://plugins.octoprint.org/plugins/psucontrol/

## Basic Setup
Run PhatStats once and in the opening message the path to the text file will be displayed. Take that file path as we will need it in OBS.

Install OBS and configure OBS to your liking. Once complete go into the scene for your printer and add a Text element, in there select the From File option and enter the path from before.

While in OBS also go to the Hotkey section in Settings and configure the bindings for Start Stream and Stop Stream.

Once everything is ready turn on PhatStats and it will turn on the stream automatically once a print beings and will turn it off automatically after 30minutes of no print activity. Operations like leveling are not factored into PhatStats as it specifically checks for if the printer is in Printing status or not.

Something I also added for myself is an auto power off for the printer. I use the plugin PSU Control to turn off my printer automatically, but after some updates and adding other plugins the auto off stopped working. So I added it to this instead. After 40 minutes of nothing happening PhatStats will send the power off command for the plugin.

## App.config

### ApiUri
The URL used to access your OctoPrint

### ApiKey
The API Key provided by your OctoPrint install. Details on how to find it can be found here: http://docs.octoprint.org/en/master/api/general.html#authorization

### OBS
The name of the process running OBS. 64bit installs should be obs64 and 32bit should be just obs.

### StartKey
The key bound in OBS to start the stream. Keep to a single key.

### StopKey
The key bound in OBS to stop the stream. Keep to a single key.

### EnableLog
Write everything that is displayed in the Console to a log file when enabled.
