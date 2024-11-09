/* */

CREATE TABLE IF NOT EXISTS `profilepreference` (
	`Id`									INTEGER NOT NULL,
	`profileId`								TEXT NOT NULL,
	`parkonwait`							INTEGER DEFAULT 0,
	`enableSmartPlanWindow`					INTEGER DEFAULT 1,
	`exposurethrottle`						REAL DEFAULT 125,
	`enableDeleteAcquiredImagesWithTarget`	INTEGER DEFAULT 1,
	`delayGrading`							REAL DEFAULT 80,
	`acceptimprovement`						INTEGER DEFAULT 1,
	`maxGradingSampleSize`					INTEGER,
	`enableMoveRejected`					INTEGER DEFAULT 0,
	`enableGradeRMS`						INTEGER,
	`enableGradeStars`						INTEGER,
	`enableGradeHFR`						INTEGER,
	`enableGradeFWHM`						INTEGER DEFAULT 0,
	`enableGradeEccentricity`				INTEGER DEFAULT 0,
	`rmsPixelThreshold`						REAL,
	`detectedStarsSigmaFactor`				REAL,
	`hfrSigmaFactor`						REAL,
	`fwhmSigmaFactor`						INTEGER DEFAULT 4,
	`eccentricitySigmaFactor`				INTEGER DEFAULT 4,
	`enableSynchronization`					INTEGER DEFAULT 0,
	`syncWaitTimeout`						INTEGER DEFAULT 300,
	`syncActionTimeout`						INTEGER DEFAULT 300,
	`syncSolveRotateTimeout`				INTEGER DEFAULT 300,
	`syncEventContainerTimeout`				INTEGER DEFAULT 300,
	PRIMARY KEY(`id`)
);

CREATE TABLE IF NOT EXISTS `acquiredimage` (
	`Id`			INTEGER NOT NULL,
	`projectId`		INTEGER NOT NULL,
	`targetId`		INTEGER NOT NULL,
	`acquireddate`	INTEGER,
	`filtername`	TEXT NOT NULL,
	`accepted`		INTEGER NOT NULL,
    `rejectreason`	TEXT,
    `metadata`		TEXT NOT NULL,
	PRIMARY KEY(`Id`)
);

