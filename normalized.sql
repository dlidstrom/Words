-- \copy normalized(normalized, original) from 'C:\Programming\normalized_to_originals.csv' with (format csv, header);
CREATE TABLE "public"."normalized" (
    normalized_id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    normalized VARCHAR(255) NOT NULL,
    original VARCHAR(255) NOT NULL
);
CREATE INDEX normalized_normalized_idx ON normalized (normalized);
