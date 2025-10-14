#!/bin/bash

# Define the commands for each tab
cmd1="midimonster"
cmd2="/home/lichtmaster/apps/ossia.score-3.7.1-linux-x86_64.AppImage"
cmd3="qlcplus"
cmd4="glow /home/lichtmaster/Documents/felix_in_media/README.md"

# Open gnome-terminal with 4 tabs, each running a command
gnome-terminal --tab --title="Midimonster" -- bash -c "$cmd1; exec bash" 
gnome-terminal --tab --title="OssiaScore" -- bash -c "$cmd2; exec bash" 
gnome-terminal --tab --title="qlcplus" -- bash -c "$cmd3; exec bash"
gnome-terminal --tab --title="readme" --geometry=80x43 -- bash -c "$cmd4; exec bash"
