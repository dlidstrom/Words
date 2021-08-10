-- \copy permutation(normalized, permutation) from 'C:\Programming\word_permutations.csv' with (format csv, header, encoding 'utf8', delimiter ';');
CREATE TABLE "public"."permutation" (
    permutation_id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    normalized TEXT NOT NULL,
    permutation TEXT NOT NULL
);
CREATE INDEX permutation_normalized_idx ON permutation (normalized);
