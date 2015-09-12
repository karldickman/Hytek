#!/usr/bin/env python2.6
"""The HyTek Meet Manager is the standard program for managing Cross-Country
races.  This module is designed to parse HyTek race results as far as is
possible and dump race results to a HyTek-style report."""

from abc import ABCMeta, abstractproperty
from datetime import timedelta
from formatting import Table
from miscellaneous import typecasted_arithmetic
from re import compile as Regex

#Functions

def load(string):
    """Attempt to parse HyTek race results into a Python dictionary.  Because
    some race results may have different column orders, add new ones, or leave
    standard columns out, this may fail.  If the required information cannot be
    extracted, a LoadError is raised."""
    #Building block patterns
    first_name = "[A-Z]\w*"
    last_name= "[A-Z](\w|')*([ -](\w|')+)?"
    last_first = last_name + ", " + first_name
    first_last = first_name + " " + last_name
    freshman = "F[rR]"
    sophomore = "S[oOpP]"
    junior = "J[rR]"
    senior = "S[rR]"
    year = "|".join([freshman, sophomore, junior, senior])
    #Create a dictionary of the patterns
    field_order = ("place", "bib", "name", "year", "team", "time", "points")
    patterns = dict(place="\d+", bib="#?\d+", year=r"\b(" + year + r")\b",
                    name=last_first + "|" + first_last, team="[A-Z]\D*",
                    time="\d+:\d\d(\.\d{1,2})", points=r"\d+")
    #Apply names to all the patterns
    for field, pattern in patterns.iteritems():
        patterns[field] = "(?P<%s>%s)" % (field, pattern)
        #These fields are optional
        if field in ("bib", "year", "points"):
            patterns[field] += "?"
    row = "\s*".join(patterns[field] for field in field_order)
    pattern = Regex(row)
    #Apply regular expressions
    cleanup = {"place": int, "team": str.strip, "points": int, "time":
               RaceTime.from_string}
    results = []
    for i, line in enumerate(string.splitlines()):
        if len(line) == 0 or line.isspace():
            continue
        match = pattern.search(line)
        if match is None:
            raise LoadError("Line %d: \"%s\" does not match /%s/." %
                            (i + 1, line, row))
        results.append(Finisher(None, None))
        for field in patterns.iterkeys():
            value = match.group(field)
            try:
                setattr(results[-1], field, cleanup[field](value))
            except (AttributeError, KeyError, TypeError):
                setattr(results[-1], field, value)
    return results

def dump(results, scores=None, distance=None):
    """Dump the given meet score and results to a string.  It is recommended
    that the results argument be a list of IFinisher instances and the scores
    argument be a list of ITeam instances."""
    """Dump the given meet score and results to a string."""
    return "\n".join(idump(results, scores, distance))

def idump(results, scores=None, distance=None):
    """Dump the given meet score and results to an iterator.  It is recommended
    that the results argument be a list of IFinisher instances and the scores
    argument be a list of ITeam instances."""
    iterator = ResultsDumper(results, distance)
    print iterator.column_seperator
    for row in iterator:
    #for row in ResultsDumper(results, distance):
        yield row
    if scores is not None:
        yield ""
        for row in ScoreDumper(scores):
            yield row

#Interfaces

class IFinisher(object):
    """Interface representing finishers of a race."""
    __metaclass__ = ABCMeta
    place = abstractproperty()
    name = abstractproperty()
    year = abstractproperty()
    team = abstractproperty()
    time = abstractproperty()
    points = abstractproperty()

class ITeam(object):
    """Interface representing teams in a race."""
    __metaclass__ = ABCMeta
    place = abstractproperty()
    name = abstractproperty()
    score = abstractproperty()
    finishers = abstractproperty()
    top_five = abstractproperty()
    top_seven = abstractproperty()

#Classes

class Finisher(IFinisher):
    """Simple implementation of the IFinisher interface."""
    __slots__ = ["place", "name", "team", "time", "points", "year"]
    def __init__(self, name, time, year=None, team=None, place=None,
                 points=None):
        self.name = name
        self.year = year
        self.time = time
        self.place = place
        self.team = team
        self.points = points

    def __repr__(self):
        return "Finisher(%s, %s, %s, %s, %s, %s)" % (repr(self.name),
                                                     repr(self.time),
                                                     repr(self.year),
                                                     repr(self.team),
                                                     repr(self.place),
                                                     repr(self.points))

