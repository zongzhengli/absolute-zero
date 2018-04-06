Absolute Zero
=============

Absolute Zero is a bitboard chess engine written in C#. It was developed from scratch to learn about chess programming and game tree searching. By default it runs with its own GUI but it supports the UCI protocol when given the -u command-line parameter. While in UCI/command-line mode it also accepts commands such as perft and divide. 

![Demo image](image.png)

General features:
- Runs with own GUI by default
- Runs in UCI/command-line mode with -u argument
- Accepts perft and divide commands in command-line mode
- Bitboard chess engine that runs in 64-bit when possible

Search features:
- Principal variation search
- Iterative deepening
- Transposition table
- Null move heuristic
- Killer move heuristic
- MVV/LVA heuristic
- Futility pruning
- Late move reductions
- Quiescence search with SEE
- Draw detection
- Mate distance pruning
- Time control heuristics

Evaluation features:
- Phase interpolation
- Piece-square tables
- Mobility evaluation
- Pawn structure evaluation
- Simple capture evaluation
