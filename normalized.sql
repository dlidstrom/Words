-- \copy normalized(normalized, original) from 'C:\Programming\normalized_to_originals.csv' with (format csv, header, encoding 'utf8', delimiter ';');
CREATE TABLE "public"."normalized" (
    normalized_id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    normalized TEXT NOT NULL,
    original TEXT NOT NULL
);
CREATE INDEX normalized_normalized_idx ON normalized (normalized);
