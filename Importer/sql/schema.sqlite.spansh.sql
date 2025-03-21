
CREATE TABLE IF NOT EXISTS 'spansh_system'(
    'id64' INTEGER PRIMARY KEY,
    'name' TEXT NOT NULL UNIQUE,
    'x' REAL,
    'y' REAL,
    'z' REAL,
    'allegiance' TEXT,
    'government' TEXT,
    'primaryEconomy' TEXT,
    'secondaryEconomy' TEXT,
    'security' TEXT,
    'population' INTEGER,
    'bodyCount' INTEGER,
    'controllingFactionName' TEXT,
    'controllingFactionGovernment' TEXT,
    'controllingFactionAllegiance' TEXT,
    'controllingPower' TEXT,
    'powers' TEXT,
    'powerState' TEXT,
    'date' INTEGER,
    FOREIGN KEY ('controllingFactionName') REFERENCES 'spansh_system_faction'('name')
);

CREATE TABLE IF NOT EXISTS 'spansh_body'(
    'id64' INTEGER PRIMARY KEY,
    'systemId64' INTEGER NOT NULL,
    'bodyId' INTEGER,
    'name' TEXT NOT NULL,
    'type' TEXT,
    'subType' TEXT,
    'distanceToArrival' REAL,
    FOREIGN KEY ('systemId64') REFERENCES 'spansh_system'('id64')
);

CREATE TABLE IF NOT EXISTS 'spansh_system_faction'(
    'name' TEXT,
    'systemId64' INTEGER,
    'allegiance' TEXT,
    'government' TEXT,
    'influence' REAL,
    'state' TEXT,
    PRIMARY KEY ('name', 'systemId64'),
    FOREIGN KEY ('systemId64') REFERENCES 'spansh_system'('id64')
);

CREATE INDEX IF NOT EXISTS 'spansh_system_name' ON 'spansh_system'('name');
CREATE INDEX IF NOT EXISTS 'spansh_system_coords' ON 'spansh_system'('z', 'y', 'z');

CREATE INDEX IF NOT EXISTS 'spansh_body_systemId64' ON 'spansh_body'('systemId64');
CREATE INDEX IF NOT EXISTS 'spansh_body_name' ON 'spansh_body'('name');