class RaceTime(timedelta):
    """The time it took for a runner to finish a race."""
    def __new__(cls, seconds):
        instance = super(RaceTime, cls).__new__(cls, 0, seconds)
        return instance

    def __repr__(self):
        return "RaceTime(%d, %d)" % (self.seconds, self.microseconds)

    def __str__(self):
        minutes = str(self.seconds // 60)
        seconds = str(self.seconds % 60)
        fraction = str(self.microseconds // 10000)
        if len(seconds) < 2:
            seconds = "0" + seconds
        if len(fraction) < 2:
            fraction = "0" + fraction
        return "%s:%s.%s" % (minutes, seconds, fraction)

    @classmethod
    def from_string(cls, string):
        """Construct a RaceTime instance from a string.

        >>> RaceTime.from_string("24:44.8")
        RaceTime(1484, 800000)"""
        try:
            if ":" not in string:
                minutes = 0
                seconds = float(string)
            else:
                minutes, seconds = string.split(":")
                minutes = int(minutes)
                seconds = float(seconds)
                if seconds >= 60:
                    raise TypeError
        except ValueError, error:
            raise TypeError(error)
        return cls(minutes * 60 + seconds)

    @classmethod
    def from_superclass(cls, tdobject):
        """Construct a RaceTime instance from a datetime.timedelta instance.

        >>> RaceTime.from_superclass(datetime.timedelta(0, 24*60+44.8))
        RaceTime(1484, 800000)"""
        value = tdobject.seconds + tdobject.microseconds / 1000000
        return cls(value)

RaceTime = typecasted_arithmetic(RaceTime)

class DefaultTable(Table):
    """Default HyTek table, for subclassing."""

    def __init__(self, rows, label=None, headings=None, pads=None):
        super(DefaultTable, self).__init__(rows, label, headings,
                                           top_border="=", body_top="=",
                                           pads=pads)

class ResultsDumper(DefaultTable):
    """Dump a list of race results to a HyTek-style report.  Iterating over an
    instance of this class produces the the report line-by-line."""
    def __init__(self, results, distance=None):
        headings = [None, "Name", "Year", "School", "Finals", "Points"]
        pads = [str.rjust, None, None, None, None, lambda string, width:
                    string.rjust(4).ljust(width)]
        rows = [[runner.place, runner.name, runner.year, runner.team,
                 runner.time, runner.points] for runner in results]
        label = "%d m run CC" % distance if distance is not None else None
        super(ResultsDumper, self).__init__(rows, label, headings, pads)

class ScoreDumper(DefaultTable):
    """Dump the scoring information of a race to a HyTek-style report."""

    def __init__(self, scores):
        self.scores = scores
        label = "Team Scores"
        headings = ["Rank", "Team".ljust(len("  Top 5 Avg:  dd:dd.dd ")),
                    "Total"]
        headings += [str(num).rjust(4) for num in xrange(1, 6)]
        headings += [("*" + str(num)).rjust(4) for num in xrange(6, 8)]
        pads = [str.rjust] * 10
        pads[1] = None
        rows = []
        for team in scores:
            new_row = [team.place, team.name, team.score]
            new_row += [runner.points for runner in team.finishers]
            diff = 7 - len(team.finishers)
            if diff > 0:
                new_row += [None] * diff
            rows.append(new_row)
        super(ScoreDumper, self).__init__(rows, label, headings, pads)

    def __iter__(self):
        """Yields each of the lines of the report one at a time."""
        for row in self.iheader():
            yield row
        for row, team in zip(self.ibody(), self.scores):
            yield row
            yield "       Top 5 Avg:  " + str(team.top_five)
            if team.top_seven is not None:
                yield "       Top 7 Avg:  " + str(team.top_seven)

#Exceptions

class LoadError(Exception): pass

#******************************************************************************
#******************************** UNIT TESTS **********************************
#******************************************************************************

from py.test import raises

def test_load_good_files():
    nwc = """
    1 Reynolds, Francis            Puget Sound           25:00.71    1
    2 Castillo, Leo                Willamette            25:21.38    2
    3 Parker, Matt                 Willamette            25:24.27    3
    4 Redfield, Stefan             Willamette            25:35.76    4
    5 Dickman, Karl                Lewis & Clark         25:39.35    5
    6 Fisher, Shawn                Linfield College      25:47.41    6
    7 McIsaac, Chris               Linfield College      25:53.22    7
    8 Rebol, Nick                  Willamette            25:55.10    8
    9 Jenkins, Aaron               Whitworth             25:58.83    9
    10 Dudley, Tyler                Whitworth             26:00.72   10
    11 Rand, Cory                   Whitman College       26:03.20   11
    12 Roberts, John                Lewis & Clark         26:09.10   12
    13 Davis, Tyler                 Linfield College      26:09.78   13
    14 Caseria, Dusty               Whitworth             26:10.56   14
    15 Platano, Chris               Willamette            26:15.49   15
    16 McLaughlin, Ryan             Willamette            26:22.06   16
    17 Aubol, Kevin                 Willamette            26:23.94   17
    18 Gallagher, Nicholas          Whitworth             26:30.72   18
    19 Eberhart, Cameron            George Fox            26:34.71   19
    20 Stewart, Collin              Whitworth             26:40.91   20
    21 Donovan, Ben                 Willamette            26:43.73
    22 Berrian, Trevor              Whitworth             26:50.68   21
    23 Smith, Nathan                Willamette            26:51.67
    24 Sharma, Sean                 Willamette            26:58.43
    25 Parker, Hugh                 Whitman College       27:07.11   22
    26 Phillips, John               Pacific Lutheran      27:14.10   23
    27 Hennessey, Sam               Whitman College       27:14.72   24
    28 Villasenor, Alfredo          Whitman College       27:18.41   25
    29 Andrascik, Sean              Pacific Lutheran      27:18.80   26
    30 Deardorff, Joseph            Pacific University    27:21.72   27
    31 Davis, Mark                  Whitworth             27:22.97   28
    32 Reid, Curtis                 Whitman College       27:28.89   29
    33 Anderson, Arian              Linfield College      27:29.96   30
    34 Weiss, Asa                   Lewis & Clark         27:31.40   31
    35 Baldridge, Jesse             Puget Sound           27:32.66   32
    36 Weinbender, Eric             Linfield College      27:42.17   33
    37 Bras, Orion                  Pacific Lutheran      27:43.03   34
    38 Gage, Scott                  Linfield College      27:44.74   35
    39 Snowden, Robert              Puget Sound           27:45.09   36
    40 Smith, Samuel                Lewis & Clark         27:51.59   37
    41 Barth, Justin                Pacific Lutheran      27:52.65   38
    42 Gillem, John                 Pacific University    27:54.23   39
    43 Kelly, Matthew               Whitman College       27:54.63   40
    44 Luecke, Daniel               Whitman College       27:57.30   41
    45 Rapet, Paul                  George Fox            27:57.82   42
    46 VanSlyke, Alex               Linfield College      27:59.59   43
    47 Boyer, Brendan               Whitman College       28:04.40
    48 Butler, Cameron              Puget Sound           28:06.76   44
    49 Bollen, Barrett              Pacific Lutheran      28:10.81   45
    50 Fikak, Yonas                 Whitman College       28:13.23
    51 Larson, Jonathan             Pacific University    28:14.25   46
    52 Eifert, Christian            Whitworth             28:16.57
    53 Battaglia, Lucian            Linfield College      28:22.11
    54 Callow, John                 Whitman College       28:23.03
    55 Wall, Casey                  Puget Sound           28:24.55   47
    56 Horton, Anthony              Pacific Lutheran      28:26.95   48
    57 Cushman, John                Pacific University    28:27.56   49
    58 Grigsby, Kolter              Pacific Lutheran      28:30.31   50
    59 Flora, Daniel                Pacific University    28:40.32   51
    60 Martin, Austin               Pacific Lutheran      28:47.36
    61 Burger, Adam                 Pacific University    28:54.09   52
    62 Erickson, Ryan               George Fox            29:00.58   53
    63 Cassel, Allen                George Fox            29:04.88   54
    64 Klein, Matt                  Puget Sound           29:09.47   55
    65 Fisher, Peter                Lewis & Clark         29:11.55   56
    66 Sutfin, Chad                 George Fox            29:14.29   57
    67 Shaver, Daniel               Lewis & Clark         29:14.85   58
    68 Porter, Wesley               Pacific University    29:27.97   59
    69 Allen-Slaba, Nathaniel       Pacific Lutheran      29:35.21
    70 Polley, Shane                Whitworth             29:37.01
    71 Page, Nathan                 Pacific Lutheran      29:37.38
    72 Morrell, Austin              George Fox            29:39.32   60
    73 Calavan, Mike                George Fox            29:55.43   61
    74 Graham, Patrick              Puget Sound           30:08.67   62
    75 Nishimura, Casey             Pacific University    30:19.56
    76 Nevarez, Jose                Lewis & Clark         30:44.43   63
    77 Church, Jason                Linfield College      31:01.34
    78 Miles, Nic                   Linfield College      31:27.42
    79 Cooper, Evan                 Pacific University    31:44.78
    """
    load("")
    runners = load(nwc)
    winner = runners[0]
    assert winner.name == "Reynolds, Francis"
    assert winner.time == RaceTime(1500.71)
    assert winner.year == None
    assert winner.team == "Puget Sound"
    assert winner.place == 1
    assert winner.points == 1
    reg09m = """
    1 #278 Jackson Brainerd     SO Colorado College      25:26.65    1
    2 #323 Eric Kleinsasser     SO Occidental            25:26.81    2
    3 #345 Francis Reynolds     SR Puget Sound           25:46.49
    4 #276 Kramer Straube       JR Claremont-Mudd-S      25:49.09    3
    5 #384 Matt Parker          JR Willamette            25:51.52    4
    6 #272 Brian Kopczynski     JR Claremont-Mudd-S      25:51.63    5
    7 #310 Shawn Fisher         SR Linfield              25:56.21    6
    8 #387 Stefan Redfield      JR Willamette            26:00.05    7
    9 #275 Florian Scheulen     SR Claremont-Mudd-S      26:01.07    8
    10 #250 Ray Ostrander        JR Cal Lutheran          26:02.39    9
    11 #376 Aaron Jenkins        SO Whitworth             26:09.04   10
    12 #375 Nicholas Gallagher   JR Whitworth             26:15.19   11
    13 #381 Leo Castillo         SO Willamette            26:16.52   12
    14 #277 Brian Sutter         FR Claremont-Mudd-S      26:16.95   13
    15 #298 Karl Dickman         SR Lewis & Clark         26:17.87   14
    16 #342 John Mering          SR Pomona-Pitzer         26:21.86   15
    17 #373 Tyler Dudley         SO Whitworth             26:23.00   16
    18 #284 Daniel Kraft         JR Colorado College      26:24.03   17
    19 #286 Andrew Wagner        JR Colorado College      26:26.00   18
    20 #385 Chris Platano        SR Willamette            26:29.85   19
    21 #341 Alex Johnson         FR Pomona-Pitzer         26:31.81   20
    22 #271 Georgi Dinolov       JR Claremont-Mudd-S      26:37.37   21
    23 #386 Nick Rebol           JR Willamette            26:41.82   22
    24 #340 Brian Gillis         SR Pomona-Pitzer         26:43.80   23
    25 #288 Cameron Eberhart     SR George Fox            26:45.43
    26 #257 Alan Menezes         FR Caltech               26:51.25   24
    27 #274 Matt Kurtis          SR Claremont-Mudd-S      26:52.96   25
    28 #379 Collin Stewart       SR Whitworth             26:54.79   26
    29 #349 Jeremy Kalmus        JR Redlands              26:57.19   27
    30 #287 David Wilder         SO Colorado College      26:58.74   28
    31 #283 Max Gerken           SO Colorado College      26:59.87   29
    32 #344 Hale Shaw            SO Pomona-Pitzer         27:00.96   30
    33 #254 Cameron Fen          FR Caltech               27:04.68   31
    34 #370 Trevor Berrian       FR Whitworth             27:08.80   32
    35 #338 Charles Enscoe       JR Pomona-Pitzer         27:08.93   33
    36 #383 Ryan McLaughlin      JR Willamette            27:12.24   34
    37 #311 Scott Gage           SO Linfield              27:13.06   35
    38 #337 Anders Crabo         SO Pomona-Pitzer         27:14.77   36
    39 #301 John Roberts         SO Lewis & Clark         27:21.27   37
    40 #360 Curtis Reid          SR Whitman               27:24.82   38
    41 #305 Asa Weiss            SR Lewis & Clark         27:26.47   39
    42 #268 Kris Brown           JR Claremont-Mudd-S      27:28.47   40
    43 #371 Dusty Caseria        SR Whitworth             27:29.68   41
    44 #347 Jake Baechle         SR Redlands              27:30.18   42
    45 #346 Duncan Ashby         SO Redlands              27:36.53   43
    46 #372 Mark Davis           SO Whitworth             27:39.81   44
    47 #336 Paul Balmer          SO Pomona-Pitzer         27:41.34   45
    48 #280 Michael Dougan       SO Colorado College      27:42.56   46
    49 #312 Chris McIsaac        SR Linfield              27:43.83   47
    50 #380 Kevin Aubol          FR Willamette            27:46.79   48
    51 #361 Alfredo Villasenor   FR Whitman               27:49.43   49
    52 #322 Victor Kali          SR Occidental            27:49.47   50
    53 #309 Tyler Davis          SR Linfield              27:52.47   51
    54 #303 Samuel Smith         FR Lewis & Clark         27:55.69   52
    55 #248 Brian Kahovec        SR Cal Lutheran          27:56.65   53
    56 #291 Jasper Chang         SR La Verne              27:56.84   54
    57 #304 Lars Steier          SR Lewis & Clark         28:03.95   55
    58 #329 Joseph Deardorff     FR Pacific (Ore.)        28:03.98   56
    59 #296 Leo Martinez         SO La Verne              28:04.47   57
    60 #317 Thomas Cahuzac       SO Occidental            28:05.89   58
    61 #359 Cory Rand            FR Whitman               28:07.99   59
    62 #314 Alex VanSlyke        SO Linfield              28:09.76   60
    63 #297 Matthew Sustayta     FR La Verne              28:10.74   61
    64 #306 Arian Anderson       SO Linfield              28:12.50   62
    65 #290 Oscar Castro         FR La Verne              28:17.58   63
    66 #318 Mario Castillo       FR Occidental            28:24.03   64
    67 #355 Matt Kelly           SR Whitman               28:26.85   65
    68 #358 Hugh Parker          FR Whitman               28:27.03   66
    69 #354 Sam Hennessey        JR Whitman               28:27.66   67
    70 #252 Stephen Shirk        SO Cal Lutheran          28:27.95   68
    71 #332 Jonathan Larson      FR Pacific (Ore.)        28:35.71   69
    72 #319 Sebi Devlin-Foltz    SO Occidental            28:37.43   70
    73 #251 Evan Reed            SO Cal Lutheran          28:44.01   71
    74 #285 Andrew Vierra        FR Colorado College      28:45.71   72
    75 #351 Aaron Minsk          SO Redlands              28:47.01   73
    76 #295 Sean Kusick          FR La Verne              28:54.66   74
    77 #292 Alex Forbess         FR La Verne              28:55.10   75
    78 #261 Chris Cresci         SO Chapman               28:59.56   76
    79 #363 Juan Bustos          SO Whittier              29:01.06   77
    80 #331 John Gillem          SO Pacific (Ore.)        29:02.45   78
    81 #348 Alec Fillmore        JR Redlands              29:09.36   79
    82 #258 Andy Zucker          FR Caltech               29:11.82   80
    83 #293 Marcus Fortugno      SO La Verne              29:14.12   81
    84 #356 Daniel Luecke        JR Whitman               29:15.37   82
    85 #330 Daniel Flora         FR Pacific (Ore.)        29:19.06   83
    86 #315 Eric Weinbender      SO Linfield              29:21.99   84
    87 #247 Brett Halvaks        SO Cal Lutheran          29:29.90   85
    88 #362 Travis Airola        SO Whittier              29:38.48   86
    89 #364 Bryce Holewinski     JR Whittier              29:39.80   87
    90 #299 Peter Fisher         FR Lewis & Clark         29:42.24   88
    91 #255 Andrew Gong          SO Caltech               29:46.96   89
    92 #365 Vega Jordan          FR Whittier              29:52.60   90
    93 #302 Daniel Shaver        FR Lewis & Clark         29:56.83   91
    94 #324 Avery Mainardi       FR Occidental            30:24.94   92
    95 #328 John Cushman         SO Pacific (Ore.)        30:28.22   93
    96 #262 Angel Flores         FR Chapman               30:41.04   94
    97 #350 Tom Marshall         FR Redlands              30:55.38   95
    98 #325 Charlie Sauter       FR Occidental            30:56.00   96
    99 #366 Marco Leone          SO Whittier              31:03.73   97
    100 #256 Ryan Keeley          SO Caltech               31:16.39   98
    101 #326 Adam Burger          FR Pacific (Ore.)        31:53.42   99
    102 #264 Craig McGirr         FR Chapman               32:24.41  100
    103 #267 Nathan Worden        FR Chapman               33:58.47  101
    104 #260 Sergi Casamitjana    JR Chapman               34:29.63  102
    105 #266 Cale Skagen          FR Chapman               35:08.82  103
    """
    runners = load(reg09m)
    winner = runners[0]
    assert winner.name == "Jackson Brainerd"
    assert winner.time == RaceTime(25*60+26.65)
    assert winner.year == "SO"
    assert winner.team == "Colorado College"
    assert winner.place == 1
    assert winner.points == 1
    assert winner.bib == "#278"
    assert runners[2].name == "Francis Reynolds"
    reg09w = """
    1 #186 Alicia Freese        SR Pomona-Pitzer         22:07.60    1
    2 #126 Jennifer Tave        SO Claremont-Mudd-S      22:10.70    2
    3 #230 Dana Misterek        JR Whitworth             22:10.74    3
    4 #162 Marci Klimek         SR Linfield              22:23.08    4
    5 #218 Michele Callaway     SO Whittier              22:27.77    5
    6 #233 Joy Shufeldt         FR Whitworth             22:29.25    6
    7 #203 Mikayla Murphy       JR UC Santa Cruz         22:39.86    7
    8 #235 Tonya Turner         JR Whitworth             22:44.39    8
    9 #242 Tina Patel           SR Willamette            22:54.23    9
    10 #190 Hayley Walker        SO Puget Sound           22:56.10
    11 #124 Julia Rigby          SO Claremont-Mudd-S      22:58.87   10
    12 #208 Yasmeen Colis        SR Whitman               23:01.15   11
    13 #236 Kathryn Williams     JR Whitworth             23:03.28   12
    14 #187 Rose Haag            SR Pomona-Pitzer         23:12.36   13
    15 #241 Kimber Mattox        SO Willamette            23:13.81   14
    16 #214 Sara McCune          SR Whitman               23:19.60   15
    17 #173 Grace Peck           SR Occidental            23:20.81   16
    18 #105 Toccoa Kahovec       SO Cal Lutheran          23:23.26   17
    19 #207 Kristen Ballinger    JR Whitman               23:30.68   18
    20 #183 Roxanne Cook         FR Pomona-Pitzer         23:35.70   19
    21 #229 Jo E Mayer           SR Whitworth             23:41.36   20
    22 #172 Sadie Mohler         SO Occidental            23:43.54   21
    23 #194 Katie Ostrinski      JR Redlands              23:45.98   22
    24 #160 Nelly Evans          SO Linfield              23:46.30   23
    25 #158 Frances Corcorran    SR Linfield              23:46.77   24
    26 #155 Heather Spurling     FR Lewis & Clark         23:46.92   25
    27 #166 Anna Dalton          SO Occidental            23:48.66   26
    28 #154 Hannah Palmer        JR Lewis & Clark         23:49.53   27
    29 #192 Heather Mayer        JR Redlands              23:51.08   28
    30 #167 Eliza Dornbush       SO Occidental            23:53.72   29
    31 #122 Breanna Deutsch      SO Claremont-Mudd-S      23:54.49   30
    32 #215 Heather O'Moore      SR Whitman               23:55.62   31
    33 #156 Emily Thomas         FR Lewis & Clark         23:59.88   32
    34 #209 Michela Corcorran    SR Whitman               24:01.12   33
    35 #188 Rachel Haislet       SR Pomona-Pitzer         24:01.76   34
    36 #219 Molly Litherland     SO Whittier              24:03.32   35
    37 #177 Amanda Basham        SO Pacific (Ore.)        24:04.89   36
    38 #225 Christine Verduzco   FR Whittier              24:04.95   37
    39 #202 Jessica Meyer        FR UC Santa Cruz         24:06.67   38
    40 #239 Kaitlin Greene       SO Willamette            24:12.17   39
    41 #170 Megan Lang           FR Occidental            24:14.01   40
    42 #125 Ashley Scott         JR Claremont-Mudd-S      24:14.70   41
    43 #161 Gretchen George      SO Linfield              24:15.43   42
    44 #243 Amanda Tamanaha      FR Willamette            24:15.54   43
    45 #103 Nicole Flanary       JR Cal Lutheran          24:15.83   44
    46 #121 Kate Crawford        FR Claremont-Mudd-S      24:16.18   45
    47 #193 Stephanie Mera       SR Redlands              24:18.14   46
    48 #150 Kirsten Fix          SR Lewis & Clark         24:19.64   47
    49 #211 Emilie Gilbert       FR Whitman               24:21.66   48
    50 #128 Aubrey Zimmerling    FR Claremont-Mudd-S      24:23.04   49
    51 #226 Candace Wray         SO Whittier              24:24.70   50
    52 #165 Charlotte Trowbridge  SR Linfield              24:25.75   51
    53 #127 Laura Wyatt          FR Claremont-Mudd-S      24:27.80   52
    54 #216 Emily Rodriguez      SR Whitman               24:27.87   53
    55 #191 Kelly Luck           SR Redlands              24:30.35   54
    56 #130 Maggie Harkins       SO Colorado College      24:33.35   55
    57 #244 Alisha Till          FR Willamette            24:43.25   56
    58 #152 Corinne Innes        SO Lewis & Clark         24:46.69   57
    59 #197 Madison Smith        FR Redlands              24:48.56   58
    60 #228 Kaitlin Hildebrand   SR Whitworth             24:50.97   59
    61 #231 Emily Morehouse      SR Whitworth             24:52.08   60
    62 #147 Raven Campbell       JR Lewis & Clark         24:54.26   61
    63 #204 Rebecca Parsons      FR UC Santa Cruz         25:01.23   62
    64 #185 Kayla Eland          SO Pomona-Pitzer         25:01.74   63
    65 #237 Theresa Edwards      FR Willamette            25:04.67   64
    66 #224 Guadalupe Ulloa      FR Whittier              25:05.09   65
    67 #240 Megan Horning        JR Willamette            25:06.71   66
    68 #132 Megan Hurster        SO Colorado College      25:08.06   67
    69 #221 Darlene Partida      SR Whittier              25:13.56   68
    70 #180 Samantha Lee         JR Pacific (Ore.)        25:16.48   69
    71 #174 Tara Saxena          FR Occidental            25:18.81   70
    72 #189 Zoe Meyers           SR Pomona-Pitzer         25:20.51   71
    73 #129 Margot Cutter        SO Colorado College      25:23.54   72
    74 #109 Justine Chia         JR Caltech               25:25.48   73
    75 #140 Brigitte Blazys      JR La Verne              25:32.02   74
    76 #205 Kelsey Shields       SO UC Santa Cruz         25:37.91   75
    77 #142 Micaela Castillo     SO La Verne              25:38.95   76
    78 #101 Lynn Clahassey       JR Cal Lutheran          25:43.24   77
    79 #196 Vainayaki Sivaji     FR Redlands              25:45.75   78
    80 #182 Kate Brieger         JR Pomona-Pitzer         25:47.24   79
    81 #157 Jill Boroughs        FR Linfield              25:48.04   80
    82 #206 Hailey Stiers        JR UC Santa Cruz         25:48.13   81
    83 #222 Jamie Slingluff      SO Whittier              25:49.15   82
    84 #111 Sylvia Sullivan      SO Caltech               25:51.62   83
    85 #178 Hayley Brusewitz     FR Pacific (Ore.)        25:51.82   84
    86 #198 Seana Thompson       FR Redlands              25:54.72   85
    87 #176 Lauren Barnard       SO Pacific (Ore.)        25:58.61   86
    88 #148 Kelsey Croall        JR Lewis & Clark         25:59.19   87
    89 #163 Rosika Nees          FR Linfield              26:01.30   88
    90 #110 Clara Eng            SO Caltech               26:05.69   89
    91 #199 Danielle Breski      JR UC Santa Cruz         26:09.43   90
    92 #117 Angelica Hernandez   SO Chapman               26:15.12   91
    93 #134 Molly McGee          JR Colorado College      26:15.53   92
    94 #146 Sydney Rose          FR La Verne              26:15.76   93
    95 #104 Michelle Horgan      JR Cal Lutheran          26:16.01   94
    96 #181 Whitney Nelson       JR Pacific (Ore.)        26:21.51   95
    97 #179 Jilinda Franklin     FR Pacific (Ore.)        26:33.90   96
    98 #102 Caitlin Coomber      SO Cal Lutheran          26:35.98   97
    99 #119 Kirsten Moore        JR Chapman               27:05.26   98
    100 #118 Amanda Kristedja     FR Chapman               27:15.75   99
    101 #106 Masha Belyi          SR Caltech               27:20.58  100
    102 #200 Jenny Cain           SO UC Santa Cruz         27:23.23  101
    103 #138 Rebecca Thompson     SO Colorado College      27:34.76  102
    104 #131 Chelsea Herzog       JR Colorado College      27:36.32  103
    105 #175 Meghan Whalen        SO Occidental            27:43.37  104
    106 #107 Nina Budaeva         FR Caltech               27:55.26  105
    107 #143 Stephanie Fuentes    SR La Verne              28:05.81  106
    108 #133 Georgia Ivsin        SO Colorado College      28:15.93  107
    109 #141 Guadalupe Camberos   FR La Verne              28:58.78  108
    110 #139 Amber Blackshear     JR La Verne              29:20.64  109
    111 #113 Jessie Drews         FR Chapman               29:41.99  110
    112 #114 Jillian Freitas      JR Chapman               32:09.15  111
    113 #116 Katherine Hendricks  FR Chapman               32:18.20  112
    """
    runners = load(reg09w)
    runner = runners[31]
    assert runner.name == "Heather O'Moore"
    assert runner.time == RaceTime(23*60+55.62)
    assert runner.year == "SR"
    assert runner.team == "Whitman"
    assert runner.place == 32
    assert runner.points == 31
    assert runner.bib == "#215"

def test_load_bad_files():
    raises(LoadError, load, "This is a whole big load of nonsense.")

def test_race_time_from_string_good():
    assert RaceTime.from_string("0:0") == RaceTime(0)
    assert RaceTime.from_string("24:44.80") == RaceTime(24*60+44.8)
    assert RaceTime.from_string("1") == RaceTime(1)
    assert RaceTime.from_string("777") == RaceTime(777)

def test_race_from_string_bad():
    raises(TypeError, "RaceTime.from_string(':0')")
    raises(TypeError, "RaceTime.from_string(':')")
    raises(TypeError, "RaceTime.from_string('33:44:70')")
    raises(TypeError, "RaceTime.from_string('33.44:70')")
    raises(TypeError, "RaceTime.from_string('0:777')")
