########################################
# Handles color palette for display
########################################

# Sourcing check
if [ -z $COLOR_PALETTE_SH ]; then

COLOR_PALETTE_SH=1

# Color Palette
  GRAY="\e[1;90m"
 GREEN="\e[1;32m"
   RED="\e[1;31m"
 WHITE="\e[0m"
YELLOW="\e[1;33m"
PURPLE="\e[1;35m"
  BLUE="\e[1;34m"
 BLACK="\e[30m"

RED_BG="\e[41m"
 NO_BG="\e[46m"

fi