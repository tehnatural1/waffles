#
# ~/.bashrc
#

export CLICOLOR=1
# export LSCOLORS=ExFxCxDxBxegedabagacad

export LESS_TERMCAP_mb=$'\e[1;32m'
export LESS_TERMCAP_md=$'\e[1;32m'
export LESS_TERMCAP_me=$'\e[0m'
export LESS_TERMCAP_se=$'\e[0m'
export LESS_TERMCAP_so=$'\e[01;33m'
export LESS_TERMCAP_ue=$'\e[0m'
export LESS_TERMCAP_us=$'\e[1;4;31m'

# If not running interactively, don't do anything
case $- in
    *i*) ;;
      *) return;;
esac

. /Users/dustin/Scripts/gruvbox_256palette.sh

# Set the format of history
HISTTIMEFORMAT="%y/%m/%d %T "

# don't put duplicate lines or lines starting with space in the history.
# See bash(1) for more options
HISTCONTROL=ignoreboth:erasedups

# setting history length see HISTSIZE and HISTFILESIZE in bash(1)
HISTSIZE=100000
HISTFILESIZE=2000000

# append to the history file, don't overwrite it
shopt -s histappend

# check the window size after each command and, if necessary,
# update the values of LINES and COLUMNS.
shopt -s checkwinsize

# maximum amount of folders to display in \w path
PROMPT_DIRTRIM=3

# Alias definitions.
# You may want to put all your additions into a separate file like
# ~/.bash_aliases, instead of adding them here directly.
# See /usr/share/doc/bash-doc/examples in the bash-doc package.
if [ -f ~/.bash_aliases ]; then
    . ~/.bash_aliases
fi

# Color codes for bash prompt
RED="\[\e[0;31m\]"
GRAY="\[\e[0;90m\]"
YELLOW="\[\e[0;33m\]"
BLUE="\[\e[0;34m\]"
WHITE="\[\e[0m\]"
PURPLE="\[\e[0;35m\]"

# Character codes for bash prompt
DOWNBAR='\342\224\214'
HORBAR='\342\224\200'
UPBAR='\342\224\224'
HORBARPLUG='\342\225\274'

end_module() {
    echo "\n"$GRAY$UPBAR$HORBAR$HORBAR$HORBARPLUG $WHITE
}

begin_module() {
    printf '\n'$GRAY$DOWNBAR$HORBAR
}

user_module() {
    echo $HORBAR[$RED'\u'$GRAY]
}

location_module() {
    echo $HORBAR[$BLUE'\w'$GRAY]
}

time_module() {
    echo $HORBAR[$YELLOW'\t'$GRAY]
}

git_module() {
  # Collect branch and commits ahead and behind
  gStatus=$(git status --porcelain=2 -b 2>/dev/null \
            | grep -e 'branch.head' \
                   -e 'branch.ab' \
            | sed  -e 's/# branch.head //g' \
                   -e 's/# branch.ab //g' \
                   -e 's/+//g' \
                   -e 's/-//g')

  if [ ! -z "${gStatus}" ]
  then
    arrOutput=(${gStatus// / })

    branch=${arrOutput[0]}
     ahead=${arrOutput[1]}
    behind=${arrOutput[2]}

    symbol_git_branch=$'\xE2\x91\x82'
      symbol_git_push=$'\xE2\x86\x91'
      symbol_git_pull=$'\xE2\x86\x93'

    output="${symbol_git_branch}${branch}"

    if [ -z ${ahead+x} ] && [ $ahead -gt 0 ]; then
      output+=" ${symbol_git_push}${ahead}"
    fi

    if [ -z ${behind+x} ] && [ $behind -gt 0 ]; then
      output+=" ${symbol_git_pull}${behind}"
    fi

    echo $HORBAR[$PURPLE$output$GRAY]
  fi
}

set_bash_prompt() {
    PS1=$(begin_module)$(user_module)$(time_module)$(location_module)$(git_module)$(end_module)
}


PROMPT_COMMAND=set_bash_prompt
