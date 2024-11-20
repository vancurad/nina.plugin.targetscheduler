/* */

CREATE TABLE IF NOT EXISTS "profilepreference" (
	"Id"									INTEGER NOT NULL,
	"profileId"								TEXT NOT NULL,
	"parkonwait"							INTEGER DEFAULT 0,
	"enableSmartPlanWindow"					INTEGER DEFAULT 1,
	"exposurethrottle"						REAL DEFAULT 125,
	"enableDeleteAcquiredImagesWithTarget"	INTEGER DEFAULT 1,
	"delayGrading"							REAL DEFAULT 80,
	"acceptimprovement"						INTEGER DEFAULT 1,
	"maxGradingSampleSize"					INTEGER,
	"enableMoveRejected"					INTEGER DEFAULT 0,
	"enableGradeRMS"						INTEGER,
	"enableGradeStars"						INTEGER,
	"enableGradeHFR"						INTEGER,
	"enableGradeFWHM"						INTEGER DEFAULT 0,
	"enableGradeEccentricity"				INTEGER DEFAULT 0,
	"rmsPixelThreshold"						REAL,
	"detectedStarsSigmaFactor"				REAL,
	"hfrSigmaFactor"						REAL,
	"fwhmSigmaFactor"						INTEGER DEFAULT 4,
	"eccentricitySigmaFactor"				INTEGER DEFAULT 4,
	"enableSynchronization"					INTEGER DEFAULT 0,
	"syncWaitTimeout"						INTEGER DEFAULT 300,
	"syncActionTimeout"						INTEGER DEFAULT 300,
	"syncSolveRotateTimeout"				INTEGER DEFAULT 300,
	"syncEventContainerTimeout"				INTEGER DEFAULT 300,
	PRIMARY KEY("id")
);

CREATE TABLE IF NOT EXISTS "project" (
	"Id"						INTEGER NOT NULL,
	"profileId"					TEXT NOT NULL,
	"name"						TEXT NOT NULL,
	"description"				TEXT,
	"state"						INTEGER,
	"priority"					INTEGER,
	"createdate"				INTEGER,
	"activedate"				INTEGER,
	"inactivedate"				INTEGER,
	"isMosaic"					INTEGER NOT NULL DEFAULT 0,
	"flatsHandling"				INTEGER NOT NULL DEFAULT 0,
	"minimumtime"				INTEGER,
	"minimumaltitude"			REAL,
	"usecustomhorizon"			INTEGER,
	"horizonoffset"				REAL,
	"meridianwindow"			INTEGER,
	"filterswitchfrequency"		INTEGER,
	"ditherevery"				INTEGER,
	"enablegrader"				INTEGER,
	PRIMARY KEY("id")
);

CREATE TABLE IF NOT EXISTS "ruleweight" (
	"Id"						INTEGER NOT NULL,
	"projectid"					INTEGER,
	"name"						TEXT NOT NULL,
	"weight"					REAL NOT NULL,
	PRIMARY KEY("Id"),
	FOREIGN KEY("projectId") REFERENCES "project"("Id")
);

CREATE TABLE IF NOT EXISTS "target" (
	"Id"						INTEGER NOT NULL,
	"projectid"					INTEGER,
	"name"						TEXT NOT NULL,
	"active"					INTEGER NOT NULL,
	"ra"						REAL,
	"dec"						REAL,
	"epochcode"					INTEGER NOT NULL,
	"rotation"					REAL,
	"roi"						REAL,
	PRIMARY KEY("id"),
	FOREIGN KEY("projectId") REFERENCES "project"("Id")
);

CREATE TABLE IF NOT EXISTS "exposureplan" (
	"Id"						INTEGER NOT NULL,
	"profileId"					TEXT NOT NULL,
	"targetid"					INTEGER,
	"exposureTemplateId"		INTEGER,
	"exposure"					REAL NOT NULL,
	"desired"					INTEGER,
	"acquired"					INTEGER,
	"accepted"					INTEGER,
	PRIMARY KEY("Id"),
	FOREIGN KEY("exposureTemplateId") REFERENCES "exposuretemplate"("Id"),
	FOREIGN KEY("targetId") REFERENCES "target"("Id")
);

CREATE TABLE IF NOT EXISTS "exposuretemplate" (
	"Id"						INTEGER NOT NULL,
	"profileId"					TEXT NOT NULL,
	"name"						TEXT NOT NULL,
	"filtername"				TEXT NOT NULL,
	"defaultexposure"			REAL DEFAULT 60,
	"gain"						INTEGER,
	"offset"					INTEGER,
	"bin"						INTEGER,
	"readoutmode"				INTEGER,
	"twilightlevel"				INTEGER,
	"maximumhumidity"			REAL,
	"moonavoidanceenabled"		INTEGER,
	"moonavoidanceseparation"	REAL,
	"moonavoidancewidth"		INTEGER,
	"moonrelaxscale"			REAL DEFAULT 0,
	"moonrelaxmaxaltitude"		REAL DEFAULT 5,
	"moonrelaxminaltitude"		REAL DEFAULT -15,
	"moondownenabled"			INTEGER DEFAULT 0,
	PRIMARY KEY("Id")
);

CREATE TABLE IF NOT EXISTS "overrideexposureorder" (
   "Id"				INTEGER NOT NULL,
   "targetid"		INTEGER,
   "order"			INTEGER NOT NULL,
   "action"			INTEGER NOT NULL,
   "referenceIdx"	INTEGER,
   PRIMARY KEY("Id"),
   FOREIGN KEY("targetId") REFERENCES "target"("Id")
);

CREATE TABLE IF NOT EXISTS "acquiredimage" (
	"Id"			INTEGER NOT NULL,
	"projectId"		INTEGER NOT NULL,
	"targetId"		INTEGER NOT NULL,
	"acquireddate"	INTEGER,
	"filtername"	TEXT NOT NULL,
	"accepted"		INTEGER NOT NULL,
    "rejectreason"	TEXT,
    "metadata"		TEXT NOT NULL,
	PRIMARY KEY("Id")
);

CREATE TABLE IF NOT EXISTS "imagedata" (
	"Id"			INTEGER NOT NULL,
	"tag"			TEXT,
	"imagedata"		BLOB,
	"acquiredimageid"	INTEGER,
	FOREIGN KEY("acquiredImageId") REFERENCES "acquiredimage"("Id"),
	PRIMARY KEY("Id")
);

CREATE TABLE IF NOT EXISTS "flathistory" (
	"Id"				INTEGER NOT NULL,
	"profileId"			TEXT NOT NULL,
	"targetId"			INTEGER,
	"lightSessionId"	INTEGER NOT NULL DEFAULT 0,
	"lightSessionDate"	INTEGER,
	"flatsTakenDate"	INTEGER,
	"flatsType"			TEXT,
	"filterName"		TEXT,
	"gain"				INTEGER,
	"offset"			INTEGER,
	"bin"				INTEGER,
	"readoutmode"		INTEGER,
	"rotation"			REAL,
	"roi"				REAL,
	PRIMARY KEY("id")
);
