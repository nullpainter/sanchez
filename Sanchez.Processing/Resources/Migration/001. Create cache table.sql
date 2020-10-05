CREATE TABLE IF NOT EXISTS UnderlayCache(
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Filename TEXT NOT NULL,
  Longitude NUMERIC,
  Configuration TEXT NOT NULL
);

CREATE UNIQUE INDEX CacheFilename_IX ON UnderlayCache(Filename);
