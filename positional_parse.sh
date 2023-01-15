########################################
# Handles positional argument parsing
########################################

# Avoid multiple sourcing
if [ -z $POSITIONAL_PARSE_SH ]; then

POSITIONAL_PARSE_SH=1

# Dictionary to hold positional arguments
declare -A ARGD

readArguments()
{
	last_key=""
	while [ $# -gt 0 ]; do
		pos=$1
		case $pos in
			-*=*)
				value="${pos##*=}"
				for (( i=0; i<${#pos}-${#value}; i++ )); do
					arg="${pos:$i:1}"
					key="${pos:$i:$((${#pos}-${#value}-$i-1))}"
					[[ ! "$arg" =~ "-" ]] && [ ! -v ARGD[$key] ] && ARGD[$key]=$value
					[[ ! "$arg" =~ "-" ]] && break
				done
				;;
			--*)
				for (( i=0; i<${#pos}; i++ )); do
					[[ ! "${pos:$i:1}" =~ "-" ]] && break
				done
				key=${pos:$i:$((${#pos}-$i))}
				[ ! -v ARGD[$key] ] && ARGD[$key]=""
				last_key=$key
				;;
			-*)
				for (( i=0; i<${#pos}; i++ )); do
					char="${pos:$i:1}"
					[[ ! "$char" =~ "-" ]] && ARGD[$char]=$((${ARGD[$char]}+1))
				done
				last_key=$char
				;;
			*)
				for (( i=0; i<${#pos}; i++ )); do
					[ ! -v ARGD[$pos] ] && ARGD[$pos]=""
				done
				[ $last_key != "" ] && ARGD[$last_key]=$pos && unset ARGD[$pos]
				last_key=""
				;;
		esac
		shift
	done
}

readArguments $*

# Determine verbosity
[ -v ARGD["v"] ] && VERBOSITY=$((${ARGD["v"]})) || VERBOSITY=-1

fi