USING PERIODIC COMMIT
LOAD CSV FROM "file:///lmm.csv" AS row FIELDTERMINATOR ';'
MERGE (lmm:LegemiddelMerkevare {Id: row[0]}) ON CREATE SET lmm.Id = row[0], lmm.NavnFormStyrke = row[1];
CREATE INDEX ON :LegemiddelMerkevare(Id);
CREATE INDEX ON :LegemiddelMerkevare(NavnFormStyrke);
CREATE INDEX ON :LegemiddelVirkestoff(Id);
CREATE INDEX ON :LegemiddelVirkestoff(NavnFormStyrke);

USING PERIODIC COMMIT
LOAD CSV FROM "file:///vms.csv" AS row FIELDTERMINATOR ';'
CREATE (:VirkestoffMedStyrke {Id: row[0], Stoff: row[1], Styrke: row[2], AlternativStyrke: row[3], AtcKombipreparat: row[4]});
CREATE INDEX ON :VirkestoffMedStyrke(Id);
CREATE INDEX ON :VirkestoffMedStyrke(Stoff);

USING PERIODIC COMMIT
LOAD CSV FROM "file:///lmm.csv" AS row FIELDTERMINATOR ';'
MATCH (lmm:LegemiddelMerkevare {Id: row[0]})
MATCH (vms:VirkestoffMedStyrke {Id: row[3]})
MERGE (lmm)-[h:har]->(vms)
ON CREATE SET h.Sortering = row[2];

USING PERIODIC COMMIT
LOAD CSV FROM "file:///lmv.csv" AS row FIELDTERMINATOR ';'
MERGE (lmm:LegemiddelVirkestoff {Id: row[0]}) ON CREATE SET lmm.Id = row[0], lmm.NavnFormStyrke = row[1];

USING PERIODIC COMMIT
LOAD CSV FROM "file:///lmv.csv" AS row FIELDTERMINATOR ';'
MATCH (lmv:LegemiddelVirkestoff {Id: row[0]})
MATCH (vms:VirkestoffMedStyrke {Id: row[3]})
MERGE (lmv)-[h:har]->(vms)
ON CREATE SET h.Sortering = row[2];

MATCH (vfga:VFGA),(vms:VirkestoffMedStyrke)
WHERE  vms.Id = vfga.VFGAID
CREATE (vms)-[:CONNECTED_TO]->(vfga)