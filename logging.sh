########################################
# Handles console display of messaging
########################################

# Avoid multiple sourcing
if [ -z $LOGGING_SH ]; then

       LOGGING_SH=1
  LOG_LEVEL_ERROR=1
LOG_LEVEL_WARNING=2
   LOG_LEVEL_INFO=3
  LOG_LEVEL_DEBUG=4

display_logging_message()
{
    log_type=$1
    shift
    printf "${GRAY}$(date +%T.%04N)${WHITE}: ${log_type}: $(basename ${BASH_SOURCE[2]}) ${PURPLE}${BASH_LINENO[1]}${WHITE} ${GRAY}->${WHITE} ${YELLOW}${FUNCNAME[2]}${GRAY}()${WHITE}: $*\n"
}

error()
{
    [ ! -z ${VERBOSITY+x} ] && [ $VERBOSITY -ge $LOG_LEVEL_ERROR ] && display_logging_message "[${RED}ERROR${WHITE}]" $*
}

warn()
{
    [ ! -z ${VERBOSITY+x} ] && [ $VERBOSITY -ge $LOG_LEVEL_WARNING ] && display_logging_message "[${YELLOW}WARNING${WHITE}]" $*
}

info()
{
    [ ! -z ${VERBOSITY+x} ] && [ $VERBOSITY -ge $LOG_LEVEL_INFO ] && display_logging_message "[${BLUE}INFO${WHITE}]" $*
}

debug()
{
    [ ! -z ${VERBOSITY+x} ] && [ $VERBOSITY -ge $LOG_LEVEL_DEBUG ] && display_logging_message "[${GREEN}DEBUG${WHITE}]" $*
}

# Source the color palette
[ -f $PROJECT_PATH/SCRIPTS/includes/color_palette.sh ] && \
    source $PROJECT_PATH/SCRIPTS/includes/color_palette.sh || \
    VERBOSITY=1 error "Unable to source color palette"

fi
