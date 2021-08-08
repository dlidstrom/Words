-- \copy permutation(normalized, permutation) from 'C:\Programming\word_permutations.csv' with (format csv, header);
CREATE TABLE "public"."permutation" (
    permutation_id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    normalized VARCHAR(255) NOT NULL,
    permutation VARCHAR(255) NOT NULL
);
CREATE INDEX permutation_normalized_idx ON permutation (normalized);
