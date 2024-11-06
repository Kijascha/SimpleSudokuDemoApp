namespace SimpleSudoku.CommonLibrary.System;

public enum Digits
{ None, One, Two, Three, Four, Five, Six, Seven, Eight, Nine, Zero }
public enum SearchUnitType { Row, Column, Box }
public enum GameMode { Play, Create }
[Flags] public enum CandidateMode { None = 0, CenterCandidates = 1 << 0, CornerCandidates = 1 << 1, SolverCandidates = 1 << 2 }