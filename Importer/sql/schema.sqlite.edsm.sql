
CREATE TABLE IF NOT EXISTS 'edsm_system'(
    'id' INTEGER,
    'id64' INTEGER PRIMARY KEY,
    'name' TEXT NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS 'edsm_system_faction'(
    'id' INTEGER,
    'systemId64' INTEGER,
    'name' TEXT,
    'allegiance' TEXT,
    'government' TEXT,
    'influence' REAL,
    'state' TEXT,
    'activeStates' TEXT,
    'recoveringStates' TEXT,
    'pendingStates' TEXT,
    'happiness' TEXT,
    'isPlayer' INTEGER,
    'lastUpdate' INTEGER,
    PRIMARY KEY ('id', 'systemId64'),
    FOREIGN KEY ('systemId64') REFERENCES 'edsm_system'('systemId64')
);

-- CREATE TABLE IF NOT EXISTS 'edsm_faction_states'();

CREATE INDEX IF NOT EXISTS 'edsm_system_name' ON 'edsm_system'('name');
CREATE INDEX IF NOT EXISTS 'edsm_system_faction_name' ON 'edsm_system_faction'('name');
