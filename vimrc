set nocompatible
syntax enable
" filetype on
filetype plugin on
colorscheme gruvbox

let g:C_UseTool_cmake = 'yes'
let g:C_UseTool_doxygen = 'yes'

let g:gruvbox_contrast_dark='hard'
let g:gruvbox_bold=1
let g:gruvbox_italic=1
set background=dark
set termguicolors
" set t_Co=256
let &t_8f = "\e[38;2;%lu;%lu;%lum"
let &t_8b = "\e[48;2;%lu;%lu;%lum"

set laststatus=2
set showcmd

set whichwrap=<,>,[,]
set number
set wrap!
set tabstop=4
set shiftwidth=4
set noexpandtab
set ruler
set mouse=n
set ttymouse=xterm2
" set colorcolumn=81


" Ignore case when searching
set ignorecase
" When searching try to be smart about cases
set smartcase
" Highlight search results
set hlsearch
" Set relative numbers
"set relativenumber

" Folding
set foldmethod=indent
set foldnestmax=2
" disable folding at the begging
" set nofoldenable


let g:gruvbox_termcolors = 16

let g:airline_theme='gruvbox'
let g:airline_powerline_fonts = 1


let NERDTreeMinimalUI = 1
" let NERDTreeDirArrows = 1

let g:NERDTreeFileExtensionHighlightFullName = 1
let g:NERDTreeExactMatchHighlightFullName = 1
let g:NERDTreePatternMatchHighlightFullName = 1

" let g:gitgutter_set_sign_backgrounds = 1

nmap <silent> <C-t> :NERDTreeToggle<CR>





